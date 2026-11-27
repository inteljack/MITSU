# NexGen X1 / SPH 噴印機 TCP 控制協定參考（由 X1SDKDemo C# SDK 反推）

來源（唯讀）：`D:\dev_program\MITSU\X1SDKDemo_20261012\X1SDKDemo`
本文所有結論都來自程式碼，行號格式為 `檔案:行`。檔名縮寫：

| 縮寫 | 檔案 |
|---|---|
| `TCP` | `X1LinkLibrary\CTcpClient.cs` |
| `LIB` | `X1LinkLibrary\X1LinkLib.cs` |
| `SDK` | `X1LinkLibrary\X1SDK.cs` |
| `F1`  | `X1SDKDemo\Form1.cs` |
| `F1D` | `X1SDKDemo\Form1.Designer.cs` |

> 標示 **【推論】** 者是由命名或結構推得、程式碼沒有直接證明；標示 **【不明】** 者是程式碼本身互相矛盾或資訊不足。SDK 原始碼內沒有協定規格書（目錄裡也找不到 .pdf/.doc），註解中提到的章節號（如 "15.13"、"9.11"、"7.3"）指向一份我們手上沒有的 Anser 協定文件。

---

## 0. 先講最重要的結論

1. **0xCA / 0xCF 不是 Modbus function code。** Modbus 層的 function code 固定是 **0x64（=100，Modbus 規範的 user-defined 區段）**（`LIB:1330`）。0xCA/0xCF 是包在 0x64 PDU 裡面的 **Anser 命令碼（CMD byte）**（`LIB:1333`、`LIB:1064`、`LIB:1116`）。
2. 這**不是標準 Modbus TCP**：PDU 內有自訂的 `[PDU_Length u16][ST u8][CMD u8]` 子表頭（`LIB:1350-1357`），且 PDU 長度遠超 Modbus 規範上限 253 bytes（例如 `20h+214h` 的 PDU 為 264 bytes，CA/CF 最大可達約 5 KB，上傳封包 4100 bytes）。**一般 Modbus 函式庫（NModbus、pymodbus 等）不能直接用**，請以 raw TCP socket 自行組框。
3. 埠號 **25001**（`LIB:112`），不是 Modbus 標準的 502。
4. 所有多位元組整數皆為 **Big-endian**（`LIB:1286-1308`），唯一例外是上傳資料封包 7Eh 的 packet number 為 little-endian（`LIB:791-796`，SDK 自己註明是保留舊行為）。
5. 字串編碼：CA/CF 變數值與檔名為 **UTF-8**（`LIB:1022`、`LIB:1033`、`LIB:1088`）；`20h+214h` 訊息檔名為 **ASCII**（`LIB:987`）；`20h+24Ch` 為 UTF-8（`LIB:946`）；78h 檔名為 UTF-8（`LIB:445`）。整個 SDK **沒有任何 Big5 或 UTF-16**。
6. SDK 是 **嚴格一問一答、FIFO 佇列配對**，沒有 transaction id 配對（transaction id 固定 0，`LIB:1326`），也**沒有單一命令逾時**——只要一個回應沒回來，整個佇列就永久卡住（見 §5）。

---

## 1. 傳輸層

### 1.1 連線

| 項目 | 值 / 行為 | 出處 |
|---|---|---|
| 協定 | TCP client，IPv4 | `TCP:57`、`TCP:68` |
| 埠 | 25001（寫死） | `LIB:112` |
| 連線逾時 | 5000 ms（`BeginConnect` + `WaitOne`） | `LIB:112`、`TCP:68-79` |
| 連線前檢查 | 先送 ICMP ping，逾時 150 ms，失敗就不連（ICMP 被擋的網段會連不上） | `SDK:276`、`SDK:854-872` |
| Nagle | `NoDelay = true` | `TCP:64` |
| ReceiveTimeout | 1000 ms（但接收執行緒用 `DataAvailable` 輪詢，實際上不觸發） | `TCP:81`、`TCP:141` |
| TCP keepalive (SO_KEEPALIVE) | 未設定 | — |
| 同時只能一條連線 | `LinkToPrinter` 若已連線直接回 false | `SDK:273-274` |

### 1.2 框架格式（ADU）

`makeADU()`（`LIB:1323-1365`）產生的格式，前 8 bytes 與 Modbus TCP MBAP + FC 相同，之後是 Anser 自訂子表頭：

| Offset | 長度 | 欄位 | 值 | 出處 |
|---|---|---|---|---|
| 0 | 2 | Transaction ID | 固定 `00 00` | `LIB:1326`、`LIB:1341` |
| 2 | 2 | Protocol ID | 固定 `00 00` | `LIB:1327`、`LIB:1344` |
| 4 | 2 | Length（BE）= 6 + N | 其後所有 bytes 數（Unit 1 + FC 1 + PDU_Len 2 + ST 1 + CMD 1 + data N） | `LIB:1328`、`LIB:1347` |
| 6 | 1 | Unit ID（SDK 叫 PrinterID） | 固定 `00` | `LIB:1329`、`LIB:1350` |
| 7 | 1 | Function code | 固定 `64` | `LIB:1330`、`LIB:1351` |
| 8 | 2 | PDU_Length（BE）= 2 + N | ST + CMD + data | `LIB:1331`、`LIB:1353` |
| 10 | 1 | ST | 送出固定 `00`；回應中的值 SDK 讀了但**完全不使用**（意義不明） | `LIB:1332`、`LIB:1206` |
| 11 | 1 | CMD | Anser 命令碼（32h、CAh、CFh、20h…） | `LIB:1333`、`LIB:1357` |
| 12 | N | data | 命令參數 | `LIB:1359-1360` |

最短框（無參數命令）= 12 bytes，例如 `00 00 00 00 00 06 00 64 00 02 00 43`。

### 1.3 回應解析與配對

* 接收由獨立執行緒 `ListenForPackets()` 處理（`TCP:128-210`）：`DataAvailable` 時做一次 `Read(buffer, 0, 5120)`（`TCP:146`），**直接把這一次 Read 的結果當成一個完整框**丟給 `ReadData`（`TCP:157`、`LIB:1173`）。
  * 沒有依 Length 欄位重組；TCP 分段（一個框分兩次到）或黏包（兩個框一次到）都會解析錯誤。
