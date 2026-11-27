# MITSU 鶴見噴印控制台（新版）— 通訊層與模擬器

| 專案 | 說明 |
|---|---|
| `src/Mitsu.Printer` | X1/SPH 通訊層（.NET 8 類別庫）。raw TCP :25001，Anser 協定（FC 0x64 + CMD） |
| `src/Mitsu.Printer.Simulator` | 噴碼機模擬器（同協定的 TCP server），供無機台開發與自動測試 |
| `src/Mitsu.X1Tool` | 命令列工具 `x1tool`：`sim` 跑模擬器、`probe` 連實機做 P0 驗證 |
| `tests/Mitsu.Printer.Tests` | 位元組比對測試＋用戶端對模擬器的端對端測試（含故障注入） |
| `src/Mitsu.Core` | 核心邏輯：工單驗證、機番產生、每箱 18 欄組字、每箱狀態、生產控制流程（無 UI、無 DB） |
| `tests/Mitsu.Core.Tests` | 核心邏輯單元測試＋生產流程對模擬器的端對端測試 |

協定依據：`analysis/01_X1_protocol.md`（由 X1SDKDemo 反推，**尚未經實機驗證**）。

## 建置與測試

.NET 8 SDK 裝在 `%USERPROFILE%\.dotnet`（未加入 PATH）：

```powershell
$env:DOTNET_ROOT="$env:USERPROFILE\.dotnet"; & "$env:USERPROFILE\.dotnet\dotnet.exe" test D:\dev_program\MITSU\Mitsu.sln
```

## 通訊層設計（相對於原廠 SDK 的改進）

| 原廠 SDK 問題 | 新通訊層 |
|---|---|
| 一次 socket Read 當成一個封包 | `FrameDecoder` 依 Length 欄位重組，處理分包/黏包 |
| 沒有逐命令逾時，掉一個回應整條佇列永久卡死 | 每個請求有逾時；逾時即斷線並讓呼叫端得到 `X1TimeoutException` |
| 回應只照 FIFO 配對，不比對 CMD | 比對 CMD（20h 另比對子命令）；其他封包走 `UnsolicitedFrame` 事件，不會吃掉回應 |
| 參數錯誤的例外發生在入佇列之後 → 卡死 | 所有驗證在送出前完成（20 個字串、各 ≤255 bytes、檔名 ASCII/長度、封包大小） |
| 多執行緒共用旗標無 lock | 單一請求鎖（SemaphoreSlim）序列化；狀態集中在一個 lock |
| 60 秒 watchdog 不會重連 | 閒置心跳（43h），失敗即斷線並觸發 `Disconnected`；重連由上層決定（斷線後須先確認工單狀態） |
| 依賴 ICMP ping | 不使用 ICMP；啟用 TCP keepalive |

主要 API（`X1Client`，皆為 async）：
`ConnectAsync / DisconnectAsync`、`GetProtocolVersionAsync`(43h)、`GetAlarmStatusAsync`(7Fh)、`GetPrintingStatusAsync`(45h)、
`GetCounterAsync`(3Ah)、`StartLineAsync / StopLineAsync`(20h+203h)、`SelectMessageAsync`(20h+214h)、
`SelectMessageWhilePrintingAsync`(20h+24Ch)、`PushDynamicVarsAsync`(CFh)、`SetDynamicVarTableAsync`(CAh)、
`ClearDynamicVarBufferAsync`(CBh)、`GetDynamicVarBufferCountAsync`(CCh)、`SendRawAsync`。
NAK(31h) 一律轉成 `X1NakException`（含錯誤碼與說明）。

## 核心邏輯（Mitsu.Core）

| 類別 | 作用 | 依據／與舊版差異 |
|---|---|---|
| `ProductSpec` / `CartonSpec` | PrintData、CartonSize 一列 | 欄位同客戶 Excel |
| `JobSpec.Validate` | 開印前驗證：必填、箱數 < 總箱數、各欄長度上限、紙箱編號一致、印字終了 ≤ L+W、開始 ≤ 終了 | 長度上限取舊 Config.xml 預設值（`FieldLimits` 可調） |
| `MachineNumberSequence` | 機番 = 前綴 + 固定位數流水號，順序/逆序 | **字串保存、保留前導 0**（舊版用 double 會掉 0）；以「逆序不可 < 0、順序不可超出位數」取代 2.1.2.6 被 20160530 拿掉的錯誤規則 |
| `CartonPayloadComposer` | 每箱 18 欄 → CFh 的 String1–18（19、20 空） | 順序同舊 ADT 欄位；電壓＋5 空白＋頻率、CODE_value＋3 空白＋機番（條碼）；值空白時不單獨印單位 |
| `ProductionJob` | 每箱狀態：Sent → Printed / Voided / Uncertain | 機番在建立該箱時決定；**只有噴碼機確認噴出才算 Printed** |
| `ProductionController` | 一張工單的完整流程（單一 async 迴圈，無跨執行緒共用計數） | 見下 |