* `ReadData` 依同樣表頭解析（`LIB:1189-1207`），offset 12 以後當 data（`LIB:1209-1216`）。**不檢查** transaction id、protocol id、Length、unit id、function code。
* 依回應 CMD 分派（`LIB:1218-1260`）：
  * `31h` = NAK/錯誤，data[0] = 錯誤碼（無 data 時 SDK 以 `0xFF` 代表）→ `OnExcuteAckFail`（`LIB:1220-1245`）。
  * `4Fh` = ACK（無資料）→ 上傳中則推下一包（`LIB:1248-1249`），否則 `OnExcuteAck`（`LIB:1252-1253`）。
  * 其他 = 有資料的回應，CMD 通常與請求相同（例如 32h 請求回 32h）→ `OnExcuteReturnPduData(CMD, data)`（`LIB:1257-1258`）。
* 配對方式：`X1SDK.getPDU()` 收到**任何**回應都把佇列頭 dequeue 當作「這個回應對應的請求」（`SDK:445-452`）。**不比對 CMD**。所以若印表機主動推送（例如 `30h PrintCompletedReport`，`SDK:115`、`SDK:465-472`，【推論】名稱看起來像是列印完成主動回報），就會錯誤地吃掉佇列頭命令，之後所有配對全部錯位。

### 1.4 一問一答（流量控制）

* `addToCmdQueue()`：佇列為空才立刻送出，否則只排隊（`SDK:878-892`）。
* 收到回應後，若處理前佇列長度 > 1，送出新的佇列頭（`SDK:748-758`）。
* 因此任何時刻最多只有 1 個未回應的請求在線上。上傳檔案時 78h 留在佇列頭直到整個傳輸完成（`SDK:401-413`）。

### 1.5 Keepalive / 斷線偵測

| 機制 | 行為 | 出處 |
|---|---|---|
| Watchdog timer | 每 15000 ms 觸發 | `SDK:49`、`SDK:260-261` |
| 應用層 keepalive | 若佇列為空，排入 `43h GetAnserProtocolVersion`（帶第二參數 `"timer"`） | `SDK:344-345` |
| keepalive 回應 | 因 `CommandSend.Length == 2`，43h 回應**不會**觸發 `OnX1Respone`（靜默） | `SDK:546-554` |
| 60 秒無回應 | 只是 `watchdog_timer.Stop()`；`lib.CloseLink()` 被註解掉，**不會斷線重連** | `SDK:45`、`SDK:347-355` |
| socket 層 | 閒置 > 1000 ms 時 `Poll(0, SelectRead)` + `Peek`，讀到 0 bytes 視為對方關閉 | `TCP:164-183` |
| 斷線事件 | 接收執行緒結束時呼叫 `OnDisconnect` → `X1SDK.OnDisConnected` | `TCP:208-209`、`LIB:136-140`、`SDK:316-320` |

結論：只能偵測到對方正常關閉 / RST；拔線等半開連線要等到寫入失敗才會發現，而寫入失敗會直接丟 `IOException` 給呼叫端（`TCP:232` 沒有 try/catch）。

### 1.6 執行緒模型

| 執行緒 | 做什麼 |
|---|---|
| 呼叫端（Demo 中為 UI thread） | `addToCmdQueue` → 佇列空時在 `cmd_queue_lock` 內直接 `RequestCommand` → socket write（`SDK:880-888`） |
| TCP 接收執行緒 `ListenForPackets` | 解析、觸發 `OnX1Respone`、**並從這條執行緒送出下一個佇列命令**（`SDK:757`）以及上傳的每個 7Eh 封包（`LIB:1249`、`LIB:811`） |
| `System.Timers.Timer` 執行緒池 | keepalive 排隊與 `bHasAck/elpased_time` 修改（`SDK:336-358`） |

`OnX1Respone` 在 TCP 接收執行緒上被呼叫；Demo 用 `Task.Run` + `Invoke` 轉回 UI 執行緒（`F1:116-123`）。若事件處理器阻塞，整條接收執行緒（含下一個命令的送出）都會停住。

### 1.7 單框大小上限

`CTcpClient` 寫入緩衝區 5120 bytes（`TCP:30`）。`AddToPacket` 在資料超過剩餘空間時先 Flush，但若單一 ADU 本身 > 5120，`Array.ConstrainedCopy` 會丟例外（`TCP:218-223`）。因此 **SDK 實際能送的最大 ADU = 5120 bytes**（PDU data ≤ 5108）。

---

## 2. SDK 外層 API 與回應事件

* 公開 API 全部是非同步：呼叫只排隊，結果透過 `event X1Respone OnX1Respone(params object[] list)` 回來（`SDK:14`、`SDK:61`）。
* `list[0]` = 命令名稱字串（`x1command` 對照表，`SDK:113-139`），`list[1]` = `"OK"` 或 `"NG"`（`SDK:65-69`），其後為解析出的欄位。
* `4Fh` ACK 會轉成 `(x1command[佇列頭命令], "OK")`（`SDK:363-367`、`SDK:524-529`）。
* `31h` 錯誤會轉成 `(x1command[佇列頭命令], "NG", 錯誤說明)`（`SDK:373-382`、`SDK:474-510`）。
* 子命令（20h+xxxh）在佇列中以 24-bit key 表示，例如 `0x200203`（`SDK:136-139`、`SDK:945-952`）。

### 2.1 一般錯誤碼（31h data[0]）

`err_report`（`SDK:141-155`）：

| 碼 | 說明 |
|---|---|
| 00h | Data length beyond |
| 01h | Invalid message no. |
| 02h | Invalid production line index |
| 03h | Invalid print station index |
| 04h | Invalid content |
| 05h | Ink not ready |
| 06h | Printhead already in use by another production line |
| 07h | Incorrect production line status |
| 08h | PrintStation filename not found |
| 09h | Parameter out of range |
| 10h | Invalid operation |
| 11h | Select message before print |
| 12h | Fail to create the file |
| 13h | No file found |
| 14h | file size incorrect |

**【不明】** 碼從 09h 直接跳到 10h，沒有 0Ah–0Fh。很可能原始文件寫的是十進位 10–14，被誤寫成 `0x10`–`0x14`；實機若回 `0A`–`0E` 要考慮這個可能。SDK 對表外碼只輸出 `"error code:0xNN"`（`SDK:487`）。

78h/7Eh 上傳專用錯誤碼 `err_78_report`（`SDK:157-167`）：13h 找不到檔 / 14h 檔名 > 128 bytes 或傳輸量超過宣告 / 15h 檔案型別錯 / 16h 大小為 0 / 17h 取得剩餘空間失敗 / 18h 空間不足 / 19h 取得大小上限失敗 / 1Ah 不支援此型別 / 1Bh 超過該型別大小上限。其中 13h、14h、1Bh 也可能由 SDK 本地產生（`LIB:423`、`LIB:432`、`LIB:448`）。