`PayloadField` 列舉就是 X1 訊息設計時「哪個物件綁哪個 String」的對照表（String 編號 = 列舉值 + 1）。

### 生產流程

```
開始：7Fh 無警報 → 停線 → 選訊息 → CBh 清緩衝 → 預送 BufferTarget 箱 → 啟動產線 → 讀 45h 取計數基準
迴圈（每 PollInterval）：
  每個噴印目標讀 CCh → 已消耗 = 已送 − 緩衝筆數 → 依先進先出確認該箱「這一面已噴」
  所有面都確認 → 該箱 Printed（事件 CartonPrinted）
  檢查：計數器跑得比消耗快（噴了沒資料的箱）→ 停機；正反面差 > MaxTargetSkew → 停機；每 N 次查 7Fh 警報
  補送到 BufferTarget
結束：全部確認 → 停線 → Completed
操作員停止：停線 → 最後一次讀 CCh → 清緩衝 → 未噴的箱 Voided（號碼可重用）
異常：盡量停線；若能確認停線＋清緩衝，未噴的箱 Voided，否則（斷線等）在途的箱全標 Uncertain
```

結果 `ProductionResult` 提供 `ResumePoint`（下次開印要填的「箱數」與「機番流水號」）與 `Uncertain` 清單。
**Uncertain 的箱必須讓操作員決定**（例如一面已噴、另一面沒噴；或斷線時不知道緩衝裡的有沒有噴出），
這正是 2013–2015 年跳號與正反面機番不一致客訴的情境，新版不再默默略過。

多個噴印目標（例如表面、裏面各一個 line/station 或各一台噴碼機）用 `PrintTarget` 列出，同一箱的值會送到每個目標。

### 依賴 P0 驗證的部分

- 「已噴」的判斷建立在 **CCh = 緩衝內尚未噴的筆數** 這個假設上（集中在 `ProductionController.PollTargetsAsync`）。實機若不是這樣，只需改這一處。
- `BufferTarget` 不可大於實際緩衝深度；計數器交叉檢查只對「一條產線只有一個目標」的情況有效（45h 計數是以產線為單位）。

## 模擬器的假設（P0 驗證後要回頭調整 `X1SimulatorOptions`）

| 選項 | 目前假設 | 待實機確認 |
|---|---|---|
| `BufferCapacity` | CFh 為 FIFO，深度 10 | 是否 FIFO、實際深度 |
| `BufferFullErrorCode` | 滿了回 09h | 實際錯誤碼 |
| `RequireMessageForDynamicVars` | 未選訊息時 CFh 回 11h | 是否有此限制 |
| `OnEmptyBuffer` | 緩衝空時照樣噴（空白） | 實際行為（空白／沿用上一筆／不噴／報警） |
| `EchoTransactionId` | 回顯 transaction id | 是否回顯（決定 `UseTransactionIds` 能否開啟） |
| `StatusBlockSize` | 45h 每區塊 68 bytes | 56 或 68 |

## 實機 P0 驗證步驟（`x1tool probe`）

所有收發封包會以 hex 記錄到 log 檔，驗證完把 log 一起保存。

```powershell
D:\dev_program\MITSU\src\Mitsu.X1Tool\bin\Debug\net8.0\x1tool.exe probe 192.168.x.x --log p0.log
```

1. `ver`、`name`、`alarm`、`status` — 基本通訊、45h 區塊大小（`block=`）。
2. `select 1 1 MSG0000x.xml` — 選一個訊息中引用了動態變數的訊息。
3. 先**不選訊息**對另一工位 `cf 1 2 test` — 確認是否要求先選訊息。
4. `fifo 1 1 50` — CFh 是否累加、深度多少、滿了回哪個錯誤碼；之後 `cb 1 1`。
5. `cf 1 1 AAA|BBB` 兩三筆 → `start 1` → 過箱或手動觸發 → `watch 1 1 30`：
   每噴一次 CCh 是否減 1、45h 計數是否加 1、印出內容是否依序。
6. 緩衝清空後再觸發一次：觀察實際噴出什麼（空白？上一筆？不噴？報警？）。
7. `cf 1 1 X||Z`（中間空字串）：空字串是清空還是保留原值。
8. `stop 1` 後 `status`：停機後是否就讀不到計數（文件推測如此）。
9. 用 `--tid` 重連再做一次 `ver`：看 log 裡回應的前兩個 byte 是否回顯 transaction id。
10. 故意送錯（例如 `select 9 9 NOFILE.xml`）：記錄錯誤碼，確認 0x10–0x14 是十六進位還是十進位。

## 本機用模擬器試玩

```powershell
# 視窗 1：模擬器（每 1 秒自動觸發一次噴印）
D:\dev_program\MITSU\src\Mitsu.X1Tool\bin\Debug\net8.0\x1tool.exe sim --auto-ms 1000
# 視窗 2：連上去操作
D:\dev_program\MITSU\src\Mitsu.X1Tool\bin\Debug\net8.0\x1tool.exe probe 127.0.0.1
```