`7Fh`/`A7h` 的警報碼 `err_7f_report`（`SDK:169-178`）：0 No Error、1 Ink low、2 Ink empty、3 Ink type not support、4 Ink color not support、5 EDC not match、6 No cartridge、7 Gate not close、8 DISC error、9 Error。

---

## 3. 命令總表（每一個 SDK 有實作的命令）

所有範例框皆依 `makeADU` 邏輯計算，Transaction/Protocol/Unit 均為 0。以下「Demo」欄為 `Form1.cs` 的按鈕處理函式。

### 3.1 無參數查詢命令

| CMD | 名稱 | LIB 方法 | SDK 方法 | Demo | 請求框 |
|---|---|---|---|---|---|
| 32h | GetMessageNo | `LIB:145-149` | `SDK:967-970` | `F1:191-195` | `00 00 00 00 00 06 00 64 00 02 00 32` |
| 33h | GetIPAddress | `LIB:181-185` | `SDK:987-990` | `F1:247-251` | `00 00 00 00 00 06 00 64 00 02 00 33` |
| 34h | GetSystemClock | `LIB:259-263` | `SDK:1282-1285` | `F1:297-301` | `00 00 00 00 00 06 00 64 00 02 00 34` |
| 42h | GetPrinterInformation | `LIB:829-833` | `SDK:957-960` | `F1:185-189` | `00 00 00 00 00 06 00 64 00 02 00 42` |
| 43h | GetAnserProtocolVersion | `LIB:163-167` | `SDK:962-965` | `F1:179-183` | `00 00 00 00 00 06 00 64 00 02 00 43` |
| 45h | GetPrintingStatus | `LIB:856-860` | `SDK:982-985` | `F1:530-534` | `00 00 00 00 00 06 00 64 00 02 00 45` |
| 7Fh | GetPrinterAlarmStatus | `LIB:154-158` | `SDK:972-975` | `F1:197-201` | `00 00 00 00 00 06 00 64 00 02 00 7F` |
| A7h | GetPrinterStatus | `LIB:172-176` | `SDK:977-980` | `F1:241-245` | `00 00 00 00 00 06 00 64 00 02 00 A7` |
| BEh | GetSystemMeasurementUnit | `LIB:190-194` | `SDK:991-994` | `F1:261-265` | `00 00 00 00 00 06 00 64 00 02 00 BE` |
| D3h | GetPrinterName | `LIB:847-851` | `SDK:1287-1290` | `F1:303-307` | `00 00 00 00 00 06 00 64 00 02 00 D3` |

回應 data 版面（offset 自 data 起算，皆 BE）：

* **32h**（`SDK:531-540`）：data 長 4 → `int32 messageNo`；否則兩個 `int32`（offset 0、4）。**【不明】** 第二個 int 的意義（可能是兩條產線各自的訊息號）。
* **33h**（`SDK:512-514`）：整段 ASCII 字串（IP）。
* **34h**（`SDK:516-518`）：`[0..1] year int16`、`[2] month`、`[3] day`、`[4] hour`、`[5] minute`、`[6] second`。
* **42h**（`SDK:542-544`）：`[0..3]` ASCII、`[4..7]` ASCII（顯示為 "A-B"，去除前導 0x00）、`[8..11]` 四個 byte 版本號 `{0:00}.{1:000}.{2:00}.{3:000}`、`[12]` int32、`[20]`、`[16]` int32（顯示為 "[20]-[16]"）、`[24]`、`[28]`、`[32]`、`[36]`、`[40]` 各 int32、`[44..47]` IPv4。各欄位語意 SDK 未命名。
* **43h**（`SDK:546-554`）：`[0].[1].[2]` 三個 byte 版本號。
* **45h**：見 §5.1。
* **7Fh**（`SDK:639-641`）：`[0]` 噴頭 0 警報碼、`[1]` 噴頭 1 警報碼（查 `err_7f_report`）。
* **A7h**（`SDK:643-669`）：約 112 bytes。`[0..27]` 同 42h 前段；`[28]`/`[29]` 兩噴頭警報碼；`[30..70]` 為噴頭 0 墨匣資訊、`[71..111]` 為噴頭 1，兩段格式與 86h 回應相同（偏移 +30 / +71）。
* **BEh**（`SDK:687-689`）：`[0]` 0 = mm、1 = inch。
* **D3h**（`SDK:691-693`）：整段 ASCII 印表機名稱。

### 3.2 帶參數命令

#### BFh SetSystemMeasurementUnit
* LIB `LIB:200-206`、SDK `SDK:1019-1022`、Demo `F1:267-273` + `FormUnitSelect.cs:20-30`。
* data：`[0] u8` 0 = mm、1 = inch。
* 範例（inch）：`00 00 00 00 00 07 00 64 00 03 00 BF 01`
* 回應：4Fh ACK / 31h。

#### C4h ResetSystem
* LIB `LIB:218-224`、SDK `SDK:1263-1266`、Demo `F1:275-283` + `FormResetSystem.cs:21-59`。
* data：`[0] u8` 位元旗標，**表示要保留的資料**：bit0 = message、bit1 = device setting、bit2 = system setting、bit3 = backup files（`LIB:211-217`）。
* 範例（保留訊息 + 裝置設定）：`00 00 00 00 00 07 00 64 00 03 00 C4 03`
* 注意：`FormResetSystem` 取消勾選用 XOR（`FormResetSystem.cs:28`），沒問題但脆弱。

#### 35h SetSystemClock
* LIB `LIB:235-254`、SDK `SDK:1277-1280`、Demo `F1:285-295`。
* data（7 bytes）：`[0..1] year u16 BE`、`[2] month`、`[3] day`、`[4] hour`、`[5] minute`、`[6] second`。
* 範例 2026-11-27 13:45:30：`00 00 00 00 00 0D 00 64 00 09 00 35 07 EA 0B 1B 0D 2D 1E`

#### 3Ah GetCounter
* LIB 方法名稱誤為 `SetCounter`（`LIB:265-274`），實為讀取；SDK `GetCounter`（`SDK:1292-1295`）；Demo `F1:309-318` + `FormGetCounter.cs:52-66`。
* data：`[0..1] messageNo u16 BE`、`[2] counterType u8`（1 = item counter、2 = box、3 = lot，`FormGetCounter.cs:59-66`）。
* 範例（訊息 1、item counter）：`00 00 00 00 00 09 00 64 00 05 00 3A 00 01 01`
* 回應 data（`SDK:520-522`，21 bytes）：`[0..1] messageNo u16`、`[2] type`、`[3..6] value int32`、`[7..10] max int32`、`[11..14] min int32`、`[15] digits`、`[16..19] step int32`、`[20] format u8`。
* 注意：以「訊息編號」定址，不是以產線/工位；值只有 32-bit。

#### 86h GetDiscInformation
* LIB `LIB:839-845`、SDK `SDK:1248-1251`、Demo `F1:253-259` + `FormPrintSelect.cs`。
* data：`[0] printhead 0/1`。範例：`00 00 00 00 00 07 00 64 00 03 00 86 00`
* 回應（`SDK:671-685`）：`[0]` int、`[1]` 容量碼（`ink_volume`）、`[2]` 剩餘 %、`[3]` 顏色碼、`[4]` 墨水名稱碼、`[5]`/`[9]` int32、`[13]`、`[17]` int32、`[21]` int32（0 = Normal，其餘 Abnormal）、`[25]`、`[33]` int64。

#### 46h Set printing status（Start/Stop，舊式）— **只在 LIB，SDK 未公開**
* `LIB:282-300`（StartPrint）、`LIB:308-326`（StopPrint）。`X1SDK.RequestCommand` 沒有 46h 分支（`SDK:902-944`），`x1command` 也沒有 46h（`SDK:113-139`），所以透過 `X1SDK` 無法使用；ACK 回來時查 `x1command[0x46]` 也會 KeyNotFoundException（若硬塞 `SendRawData`）。
* data（7 bytes）：`[0..1] messageNo u16`、`[2] action`（0 = start、1 = stop）、`[3..4] PL index u16`、`[5..6] PS index u16`。
* 範例（start，msg 1，PL 0，PS 1）：`00 00 00 00 00 0D 00 64 00 09 00 46 00 01 00 00 00 00 01`
* 範例（stop）：`00 00 00 00 00 0D 00 64 00 09 00 46 00 01 01 00 00 00 01`

#### 7Ah StartFwUpgrade — **只在 LIB**
* `LIB:753-757`，無參數。`00 00 00 00 00 06 00 64 00 02 00 7A`。SDK 未公開（註解 `SDK:1173` 叫你呼叫但 X1SDK 沒有這個方法）。

### 3.3 擴充命令 20h + 子命令

data 前 2 bytes 為子命令（BE），之後為參數。回應 CMD 也是 20h，data 前 2 bytes 回顯子命令（`SDK:699-743`）。

#### 20h+203h SetProductionLinePrintingOperation（**開始 / 停止列印**）
* LIB `LIB:865-882`（XML 註解誤寫為 "Get printing status"）、SDK `SetProdutionLineOperation`（拼字錯誤，`SDK:1297-1309`）、Demo `F1:536-544` + `FormSelectProductionLine.cs:23-35`。
* data（5 bytes）：`[0..1] 02 03`、`[2..3] LineID u16 BE`、`[4] operation`：**0 = Start、1 = Stop（反邏輯）**（`SDK:1301-1306`、`LIB:875-878`）。
* 範例 start line 1：`00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 00`
* 範例 stop line 1：`00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 01`
* 回應：4Fh ACK（→ `"SetProductionLinePrintingOperation","OK"`）或 31h。相關錯誤碼：05h Ink not ready、06h 噴頭被另一產線占用、07h 產線狀態不正確、11h 列印前須先選訊息。
* 以「產線」為單位，不帶工位、不帶訊息號。

#### 20h+214h SetPrintStationPrintMessage（**選擇訊息 / 工作**，停機時）
* LIB `LIB:973-998`、SDK `SetPrintMessage`（`SDK:1311-1322`）、Demo `F1:546-556` + `FormSetPrintMessage.cs:35-41`。
* data（262 bytes 固定）：`[0..1] 02 14`、`[2..3] LineID u16`、`[4..5] StationID u16`、`[6..261] 訊息檔名`，**ASCII**，右側補 0 至 256 bytes（`LIB:987-994`）。
* 範例（PL 1、PS 1、`MSG00003.xml`）：
  `00 00 00 00 01 0C 00 64 01 08 00 20 02 14 00 01 00 01 4D 53 47 30 30 30 30 33 2E 78 6D 6C 00 …(補 0 至共 274 bytes)`
* 注意：非 ASCII 字元會變成 `?`；檔名 > 256 bytes 時 `Array.Copy` 丟例外（且在佇列內，見 §5）。
* 檔名格式來自 `Form1.cs:343` 的註解範例 `MSG00003.xml`。

#### 20h+24Ch SetPrintMessageWhilePrint（**列印中切換訊息**）
* LIB `LIB:927-966`、SDK `SDK:1341-1353`、Demo `F1:595-605`。
* data：`[0..1] 02 4C`、`[2..3] LineID`、`[4..5] StationID`、`[6..7] 檔名 byte 數 u16 BE`、`[8..]` **UTF-8** 檔名（不補 0）。SDK 限制 ≤ 255 bytes（`LIB:947-950`），雖然欄位是 u16。
* 範例：`00 00 00 00 00 1A 00 64 00 16 00 20 02 4C 00 01 00 01 00 0C 4D 53 47 30 30 30 30 33 2E 78 6D 6C`
* SDK 註解寫 "subcommand 0214"（`SDK:1345`）是複製錯誤，實際送 024C（`LIB:936-937`）。

#### 20h+24Ah SetCounterWhilePrint（列印中設定計數器）
* LIB `LIB:887-922`、SDK `SDK:1324-1339`、Demo `F1:606-621` + `FormSetCounter.cs`。
* data（36 bytes）：`[0..1] 02 4A`、`[2..3] LineID`、`[4..5] StationID`、`[6] type`（1 item、2 box、3 lot）、`[7..14] value u64 BE`、`[15..22] max u64`、`[23..30] min u64`、`[31] digits`（1–11）、`[32..35] step int32`。
* 範例（PL1 PS1 item, value 100, max 999999, min 1, 6 digits, step 1）：
  `00 00 00 00 00 2A 00 64 00 26 00 20 02 4A 00 01 00 01 01 00 00 00 00 00 00 00 64 00 00 00 00 00 0F 42 3F 00 00 00 00 00 00 00 01 06 00 00 00 01`
* Bug：SDK 參數型別是 `double`/`float`，再 `Convert.ToUInt64/ToUInt32`（`SDK:1338`）；**負 step 會丟 OverflowException**，雖然註解說 step 可為 -32768~32767（`SDK:1336`）。LIB 的 `getdouble` 名稱誤導，實際寫入 u64（`LIB:1294-1300`）。

#### 只出現在 Demo 原始資料 / 註解中的 20h 子命令（`SendRawData`，`SDK:1355-1358`）

`SendRawData(byte[] d)` 會以 `d[0]` 當 CMD、其餘當 data（第一個 byte 必須是 `x1command` 內存在的命令，否則 ACK 時 KeyNotFound）。

| 子命令 | 用途（依 Demo 註解） | data 範例（不含 CMD） | 出處 |
|---|---|---|---|
| 0091h | Get target version | `00 91` | `F1:335` |
| 0132h | **設定 PL/PS 的資料庫（Excel）列索引** | `01 32 00 01 00 02 00 00 00 05`（PL1、PS2、index u32 = 5） | `F1:338` |
| 0133h | 讀取資料庫索引 | `01 33 00 00 00 00` | `F1:339` |
| 0120h | 讀使用者清單；回應 `[2..3] count`，之後每個 id 2 bytes | `01 20` | `F1:387`、`SDK:706-715` |
| 0121h | 讀使用者資訊（id u16）；回應 `[2..3] id`、`[4..259]` ASCII 名稱、`[260]` Message、`[261]` Device、`[262..263]` Setting、`[264]` QuickEdit | `01 21 00 00` | `F1:468`、`SDK:716-728` |
| 0122h | 寫使用者資訊 | 265 bytes | `F1:493-509` |
| 0123h | 新增使用者（名稱 ASCII，256 bytes 欄） | 258 bytes | `F1:409-412` |
| 0124h | 刪除使用者（id u16） | `01 24 00 03` | `F1:442` |
| 0221h | Reset count | `02 21 00 88 00 99` | `F1:385` |
| 0232h | Get printhead information | `02 32 00 00` | `F1:388` |
| 0241h | Get counter by filename（檔名 256 bytes + type 1 byte） | 259 bytes | `F1:341-351` |
| 0242h | Set counter by filename（V1.0.0 277 bytes；V1.0.24 加 high 32-bit 共 289 bytes） | 見原始碼 | `F1:354-381` |
| 0004h | Get X1 ini | `00 04` | `F1:397` |

Demo 的 `button1_Click`（`F1:332-400`）目前實際送出的是 **0132h（設定資料庫索引 = 5）**。

完整框範例（0132h）：`00 00 00 00 00 10 00 64 00 0C 00 20 01 32 00 01 00 02 00 00 00 05`

### 3.4 檔案上傳 78h + 7Eh

* SDK 入口：`UploadFile(type, path[, fileNumber])`、`UploadXml/UploadIni/UploadExcel/…`（`SDK:1029-1242`）；RequestCommand 分派 `SDK:913-924`。Demo：`F1:203-239`、`FormUploadFile.cs`。
* 型別碼 `AnserFileType`（`LIB:28-76`）：01h bms、02h jms、03h alg logo、04h tpf font、05h config.ini、06h internal.ini、07h ink.ini、08h Excel `xxxx.ini`、10h xml 訊息、11h bmp、12h shift ini、13h ttf/otf、14h gz（SPH 不支援）、15h **Variable ini**、16h firmware、17h 暫存 bmp、18h external.ini、19h 預覽 png（`PL<n>PS<n>.png`）、1Ah dio、1Bh zip、1Ch Excel csv、1Dh print-count csv（SPH 不支援）。
* 本地大小上限（X1/A1 值，`LIB:345-361`）：Excel/ExcelCsv 8 MB、xml/bmp/shift/variable/temp/external/dio 1 MB、ttf 18 MB、gz/zip 15 MB、firmware 25 MB、print-count csv 4 MB。註解：SPH 的 Excel 為 4 MB、firmware 10 MB（`LIB:342-343`）——**SDK 本地檢查用的是 X1 值，對 SPH 不正確**。
* **78h 請求 data**（`LIB:410`、`LIB:453-457`）：`[0..3] 檔案大小 u32 BE`、`[4] type`、`[5..]` 鍵值：
  * type 01h–08h：`file number u16 BE`（`LIB:368-371`、`LIB:438-441`），未指定時取檔名中最後一串數字（`LIB:491-506`）。
  * type ≥ 09h：UTF-8 檔名（只取檔名，不含路徑），≤ 128 bytes（`LIB:443-450`）。
* 範例 xml（1234 bytes 的 `MSG00003.xml`）：`00 00 00 00 00 17 00 64 00 13 00 78 00 00 04 D2 10 4D 53 47 30 30 30 30 33 2E 78 6D 6C`
* 範例 Excel `0001.ini`（316786 bytes）：`00 00 00 00 00 0D 00 64 00 09 00 78 00 04 D5 72 08 00 01`
* **7Eh 資料封包 data**（`LIB:787-824`）：`[0..1] packet number u16 **little-endian**`、`[2..4097]` 4096 bytes 檔案內容（最後一包補 0，**每包固定送 4096**，`LIB:790`、`LIB:806`）。packet number 從 0 起算，firmware 從 1 起算（`LIB:746-747`）。
  範例開頭：`00 00 00 00 10 08 00 64 10 04 00 7E 00 00 …`
* 流程：送 78h → 印表機回 4Fh → 送 7Eh#0 → 4Fh → 送 7Eh#1 → … → 檔案讀完後**再收到一個 4Fh** 時結束並觸發 `OnUploadFinish`（`LIB:798-803`）→ X1SDK dequeue 78h 並回報 `"UploadData","OK"`（`SDK:396-432`）。過程中任一 31h → 中止（`LIB:1221-1234`）。
* Demo 上傳的範例檔：`test.ini`（type 15h Variable，格式 `[DB_String]` + `String1`…`String20`，UTF-8 無 BOM，`Resources\test.ini`、`F1:203-210`、`F1:57`）與 `0001.ini`（type 08h Excel，`[1]`…`[20]` 共 20 欄 × 每欄 1000 列，`Resources\0001.ini`、`F1:212-219`）。

---

## 4. 動態變數（0xCA / 0xCF / 0xCB / 0xCC）詳解

### 4.1 CAh SetDynamicVarTable（以「變數表檔名」寫入 20 個字串）

* LIB `LIB:1012-1066`、SDK `SDK:995-998`（分派 `SDK:931`）、Demo `F1:320-330`（按鈕 "Send as Set Dynamic String Table (CAh)"，`F1D:691`；檔名欄 "var table filename:"，`F1D:641`）。
* data 版面（全部 length-prefixed，無對齊）：

| 欄位 | 長度 | 說明 |
|---|---|---|
| fnLen | 1 | 檔名 UTF-8 byte 數（0–255）（`LIB:1044`） |
| fileName | fnLen | UTF-8 檔名（`LIB:1022`、`LIB:1045`） |
| 重複 20 次：len_i | 1 | 第 i 個字串的 UTF-8 byte 數（0–255）（`LIB:1049`） |
| 重複 20 次：str_i | len_i | UTF-8 字串（`LIB:1033`、`LIB:1050`） |

* **一定要剛好 20 個字串**，否則 `ArgumentException`（`LIB:1014-1019`）。不想改的欄位只能送空字串（長度 0）；**【不明】** 空字串對印表機代表「清空」還是「不變」，程式碼無法判斷。
* 單一字串、檔名上限各 255 UTF-8 bytes（`LIB:1023-1026`、`LIB:1034-1037`）。中文一字 3 bytes → 每欄約 85 個中文字。
* 沒有 null terminator、沒有 PL/PS。
* 範例（檔名 `test.ini`，var1 = `AB12`，var2 = `佳格`，其餘 18 個空）：
  `00 00 00 00 00 2D 00 64 00 29 00 CA 08 74 65 73 74 2E 69 6E 69 04 41 42 31 32 06 E4 BD B3 E6 A0 BC 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00`
* 回應：SDK 沒有 CAh 專屬解析；名稱只出現在 `x1command`（`SDK:131`），供 4Fh/31h 路徑查表用（`SDK:527`、`SDK:484`）→ 表示 SDK 預期印表機回 **4Fh ACK** 或 **31h 錯誤**。若印表機回 CMD = CAh，會落入 default → `"Unknow command"`（`SDK:461-463`）。
* **【推論】** 語意：Demo 的 `test.ini`（type 15h Variable 檔）結構正是 `[DB_String] String1..String20`，與 CAh 的「檔名 + 20 字串」一一對應；按鈕文字 "Set Dynamic String Table" 與欄位 "var table filename" 也吻合。所以 CAh 很可能是「不經 78h 上傳、直接以命令覆寫印表機上名為 X 的變數表（Variable ini）的 20 個字串」。它以**檔名**定址、不綁產線/工位；哪個訊息會用到這張表，取決於訊息（xml）裡引用了哪個變數表檔。

### 4.2 CFh SetDynamicVarBuffer（以「產線 / 工位」寫入 20 個字串到緩衝區）

* LIB `LIB:1074-1118`、SDK `SDK:1000-1003`（分派 `SDK:932`）、Demo `F1:558-572`（按鈕 "Send to Dynamic Var Buffer (CFh)"，`F1D:680`；PL/PS 借用 `FormSetPrintMessage` 對話框，訊息檔名欄位被忽略）。
* data 版面：

| 欄位 | 長度 | 說明 |
|---|---|---|
| LineID | 2 | u16 BE（`LIB:1100`、`LIB:1103`） |
| StationID | 2 | u16 BE（`LIB:1101`、`LIB:1104`） |
| 重複 20 次：len_i | 1 | UTF-8 byte 數 0–255（`LIB:1108`） |
| 重複 20 次：str_i | len_i | UTF-8（`LIB:1088`、`LIB:1109`） |

* 同樣必須剛好 20 個字串、每個 ≤ 255 bytes（`LIB:1076-1092`）。
* 範例（PL 1、PS 1，var1 = `AB12`，var2 = `佳格`，其餘空）：
  `00 00 00 00 00 28 00 64 00 24 00 CF 00 01 00 01 04 41 42 31 32 06 E4 BD B3 E6 A0 BC 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00`
* 回應：同 CAh，預期 4Fh / 31h（`SDK:132`）。

### 4.3 CBh ClearDynamicVarBuffer / CCh ReadDynamicVarBufferStatus

* **CBh** LIB `LIB:1125-1142`、SDK `SDK:1005-1008`、Demo `F1:584-593`。data：`[0..1] LineID`、`[2..3] StationID`。
  範例：`00 00 00 00 00 0A 00 64 00 06 00 CB 00 01 00 01`；回應 4Fh / 31h。
* **CCh** LIB `LIB:1149-1166`（XML 註解誤寫成 "Clear"）、SDK `ReadDynamicVarBuffer`（`SDK:1010-1013`）、Demo `F1:574-583`。data 同上。
  範例：`00 00 00 00 00 0A 00 64 00 06 00 CC 00 01 00 01`
  回應：CMD = CCh，data `[0..1] int16`（`SDK:695-697`），SDK 只把它當成一個數字輸出，名稱是 "ReadDynamicVarBufferStatus"（`SDK:134`）。

### 4.4 CAh vs CFh 比較與建議

| | CAh SetDynamicVarTable | CFh SetDynamicVarBuffer |
|---|---|---|
| 定址 | 變數表**檔名** | **產線 + 工位** |
| 欄位數 | 固定 20 | 固定 20 |
| 編碼 | UTF-8，1-byte 長度前綴 | 同 |
| 有無配套命令 | 無 | CBh 清除、CCh 讀狀態 |
| 【推論】語意 | 覆寫一張（持久的）變數表，之後所有引用它的列印都用新值 | 推入該工位的動態變數**緩衝區**；有「清除」與「讀狀態（一個 int16，極可能是緩衝區內尚未列印的筆數）」→ 很像 FIFO 佇列，每印一次消耗一筆 |

**建議（逐箱更新變數資料）**：優先使用 **CFh**。理由：(1) 直接以產線/工位定址，不需知道印表機上的變數表檔名；(2) 有 CBh/CCh 可在換單或開機時清空並監看剩餘筆數，能做到「預先送 N 筆、印一筆補一筆」的節流，避免與輸送帶速度賽跑；(3) 若真是 FIFO，可避免 CAh「覆寫後下一個觸發才生效、可能印到舊值或新值不確定」的時序問題。
但以下必須**上機驗證**，程式碼無法證明：
1. CFh 是否為 FIFO（連送 3 筆後 CCh 是否回 3、每印一張是否減 1）；
2. 緩衝區上限多少、滿了回哪個錯誤碼（推測 00h 或 09h）；
3. 緩衝區為空時印表機行為（印空白？用變數表？停機報警？）；
4. 印表機是否必須處於列印中狀態（或已選訊息）才接受 CFh——錯誤碼 07h「Incorrect production line status」、11h「Select message before print」暗示可能有狀態要求；
5. 訊息（xml）中的欄位如何對應到第 1–20 個字串；
6. 空字串的意義。

若現場驗證 CFh 不是佇列，而是「目前值」暫存區，那麼 CAh 與 CFh 的差別主要只在定址方式；仍建議用 CFh（不依賴檔名、且不會寫入快閃記憶體——【推論】CAh 以檔名操作，可能每次都寫檔，頻繁逐箱寫入有磨耗與延遲疑慮）。

### 4.5 其他「逐筆資料」機制（供對照）

* **Excel 資料庫 + 索引**：上傳 `xxxx.ini`（type 08h，20 欄 × 最多 1000 列，`Resources\0001.ini`）後，用 `20h+0132h` 設定 PL/PS 的資料庫列索引（`F1:338`）。這是「預先下載整批資料、印時指定列號」的模式。**【推論】** 舊系統所謂「ADT」機制很可能是這一類（程式碼中沒有出現 "ADT" 字樣，無法確認）。
* **Variable ini 上傳**（type 15h，`SDK:1059-1062`）：整份檔案上傳，速度慢（78h + 至少一個 7Eh + 收尾 ACK，至少 3 個來回）。

---

## 5. 開始 / 停止、狀態、計數、選訊息（生產相關命令彙整）

| 目的 | 建議命令 | 備註 |
|---|---|---|
| 選訊息（停機時） | `20h+214h` | 以 PL/PS + ASCII 檔名（256 bytes 補 0） |
| 列印中換訊息 | `20h+24Ch` | UTF-8 + 長度前綴 |
| 開始列印 | `20h+203h`，operation = **00** | 以產線為單位；替代方案 46h 在 SDK 中未公開 |
| 停止列印 | `20h+203h`，operation = **01** | |
| 列印狀態 / 計數 | `45h` | 見下 |
| 單一計數器 | `3Ah`（以訊息號）或 `20h+0241h`（以檔名） | |
| 設定計數器（列印中） | `20h+24Ah` | |
| 警報 | `7Fh`（簡要）、`A7h`（完整，含墨水量） | |
| 校時 | `35h` / `34h` | |
| 重置 | `C4h` | 慎用 |

### 5.1 45h GetPrintingStatus 回應

`SDK:556-636`：`[0]` = 目前列印中的產線數（0、1、2）。每條產線一個區塊，區塊內 offset（相對區塊起點）：

| +off | 型別 | 欄位（SDK 註解） |
|---|---|---|
| 0 | u16 | production line |
| 2 | u16 | print station |
| 4 | int32 | item counter |
| 8 | int32 | box counter |
| 12 | int32 | lot counter |
| 16 | u32 | print count |
| 20 | u32 | print total count |
| 24 | int32 | printables |
| 28 | u32 | print per minute |
| 32 | u32 | line speed |
| 36 | u32 | reserved |
| 40 | u32 | accumulated printing time |
| 44 | u32 | reserved |
| 48 | u32 | total printable |
| 52 | u32 | cartridge idle time |
| 56 | u32 | item counter high part（V1.0.24 新增） |
| 60 | u32 | box high part |
| 64 | u32 | lot high part |

第一區塊起點 = 1（`SDK:567`）。**【不明 / Bug】** 第二區塊起點寫死 57（`SDK:594`）＝ 1 + 56，正是 V1.0.24 加入 high part 之前的區塊大小；但第一區塊卻讀到 +67，兩區塊重疊 12 bytes。實際第二區塊起點應為 57（舊格式）或 69（新格式），需以實機回應長度判斷（新格式總長應為 1 + 68 × n）。另外 2 條產線分支漏讀了第一區塊的 print-per-minute（+28）（`SDK:604-605`），回呼參數個數與 1 條產線時不一致。

注意：45h 只回報**列印中**的產線；產線停止時 `[0] = 0`，此時無法讀到其計數。

---

## 6. 已知 Bug、怪癖與執行緒安全問題

### 6.1 X1SDK.cs 與 .bak-20261012 的差異
唯一差異：`X1UploadFinish()` 中呼叫 `OnX1Respone` 前加了 null 判斷（`SDK:417-419`）。舊版在未訂閱 `OnX1Respone` 的應用程式上傳檔案完成時會丟 `NullReferenceException`（而且是在 TCP 接收執行緒上 → 程式崩潰）。注意 `getPDU` 本來就有 null 判斷（`SDK:457`），所以只有上傳路徑有這個問題。

### 6.2 佇列永久卡死（最嚴重）
1. **沒有逐命令逾時**。回應遺失、印表機不回、或被主動推送訊息錯配時，佇列頭永遠不 dequeue，之後所有命令只排隊不送出（`SDK:882-890`）。60 秒 watchdog 只會停 timer，不會清佇列或重連（`SDK:347-355`）。
2. **例外發生在 Enqueue 之後**：`addToCmdQueue` 先 `Enqueue` 再呼叫 `RequestCommand`（`SDK:884-887`）。CAh/CFh 字串數不是 20 或超過 255 bytes（`LIB:1016-1018`、`LIB:1034-1037`）、檔名無數字（`LIB:516`）、`20h+214h` 檔名 > 256（`LIB:992`）、socket 寫入失敗（`TCP:232`）等例外，都會讓命令留在佇列頭卻從未送出 → 卡死。
3. 同樣的例外若發生在 TCP 接收執行緒送「下一個命令」時（`SDK:757`），是未處理例外 → **整個程序崩潰**。
4. 斷線時 `SendADU` 靜默丟棄（`LIB:1269-1270`），命令留在佇列。`CloseLink` 不停 watchdog（`SDK:325-329`），timer 會繼續排 43h。直到下次 `LinkToPrinter` 才清空佇列（`SDK:284-287`）。

### 6.3 回應配對錯位
* FIFO 配對、不比對 CMD（`SDK:445-452`）。任何主動推送訊息（30h？）都會錯位。
* 上傳失敗時雙重 dequeue：`UploadFail`/31h 會先 `OnExcuteAckFail`（`getPDU` dequeue 78h，並可能已送出下一個命令，`SDK:748-758`），接著 `OnUploadFinish`（`LIB:528-531`、`LIB:1225-1233`）→ `X1UploadFinish` 若發現新的佇列頭又是 78h，會把**尚未完成的下一個上傳**當成完成 dequeue、回報 OK，並再送一次佇列頭（`SDK:408-430`）。連續排多個上傳且其中一個失敗時會出錯。

### 6.4 TCP 接收
* 一次 `Read` = 一個框的假設（`TCP:146`、`LIB:1187-1216`）；無長度重組、無黏包拆分。`A7h`（data 至少 112 bytes）、`20h+0121h`（data 至少 265 bytes）在區網通常沒事，但不保證。
* 收到 < 12 bytes 時 `MemoryStream.Read` 讀不足，表頭欄位為殘值。
* `ConnectToServer` 在連線完成前讀 `LocalEndPoint`（`TCP:69`），逾時分支把 `tcpClient` 設 null 但沒 `Close()` → socket 洩漏（`TCP:73-79`）。
* `Disconnect()` 在 null 檢查前就使用 `tcpClient.Client`（`TCP:294-295`）。
* 輪詢式接收：沒資料時 `Thread.Sleep(1)` 忙等（`TCP:184-185`）。

### 6.5 執行緒安全
* `bHasAck`、`elpased_time` 由三條執行緒讀寫，無 lock、無 volatile（`SDK:288-289`、`SDK:347-357`、`SDK:389-390`、`SDK:454-455`、`SDK:885-886`）。原作者也在 `SDK:885` 註解中自承有時序疑慮。
* `RequestCommand` 有時在 `cmd_queue_lock` 內（`SDK:887`）、有時在外（`SDK:429`、`SDK:757`）被呼叫；`CTcpClient` 的 `WriteBuffer`/`CurrentWriteByteCount` 完全沒有 lock（`TCP:216-235`）。依目前「一問一答」邏輯，正常情況只會有一條執行緒在寫，但上述錯配情境下可能兩條執行緒同時寫入 → 框交錯。
* 上傳狀態（`br`、`packno`、`bStartTransfer`）在 UI 執行緒建立、在接收執行緒推進，無同步（`LIB:92-97`）。
* `OnX1Respone` 在接收執行緒上執行；事件處理器慢 = 整個協定停擺。

### 6.6 其他怪癖
* `20h+203h` 反邏輯：0 = Start、1 = Stop（`SDK:1302-1306`）。
* `7Eh` packet number 是 little-endian（`LIB:791-794`）。
* 每個 `7Eh` 固定送 4096 bytes，最後一包補 0（`LIB:790`）。
* 無 data 的 31h 以 `0xFF` 表示（`LIB:1230`、`SDK:378-379`），與真實錯誤碼共用值域。
* `7Fh` 若回傳碼 > 9 → `err_7f_report[...]` KeyNotFoundException（`SDK:640`）；`BEh` 回傳 > 1 同理（`SDK:688`）；`30h` 若 `[0] > 2` 無輸出。
* `SendRawData` 的第一個 byte 若不在 `x1command` 中，收到 ACK 時 KeyNotFoundException（`SDK:527`）。
* 大小上限依 X1 值，SPH 較小（`LIB:342-343`）。
* 方法名稱與實際行為不符：`LIB.SetCounter` 其實是讀（`LIB:265`）；`SetProdutionLineOperation` 拼錯（`SDK:1297`）；多處 XML 註解錯誤（`LIB:863`、`LIB:971`、`LIB:1144-1146`、`SDK:1327`、`SDK:1345`）。
* `LinkToPrinter` 依賴 ICMP ping（`SDK:276`）。

### 6.7 乾淨重寫建議
1. 用 raw TCP socket（port 25001），**依 MBAP Length 欄位做串流重組**（先讀 6 bytes，再讀 Length bytes），支援分段與黏包。
2. 自行遞增 Transaction ID（【不明】印表機是否回顯，需上機確認；若回顯即可用它配對，否則維持一問一答）。
3. 每個請求有逾時（建議 1–3 秒，上傳另計）；逾時即視為連線異常 → 關閉 socket、清佇列、重連，並把失敗回報給呼叫端（例如 `Task<Response>` / `await`）。
4. 回應配對時檢查 CMD：請求 CMD 必須等於回應 CMD，或回應為 4Fh/31h；其他（如 30h）當作主動事件另行處理，不 dequeue。
5. 參數驗證（20 個字串、各 ≤ 255 bytes、檔名長度、總 ADU 大小）在**入佇列前**完成。
6. 單一寫入者：所有 socket 寫入集中在一條 I/O 執行緒或以 `SemaphoreSlim` 保護。事件回呼不要在 I/O 執行緒上執行。
7. 開啟 `SO_KEEPALIVE`，並保留應用層 43h 心跳（例如 5–10 秒），心跳逾時即重連。
8. 不依賴 ICMP。
9. 大小上限依機種（X1 / SPH）區分。
10. 45h 解析依實際長度判斷區塊大小（56 或 68）。

---

## 7. 最小生產流程（建議）

以下以 PL = 1、PS = 1、訊息 `MSG00003.xml` 為例；每一步都等到 4Fh（或對應資料回應）再送下一步，任何 31h 即中止並依錯誤碼處理。

```
1. TCP connect <printer-ip>:25001                     （逾時 5 s）
2. 43h  GetAnserProtocolVersion                        確認通訊 / 版本
   00 00 00 00 00 06 00 64 00 02 00 43
3. 7Fh  GetPrinterAlarmStatus                          兩噴頭須為 0 (No Error)
   00 00 00 00 00 06 00 64 00 02 00 7F
4. 20h+203h Stop line 1（保險起見，確保停機狀態）
   00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 01
5. 20h+214h 選訊息 PL1/PS1 = "MSG00003.xml"（ASCII，補 0 至 256 bytes）
   00 00 00 00 01 0C 00 64 01 08 00 20 02 14 00 01 00 01 4D 53 47 30 30 30 30 33 2E 78 6D 6C 00 ...
6. CBh  清空 PL1/PS1 動態變數緩衝區
   00 00 00 00 00 0A 00 64 00 06 00 CB 00 01 00 01
7. CFh  預先送第 1 箱（必要時連送 2~3 箱，視 §4.4 驗證結果）
   00 00 00 00 00 28 00 64 00 24 00 CF 00 01 00 01 04 41 42 31 32 06 E4 BD B3 E6 A0 BC 00 x18
8. 20h+203h Start line 1   （operation = 00）
   00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 00
9. 迴圈（每箱）：
   a. CCh 讀緩衝區狀態（若為佇列，低於水位才補）
      00 00 00 00 00 0A 00 64 00 06 00 CC 00 01 00 01  → 回 CCh + int16
   b. CFh 送下一箱的 20 個字串
   c. 45h 讀列印狀態 / print count 確認已印出（每 N 箱或定時）
      00 00 00 00 00 06 00 64 00 02 00 45
   d. 定時 7Fh 監看警報
10. 20h+203h Stop line 1   （operation = 01）
11. 45h 讀最終計數（注意停機後 45h 可能回 0 條產線；最好在 stop 前讀）
    或 3Ah GetCounter(msgNo, type)：00 00 00 00 00 09 00 64 00 05 00 3A 00 01 01
12. 關閉 TCP
```

上線前必須在實機確認：CFh 是否為 FIFO 及其深度、空緩衝區行為、CFh 是否需在列印中/已選訊息狀態、訊息欄位與 String1–20 的對應、印表機是否回顯 Transaction ID、45h 區塊大小（56 或 68）、錯誤碼 10h–14h 實際值（十六進位或十進位）。
