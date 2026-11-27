# 02 鶴見噴印控制臺（HeJianPrintingConsole.exe, .NET）原始碼還原與行為說明

> 對象：`D:\dev_program\MITSU\鶴見\HeJianPrintingConsole.exe`（AssemblyVersion 2.1.25.2，FileVersion 2.1.25，Copyright © Anser 2012，檔案日期 2013-08-23）＋同日 `.pdb`。
> 反編譯：ILSpy CLI `ilspycmd 9.1.0.7988`（由 nuget.org 下載 nupkg 後以已安裝的 .NET 8.0.11 runtime 直接執行，無 SDK），參數 `-p --use-varnames-from-pdb`。
> 反編譯輸出：`D:\dev_program\MITSU\analysis\decompiled_dotnet\HeJianPrintingConsole\`（以下引用以此為根目錄，`HJ/` = `HeJianPrintingConsole/` 子資料夾，`SC/` = `SpCommon/` 子資料夾）。
> 本文件只描述「行為」，作為之後換新印表機重寫的規格來源；原程式為 WinForms + SQL Server + Anser PrintingPro（ADT 介面）。
> 注意：`鶴見\hejian.xml`、`hejian_2.1.x.x\` 資料夾是 2015–2016 年改寫的 **Delphi 版 hejian.exe** 用的，**不是** 這支 .NET 程式的設定（.NET 版只讀 `Config.xml`），詳見 §12。

---

## 0. 一頁摘要

1. 這是一支「噴印站前端」：操作員選一筆 **Key code**（產品噴印資料）、輸入 **製番**、**箱數範圍**、**機番起始值**，按「噴印」。
2. 程式依該產品的 **CASE-No（紙箱編號）** 查 **紙箱規格**（表面/裏面印字開始、終了 mm），把兩個 PrintingPro 範本（正嘜 `.ms`、側嘜 `.ms`）**合成** 為一個「整體模板」`Msg\AdtTemp.ms`（上半＝表面、下半＝裏面，各含正嘜＋間隙＋側嘜）。
3. 依紙箱規格計算 **4 組邏輯噴頭延遲（mm）** 寫入 PrintingPro（`imSetLgphDelay`），讓表面從「表面印字開始」、裏面從「裏面印字開始」處起印。
4. 把整體模板送到 PrintingPro（`imAppendMessage`），之後每一箱送一「頁」ADT 動態資料（23 個欄位：文字或 BMP），每頁機番 +1（或逆序 −1）。
5. 送出頁數與印表機回報「已印頁序號」之差＝在途緩衝（上限 `PrintMaxMem`，預設 500 頁）；已印頁回寫 SQL 生產記錄。
6. 感測器、編碼器（同步器）、觸發都由 PrintingPro / 噴印控制器處理，本程式只透過 `aimdtl.dll` 的共享記憶體介面「送資料＋查狀態」。

---

## 1. 二進位檔分類（.NET / 原生）

以 python `pefile` 檢查 CLR header（Data Directory #14）：

| 檔案 | 類型 | 說明 |
|---|---|---|
| HeJianPrintingConsole.exe | .NET (x86) | 主程式，已反編譯 |
| NPOI.dll | .NET | Apache NPOI 1.2.4.0（讀寫 .xls），第三方，已反編譯到 `decompiled_dotnet\NPOI\`（914 個 .cs，僅供參考） |
| aimdtl.dll | 原生 x86（C++Builder 6：vcl60/rtl60.bpl, CC3260MT） | **ADT 資料傳輸介面**（Anser Ink-jet Message Data Transfer Library），FileVersion 2.0.0.1 |
| ansercui.dll | 原生 C++Builder 6 | Anser 共用 UI（登入、語系、對話框、檔案選擇），FileVersion 1.0.1.0 |
| acmmil.dll / acmmil_2.dll | 原生 C++Builder 6 | **訊息（範本 .ms）管理函式庫**（mi\*），預覽用；_2 為較早（2012-07-12）版本，兩者 export 相同 |
| acbcl.dll | 原生 C++Builder 6 | 條碼 plugin 載入器（bc\*） |
| acdsl.dll | 原生 C++Builder 6 | 資料來源 plugin 載入器（ds\*） |
| plugin\bc\abc*.dll（19 個） | 原生 | 條碼 plugin（Code128、Code39、EAN13、QR、DataMatrix、GS1…），各只 export `bcGetPlugin` |
| plugin\ds\ads*.dll（17 個） | 原生 | 資料來源 plugin（ADT、計數器、日期、班別、SN、XLS/CSV/MDB/MSSQL/MySQL/Oracle…），各 export `dsGetPlugin` |
| DataResource\InitializDataBaseTabsFileSrc.dll | **不是 DLL**，是改副檔名的 Excel 97 `.xls`（OLE2） | 建資料表用的「欄位樣板」，見 §4.1 |

**反編譯失敗項目：無**。原生 DLL 無法反編譯為原始碼，只做 export/import/字串分析（§8）。
另：主程式外，原生相依 `ANSERMBS.DLL`、`vcl60.bpl`、`rtl60.bpl`、`BORLNDMM.DLL`、`CC3260MT.DLL` 不在此資料夾（屬 PrintingPro 安裝目錄）。

---

## 2. 架構

### 2.1 專案結構（反編譯後）

- `HJ/Program.cs` — 進入點。全域 Mutex `f78436b9-98b5-43b8-aa11-bdee7c6f4ff9` 防止重複執行；若 Config `RestartAppFlag=TRUE`（系統還原後重啟）則先清旗標再啟動（`Program.cs:13-33`）。
- `HJ/frmMain.cs` — MDI 風格主視窗（實作為 Panel 內嵌子 Form），選單切換子畫面；註冊全域熱鍵。
- `HJ/frmPrintCtrl.cs`（4108 行）— **核心**：噴印控制、範本合成、延遲計算、送資料執行緒、狀態監聽。
- `HJ/ClsAdt.cs` — `aimdtl.dll` P/Invoke 宣告＋噴頭延遲讀寫包裝。
- `HJ/ClsMsg.cs` — PrintingPro `.ms` 範本格式常數與 INI 讀取輔助。
- 其餘表單：`frmPrintDataManage`、`frmPrintDataSelect`、`frmPrintDataBrowse`、`frmCartonSizeManage`、`frmProductRecord`、`frmGroupMsg`、`frmSysSet`、`frmSuperSet`、`frmLogin`、`frmPreview`、`frmPreviewRealResult`、`frmShowPrintStatus`、`frmTimer`、`frmAbout`。
- `SC/`（SpCommon 共用庫）：`ClsDataBase`（SQL）、`ClsXml`（Config.xml）、`ClsLog`、`ClsExcel`（NPOI）、`ClsIni`（Win32 INI API，用來讀寫 .ms）、`ClsPic`（轉 1bpp 黑白 BMP、裁白邊）、`ClsDrawText`（文字轉點陣圖）、`ClsRegHotKey`、`ClsMusic`（未啟用）、`ClsFile`、`ClsShare`、`frmDefMsgBox`。

### 2.2 執行緒模型（皆在 `frmPrintCtrl`）

| 執行緒 | 建立處 | 週期 | 作用 |
|---|---|---|---|
| UI 執行緒 | — | — | 按鈕事件；`btnPrint_Click` 內同步做合成、延遲設定、ADT 初始化 |
| `m_threadListenPrintStatus` (`ListenPrinterStatus`) | `frmPrintCtrl.cs:395-430` | 每 3 秒（`m_listenInterval_ms=3000`） | `imGetPrinterStatus` / `imGetPrinterAlarm`，更新狀態列與浮動狀態窗；若不在「噴印中」則把 UI 解鎖、若送資料執行緒仍在跑則記錄異常結束（`PrintStopStatus=-1`）（`:566-620`） |
| `m_threadGetPrintedPageSn` (`GetPrintedPageSN`) | `:622-667` | 每 100 ms | `imGetPrinttedPagesn` 取得「已印頁序號」，在途計數 −1，回寫 DB 該頁結束時間，更新「已印箱數 / 機番」顯示 |
| `m_ThreadSendData` (`SendDataThread`) | `:2693-2875` | 每 500 ms 一輪 | 在途 < `PrintMaxMem` 時送下一頁（一箱）ADT 資料，寫 DB 明細 |

所有 `aimdtl` 呼叫以 `Mutex m_adtMutex`（`:123`）序列化（等待無逾時 `-1`）。`m_adtMutex` 在 Form Load 時建立並 `imOpenPrinter()`（`:365-384`）。

---

## 3. 畫面（UI）逐一說明

主視窗標題「鶴見噴印控制臺軟體」，選單（`frmMain.cs:339-379`）：

| 選單 | 表單 | 需密碼 | 說明 |
|---|---|---|---|
| 噴印控制 | `frmPrintCtrl` | 否 | 啟動時預設開啟（`frmMain.cs:178-184`），常駐不銷毀 |
| 噴印資料管理 | `frmPrintDataManage` | 是（`frmLogin`） | |
| 紙箱規格管理 | `frmCartonSizeManage` | 是 | |
| 生產記錄 | `frmProductRecord` | 否 | |
| 合成模板 | `frmGroupMsg` | 否 | 手動合成工具（工程用） |
| 系統設定 | `frmSysSet` | 是 | |
| 關於 | `frmAbout` | 否 | 顯示 ProductName / Version / Copyright（`frmAbout.cs:39-45`） |
| 熱鍵 **Ctrl+Alt+Shift+Delete** | `frmSuperSet`「超級設定」 | **否（無密碼）** | `RegisterHotKey(…,7,46)`（`frmMain.cs:183`、`SC/ClsRegHotKey.cs:51-69`）；噴印中（停止鈕可用）時拒絕進入 |

- 噴印中（`btnStop.Enabled`）時無法切換到其他畫面（`frmMain.cs:63-66`）。
- 關閉程式時詢問「是否關閉程式?」；`frmPrintCtrl` 關閉時依序 `imSendCancel → imClearReturnQueue → imClearPrintQueue → imPrinterStop → imClosePrinter`（`frmPrintCtrl.cs:776-789`）。

### 3.1 登入 `frmLogin`
單一密碼欄；與 Config.xml `SystemPwd` 明文比對（`frmLogin.cs:34`）。無帳號、無角色。錯誤顯示「密碼錯誤」。

### 3.2 噴印控制 `frmPrintCtrl`（核心畫面）
控制項（`frmPrintCtrl.cs:3440-4084`）：
- 上方輸入：`Key code:`＋「讀取資料」、`製番NO:`、`箱數:` 起 `/` 迄、`機番:` 前綴（ASCII）＋流水號（數字）、順序下拉「按順序噴印 / 按逆序噴印」。
- 「紙箱規格」群組：紙箱編號、印字高度（表/裏印字開始/終了欄位存在但 `Visible=false`）。
- 「正嘜」群組：MODEL、MODEL_VALUE、BORE/值/單位、OUTPUT/值/單位、PHASE 值/單位、VOLTAGE 值/單位、HZ 值/單位、TOKKI、CODE/值、`NO:` 標籤、機番結果、製番結果。
- 「側嘜」群組：MODEL_VALUE（側）、PLACE。
- 按鈕：預覽、暫停、停止、噴印。提示：「準備預覽…」「正在停止…」「準備噴印…」「數據緩衝中，請在 N 秒後放入紙箱...」。
- 狀態列：燈號（綠＝正常/噴印中/停止、紅＝離線/錯誤）＋文字（`ClsAdt.ClsPrintStatus`，`ClsAdt.cs:49-74`）。

行為：
- **讀取資料**（`:795-815`）：以 `LIKE '%keycode%'` 開 `frmPrintDataSelect`，雙擊選定一列 → 填入各欄位（`ImportData` `:817-912`）→ 依 CASE-No 載入紙箱規格（`ImportCartonSize` `:914-965`）→ 檢查欄位長度上限（`CheckFieldExceedMaxLen` `:967-1081`，上限來自 Config `PrintDataLen_*`），超過則警告。
- 畫面重新顯示時以 Key code 精確重新讀取一次（`frmPrintCtrl_VisibleChanged` `:2384-2412`）。
- 製番、機番前綴只允許 ASCII（`:1251-1262`, `:1719-1746`）；箱數、機番流水號只允許數字。
- **噴印 / 暫停 / 停止 / 預覽**：詳見 §6、§7。

### 3.3 浮動狀態窗 `frmShowPrintStatus`「噴印狀態」
無邊框小窗（位置存 Config `ShowPrintStatusForm*`），顯示「箱數: 已印 / 總」與狀態燈號文字，由 `frmPrintCtrl.ShowPrintStatusEvent` 事件推送（`frmShowPrintStatus.cs:39-74`）。「已印」其實顯示的是 `箱數起` 欄位值（每次回報後被更新為已印箱數，`frmPrintCtrl.cs:1663-1682, 717-744`）。

### 3.4 噴印資料選擇 `frmPrintDataSelect`
分頁（每頁 10 筆，`frmPrintDataSelect.cs:18`）顯示 `TPrintData` 查詢結果；超過長度上限的儲存格以紅字標示（`:145-274`）；可勾「只找超長」（`FindExceedMaxLenPrintData` `:374-381`）。雙擊一列回傳（`ClsShare.m_dtSelData`）。

### 3.5 噴印資料管理 `frmPrintDataManage`
- 讀取資料（同上選擇窗）、新增（Key code 不可重複，`:318-356`）、修改（實作為「刪除 ID 後重新 INSERT」，`:601-631`）、刪除（依 GUID ID，`:632-684`）、匯入 Excel、匯出 Excel（`:691-744`）、瀏覽全部（`frmPrintDataBrowse`，`:855-867`）。
- 匯入：選 `.xls` → NPOI 讀第 1 個工作表（首列為欄名）→ 刪空欄/空列 → 檢查每個欄名都屬於 PrintData 欄位集合 → 若表已有資料詢問「是否清空」→ **整表 DELETE 後在交易內逐列 INSERT**（`:501-595`, `SC/ClsDataBase.cs:182-236`）。失敗訊息提示可能是 Key code 重複（Key code 為 PK）。

### 3.6 紙箱規格管理 `frmCartonSizeManage`
- 列表（欄名前加 ①～⑥ 標示重要欄位，`:166-214`）、點選顯示、新增（CASE-No 不可重複）、修改（刪除後 INSERT）、刪除、匯入、匯出（工作表名 `Carton-Size`）。
- 匯入：逐列 INSERT，若 CASE-No 已存在則逐筆詢問「已經存在，是否覆蓋?」（`SC/ClsDataBase.cs:238-293`）。
- 有一個噴頭延遲表格＋「延遲設定」按鈕（`btnDelaySet_Click` `:284-305`，呼叫 `imSetLgphDelay`），但填表函式 `ShowPrintHeadDelayList` 從未被呼叫（死碼）。
- `CheckCartonSize()`（長+寬 ≥ 印字終了檢查，`:703-768`）也未被呼叫。

### 3.7 生產記錄 `frmProductRecord`
以開始/結束日期查詢（Master.BeginPrintTime ≥ 起日 且 Master.EndPrintTime < 迄日+1），顯示：序號、Key code、製番、紙箱編號、開始噴印箱號、結束噴印箱號、實際噴印箱數、開始機番、結束機番、開始/結束噴印時間（SQL 見 §4.3）。可匯出 Excel（≤65535 列）。

### 3.8 合成模板 `frmGroupMsg`（工程工具）
輸入正嘜、側嘜 `.ms`、橫向間距（像素，預設 100）、縱向間距（預設 100）、單面最大噴頭數（8）、單噴頭高度（150 px）、目標路徑；先橫向合成 A+B 成 `Temp.ms`，檢查高度 ≤ 8×150，把 MsgInfo.Height 改為 1200，再把 `Temp.ms` 與自身縱向合成為目標檔（`frmGroupMsg.cs:90-166`）。演算法同 §5.3。另有 INI↔XML 轉換按鈕（`:713-767`）。

### 3.9 系統設定 `frmSysSet`（需密碼）
分頁（`btnSet_Click` `:269-295`）：
- **路徑**：匯入/匯出預設資料夾（`ImportFolder` / `ExportFolder`）。
- **密碼**：舊密碼驗證後改 `SystemPwd`（`:333-360`）。
- **資料庫**：Server/DB/UID/PWD；先測連線 → 寫 Config → 若 DB 已存在詢問「清空後重新初始化?」（會 `DROP DATABASE`！）→ 背景執行緒最多重試 5 次：`CREATE DATABASE` + 建 5 張表（`:362-663`）。
- **視窗**：主視窗 X/Y/W/H。
- **合成模板參數**：`SourceMsg1Path`（正嘜）、`SourceMsg2Path`（側嘜）、`OneMsgMaxPrintHeadCountOnGroupMsg`、`SinglePrintHeadHeight`（`:460-491`）。
- **備份/還原**：備份 `Config.xml`、`Images\`、`Msg\` 到指定資料夾；還原時先暫存現況到 `BackupTemp\`，覆蓋後 5 秒倒數重啟（`:493-584`）。
- **欄位長度上限**：20 個 `PrintDataLen_*`（`:872-910`）。
- 音效分頁被移除（`:679`）。

### 3.10 超級設定 `frmSuperSet`（熱鍵進入，無密碼）
- 恢復預設密碼：把 `SystemPwd` 設為程式內**硬編碼**的預設值（`frmSuperSet.cs:146-159`；值不在此重述）。
- 延遲設置：噴頭 1–4 各「延遲1」「延遲2」（mm）→ `PrintHead{n}FixedDelay{1,2}`（`:161-192`）。
- 分辨率設置：橫向分辨率 `WidthDPI`（1–450）（`:194-225`）。
- 噴印最大緩存：`PrintMaxMem`（>0）（`:227-254`）。
- 數據緩衝：`DataBufferTime_s`（≥0 秒）（`:117-144`）。

### 3.11 預覽 `frmPreviewRealResult`（實際使用）/ `frmPreview`
`checkBoxIsPreviewRealResult` 預設勾選且隱藏，所以一律用 `frmPreviewRealResult`（`frmPrintCtrl.cs:3171-3195`, `:4016-4024`）。
- 把 `AdtTemp.ms` 複製成 `Msg\Preview.ms`，把每個物件的 `SimpleData` 與 `MsgObjT0` 換成目前資料（物件 1–21 文字，22–24 BMP 十六進位；第 i 個與 i+24 個同值）（`SetPreviewMsgObjContent` `:3208-3388`）。
- 呼叫 `acbcl.bcInitPluginsforPreveiw`、`acmmil.miInitMsgParamforPreview`、`miGetPreViewMessagetoStreamLength`、`miPreViewMessagetoStream` 把 `.ms` 渲染成 BMP（`frmPreviewRealResult.cs:472-495`）。
- 模擬實際紙箱：上半（表面）裁到表面印字範圍，下半（裏面）旋轉 180°、裁掉兩面寬度差；依各自「印字開始」位移貼到一張圖，中間畫「分界線」、兩側畫紅色紙箱邊界（長+寬）（`ConvertPreviewToRealResult` `:388-470`）。座標顯示 mm。
- 校驗（`:616-841`）：印字開始/終了不可超過「長+寬」、開始<終了；表面/裏面噴印區域需 ≥ 正嘜寬+側嘜寬（裏面檢查誤用了「側嘜寬×2」，`:830`）；預覽圖寬換算 mm 後不可超出紙箱。

---

## 4. 資料庫（SQL Server）

連線字串：`server={ServerName};uid={DatabaseUserID};password={DatabasePwd};database={DatabaseName}`（SQL 驗證，`SC/ClsDataBase.cs:43-67`）。所有 SQL 都是字串串接（有 SQL injection 風險；部分欄位以 `''` 跳脫）。

**欄位命名規則**：`[表名前兩字大寫] + 欄名`。表名來自 Config：`TPrintData`、`TCartonSize`、`TProductRecordMaster`、`TProductRecordDetail`、`TLog` → 前綴分別 `TP`、`TC`、`TP`、`TP`、`TL`（Master/Detail/PrintData 前綴相同）。

### 4.1 建表（系統設定 → 資料庫初始化）
`InitializDataBaseTabsFileSrc.dll`（實為 .xls）兩個工作表的首列定義欄名：
- 工作表 `PrintData`：`Key code, CASE-No, MODEL, MODEL_value, BORE, BORE_value, BORE_unit, OUTPUT, OUTPUT_value, OUTPUT_unit, PHASE_value, PHASE_unit, VOLTAGE_value, VOLTAGE_unit, Hz_value, Hz_unit, CODE, CODE_value, TOKKI, PLACE`
- 工作表 `Cartonsize`：`CASE-No, Carton size-L, Carton size-W, Carton size-H, 印字高度, 表面印字開始, 表面印字終了, 裏面印字開始, 裏面印字終了`

| 表 | 欄位（型別） | 建立處 |
|---|---|---|
| **TPrintData** | `[TPID] nvarchar(50)`、`[TPKey code] nvarchar(512) PRIMARY KEY`、其餘 19 欄 `[TP<欄名>] nvarchar(1024)` | `SqlCreatePrintDataTab` → `SqlCreateTab`（`SC/ClsDataBase.cs:113-159, 430-436`） |
| **TCartonSize** | `[TCID] nvarchar(50)`、9 欄 `[TC<欄名>] nvarchar(1024)`（**無 PK**；數值以字串存） | `:438-444` |
| **TProductRecordMaster** | `[TPID] nvarchar(50) PK, [TPBatchNo], [TPKey code], [TPMadeNo], [TPCase-No]` nvarchar(1024)、`[TPBeginPrintTime] datetime, [TPEndPrintTime] datetime`、`[TPBeginBoxNo], [TPEndBoxNo], [TPBeginMachineNo], [TPEndMachineNo], [TPPrintStopStatus]` nvarchar(1024) | `:446-466` |
| **TProductRecordDetail** | `[TPID] nvarchar(50) PK, [TPPageNo] int, [TPKey code], [TPMadeNo], [TPCase-No], [TPCurrentPrintBoxNo], [TPCurrentPrintMachineNo]` nvarchar(1024)、`[TPPageBeginTime] datetime, [TPPageEndTime] datetime, [TPBatchNo] nvarchar(1024)` | `:468-488` |
| **TLog** | `[TLID] nvarchar(50), [TLLogTime] nvarchar(1024), [TLLogMessage] nvarchar(1024)` | `SC/ClsLog.cs:101-118` |

建 DB：`USE MASTER CREATE DATABASE <名>`（連 MASTER）；刪 DB：`USE MASTER ALTER DATABASE <名> SET SINGLE_USER WITH ROLLBACK IMMEDIATE DROP DATABASE <名>`；檢查：`USE MASTER SELECT COUNT(*) FROM MASTER..SYSDATABASES WHERE NAME='<名>'`；檢查表：`select count(1) from sysobjects where name='<表>'`（`SC/ClsDataBase.cs:161-180, 355-428`）。

### 4.2 SQL 語句全表

| # | 位置 | SQL（以實際表/欄名展開） | 用途 |
|---|---|---|---|
| 1 | `SC/ClsShare.cs:208` | `SELECT CONVERT(VARCHAR(100), GETDATE(), 121)` | 取 DB 時間（初始化 `m_strBeginPrintTime`） |
| 2 | `SC/ClsDataBase.cs:494` | `SELECT 1` | 連線檢查 |
| 3 | `SC/ClsDataBase.cs:193-222` | `DELETE TPrintData`；逐列 `INSERT TPrintData([TPID],[TP<col>]…) VALUES('<newGuid>',N'<v>',…)`（同一交易） | Excel 匯入噴印資料（覆蓋） |
| 4 | `SC/ClsDataBase.cs:238-293` | 逐列：`SELECT COUNT(1) FROM TCartonSize WHERE [TCCASE-NO]='<v>'`；若存在且使用者同意 `DELETE TCartonSize WHERE [TCCASE-NO]='<v>'`；`INSERT TCartonSize(...) VALUES(...)` | Excel 匯入紙箱規格（逐筆覆蓋確認） |
| 5 | `SC/ClsDataBase.cs:321` | `SELECT COUNT(1) FROM <表> WHERE [<欄>]='<值>'` | 重複檢查（新增前） |
| 6 | `frmPrintCtrl.cs:798` / `frmPrintDataManage.cs:134` | `SELECT ROW_NUMBER() OVER(ORDER BY [TPKey code]) AS TPNumber,* FROM TPrintData WHERE [TPKey code] LIKE '%<kc>%' ORDER BY [TPKey code]` | 讀取資料 |
| 7 | `frmPrintDataSelect.cs:361/370/379` | 同上（ORDER BY GETDATE()），可加 `AND (LEN([TPBORE])<=n AND …)`（未超長）或 `AND (LEN(..)>n OR …)`（超長） | 選擇窗搜尋 |
| 8 | `frmPrintCtrl.cs:2396` | `SELECT * FROM TPrintData WHERE [TPKey code]='<KC 大寫>'` | 畫面重新顯示時刷新 |
| 9 | `frmPrintCtrl.cs:925, 1319`；`frmPrintDataManage.cs:271`；`frmPreview*.cs` | `SELECT * FROM TCartonSize WHERE [TCCASE-NO]='<caseNo>'` | 取紙箱規格 |
| 10 | `frmPrintDataManage.cs:482-486` | `INSERT TPrintData([TPID],[TPKEY CODE],[TPCASE-NO],[TPMODEL],…,[TPPLACE]) VALUES(N'<guid>',N'…',…)` | 新增/修改 |
| 11 | `frmPrintDataManage.cs:680-683` | `DELETE TPrintData WHERE [TPID]='<guid>'` | 刪除/修改 |
| 12 | `frmPrintDataManage.cs:522` | `SELECT TPID FROM TPrintData` | 匯入前檢查是否有資料 |
| 13 | `frmPrintDataManage.cs:712`, `:860` | `SELECT * FROM TPrintData ORDER BY [TPKey code]` / `SELECT * FROM TPrintData` | 匯出 / 瀏覽 |
| 14 | `frmCartonSizeManage.cs:154, 366` | `SELECT * FROM TCartonSize ORDER BY [TCCASE-NO]` | 列表 / 匯出 |
| 15 | `frmCartonSizeManage.cs:490-505` | `DELETE TCartonSize WHERE [TCCASE-NO]='<v>'`；`INSERT TCartonSize([TCID],[TCCASE-NO],[TCCARTON SIZE-L],…) VALUES(N'<guid>',…)` | 新增/修改/刪除 |
| 16 | `frmPrintCtrl.cs:1279-1298` | `INSERT TProductRecordMaster([TPID],[TPBatchNo],[TPKey code],[TPMadeNo],[TPCase-No],[TPBeginPrintTime],[TPBeginBoxNo],[TPBeginMachineNo]) VALUES('<guid>','<batch>','<kc>','<製番>','<case>',CONVERT(VARCHAR(100),GETDATE(),121),'<起箱數>','<起機番>')` | 開始噴印 |
| 17 | `frmPrintCtrl.cs:1643-1661` | `INSERT TProductRecordDetail([TPID],[TPPageNo],[TPKey code],[TPCase-No],[TPMadeNo],[TPCurrentPrintBoxNo],[TPCurrentPrintMachineNo],[TPPageBeginTime],[TPBatchNo]) VALUES('<guid>',<pageSn>,'<kc>','<case>','<製番>','<箱號>','<機番>',GetDate(),'<batch>')` | 每送出一頁 |
| 18 | `frmPrintCtrl.cs:696-715` | `UPDATE TProductRecordDetail SET [TPPageEndTime]=GetDate() WHERE [TPPageNo]=<sn> AND [TPBatchNo]='<batch>'`（失敗則放入重試清單） | 印表機回報已印 |
| 19 | `frmPrintCtrl.cs:746-766` | `SELECT COUNT([TPPageEndTime]) FROM TProductRecordDetail WHERE [TPBatchNo]='<batch>' AND [TPPageEndTime] IS NOT NULL` | 本批已印箱數 |
| 20 | `frmPrintCtrl.cs:3071-3095` | `UPDATE TProductRecordMaster SET [TPEndPrintTime]=CONVERT(VARCHAR(100),GETDATE(),121), [TPEndBoxNo]=<迄箱數>, [TPEndMachineNo]='<末機番>', [TPPrintStopStatus]='<1/-1>' WHERE [TPBatchNo]='<batch>'` | 每次回報及結束時更新 |
| 21 | `frmProductRecord.cs:127-143` | `SELECT ROW_NUMBER() OVER(ORDER BY M.[TPBeginPrintTime]) AS [序號], M.[TPKey code] AS [Key code], M.[TPMadeNo] AS [製番], M.[TPCase-No] AS [紙箱編號], CAST(M.[TPBeginBoxNo] AS INT)+1 AS [開始噴印箱號], CAST(M.[TPBeginBoxNo] AS INT)+CAST(D.[RealPrintBoxCount] AS INT) AS [結束噴印箱號], D.[RealPrintBoxCount] AS [實際噴印箱數], M.[TPBeginMachineNo], M.[TPEndMachineNo], M.[TPBeginPrintTime], M.[TPEndPrintTime] FROM (SELECT TPBatchNo, COUNT(TPPageEndTime) AS [RealPrintBoxCount] FROM TProductRecordDetail WHERE [TPPageEndTime] IS NOT NULL GROUP BY TPBatchNo) D LEFT JOIN TProductRecordMaster M ON M.TPBatchNo=D.TPBatchNo WHERE M.[TPKey code] IS NOT NULL AND M.[TPBeginPrintTime]>=CAST('y-m-d' AS DATETIME) AND M.[TPEndPrintTime]<DATEADD(DAY,1,CAST('y-m-d' AS DATETIME)) ORDER BY M.[TPBeginPrintTime] ASC, D.[RealPrintBoxCount] ASC` | 生產記錄查詢 |
| 22 | `SC/ClsLog.cs:84` | `INSERT INTO TLog(TLID,TLLogTime,TLLogMessage) VALUES('<guid>', CONVERT(NVARCHAR(100),GETDATE(),25), N'<msg>')` | 記錄 |
| 23 | `SC/ClsLog.cs:114` | `DELETE TLog WHERE TLLogTime<DATEADD(month,-6,GETDATE())` | 啟動時清 6 個月前紀錄 |

`PrintStopStatus`：`1` = 正常（含每次回報時的暫時結束更新、使用者停止、印完），`-1` = 異常（送資料中但印表機狀態不再是「噴印中」，`frmPrintCtrl.cs:600-607`）。

---

## 5. Excel 匯入格式與每箱噴印內容組成

### 5.1 PrintData（噴印資料）Excel
- `.xls`（BIFF/HSSF，NPOI 1.2.4；**不支援 .xlsx**），第 1 個工作表，第 1 列為欄名（大小寫不拘），所有值以 `cell.ToString()` 文字存入（`SC/ClsExcel.cs:14-68`）。
- 欄名必須全部屬於下表（也接受 `ID`），否則「導入格式錯誤」（`frmPrintDataManage.cs:555-579`）。範例：`DataResource\PrintData-0917.xls`（364 筆）。

| Excel 欄 | 意義（範例值） | 長度上限 Config 鍵（預設值 Config.xml） |
|---|---|---|
| Key code | 產品鍵（PK），例 `IA01HN-2DS51` | PrintDataLen_KeyCode (50) |
| CASE-No | 紙箱編號，對應 TCartonSize，例 `01156` | PrintDataLen_CaseNo (12) |
| MODEL | 標籤文字 `MODEL` | PrintDataLen_Model (5) |
| MODEL_value | 型號，例 `HSN2.4S-51`（正嘜大字、側嘜更大字，皆轉 BMP） | PrintDataLen_ModelValue (21) |
| BORE / BORE_value / BORE_unit | `BORE` / `50` / `mm` | 4 / 5 / 4 |
| OUTPUT / OUTPUT_value / OUTPUT_unit | `OUTPUT` / `0.4` / `kW` | 6 / 4 / 2 |
| PHASE_value / PHASE_unit | `1` / 相別單位（中文字） | 1 / 2 |
| VOLTAGE_value / VOLTAGE_unit | `100` / `V` | 11 / 1 |
| Hz_value / Hz_unit | `50` / `Hz` | 2 / 2 |
| CODE / CODE_value | `CODE` / `A01HN-2DS51`（條碼內容） | 4 / 12 |
| TOKKI | 特記（例 `2''NPTFLANGE USA(ETL)`） | 25 |
| PLACE | 產地（例 `MADE BY TSURUMI TAIWAN`，側嘜，轉 BMP） | 25 |

長度上限只用來「警告/紅字」，不阻擋噴印（新增/修改時會阻擋）。

### 5.2 CartonSize（紙箱規格）Excel
`CASE-No, Carton size-L, Carton size-W, Carton size-H, 印字高度, 表面印字開始, 表面印字終了, 裏面印字開始, 裏面印字終了`（單位 mm）。範例 `DataResource\Carton-Size-0917.xls`：`01153, 330, 225, 538, 248, 25, 530, 25, 530`。
噴印時 L、W、H、印字高度只檢查非空，實際使用的是四個印字位置（`frmPrintCtrl.cs:1316-1410`）；預覽用 L+W 作為紙箱一圈「表面＋側面」的展開長度上限。

### 5.3 範本（.ms）與「合成模板」計算
`.ms` 是 PrintingPro 的 INI 格式訊息檔（Encoding.Default/Big5），區段：
- `[MsgInfo]`：`Ver=6, No, ID, Name, Width, Height(像素), Angle, Count(物件數), daCount(資料來源數), dsid`
- `[ObjectN]`：`Type`（1 文字、6 Logo/BMP、7 條碼、9 線）、`X, Y, ZoomX, ZoomY, Width, Height, Angle, Hide, FontSize, Text, FontName, StyleType, SimpleData`，以及 **`MsgObjT0=<資料來源索引>,<欄位索引>,<預設內容>`**（把物件綁到 ADT 欄位）；Logo 物件含 `Data=<BMP 十六進位>`。
- `[DataSrcN]`：`UID=ADSADT`（ADT 資料來源 plugin）、`Name=ADT`、`Group`、`Data=<hex>`：第 1 byte＝欄位數，之後每欄 52 bytes（1 byte 型別：1=文字、2=點陣圖；51 bytes 欄名，NUL 補齊），整段補 0 至 2601 bytes（=1+50×52，ADT 上限 50 欄，`ClsMsg.cs:67-72`、adsadt.dll 字串「you can only edit 50 Fields」）。

**ADT 欄位（整體模板 23 欄，順序即每頁送出順序）**——由 `hejian_2.1.0.7\MSG\AdtTemp.ms` 解碼並與 `AppendPageField2` 對照一致：

| idx | 型別 | 欄名 | 每頁送出內容（`frmPrintCtrl.cs:1480-1584`） |
|---|---|---|---|
| 0 | 文字 | Model | `MODEL + ":"`（MODEL、MODEL_value 都非空才加冒號） |
| 1 | BMP | ModelValueFront | MODEL_value 以 Config `ModelValueFront*` 字型（Arial 140）繪製的黑白 BMP |
| 2 | 文字 | Bore | `BORE + ":"`（BORE_value 非空時） |
| 3 | 文字 | BoreValue | `BORE_value + BORE_unit` |
| 4 | 文字 | BoreUnit | `BORE_unit` |
| 5 | 文字 | Output | `OUTPUT + ":"`（值與單位皆非空時） |
| 6 | 文字 | OutputValue | `OUTPUT_value + OUTPUT_unit` |
| 7 | 文字 | OutputUnit | `OUTPUT_unit` |
| 8 | 文字 | PhaseValue | `PHASE_value` |
| 9 | 文字 | PhaseUnit | `PHASE_unit` |
| 10 | 文字 | VoltageValue | `VOLTAGE_value + VOLTAGE_unit + N 個空白 + Hz_value + Hz_unit`（N=`SpaceBetweenVoltageUnitAndHzValue`=5） |
| 11 | 文字 | VoltageUnit | `VOLTAGE_unit` |
| 12 | 文字 | HzValue | `Hz_value + Hz_unit` |
| 13 | 文字 | HzUnit | `Hz_unit` |
| 14 | 文字 | MadeNo | 製番 |
| 15 | 文字 | Tokki | TOKKI |
| 16 | 文字 | CodeValue_3Space_MachineNo | `CODE_value + "   " + 機番前綴 + 機番流水號`（範本中綁給 Code128 條碼物件） |
| 17 | 文字 | Code | `CODE + ":"` |
| 18 | 文字 | CodeValue | `CODE_value` |
| 19 | 文字 | MahcineNo | 標籤 `NO:`（`lblMacineNo.Text`） |
| 20 | 文字 | MachineNoValue | `機番前綴 + 機番流水號`；**送完此欄後流水號 ±1**（`:1568`, `:1586-1591`，保持位數補 0） |
| 21 | BMP | ModelValueSide | MODEL_value 以 `ModelValueSide*` 字型（Arial 280）繪製，與 PLACE 右對齊後再置於側嘜範本物件 2 的框內（右對齊、垂直置中） |
| 22 | BMP | Place | PLACE 以 `PlaceFont*`（Arial 80）繪製，同上，置於側嘜物件 1 的框內 |

- 範本中許多單位物件設 `Hide=1`，實際印出的是合併字串（例如 BoreValue 已含單位）；送冗餘欄位是為了讓範本可自由選用。
- 文字欄長度以「≥0x80 的字元算 2 bytes」計算後 `imAppendPageField(1, str, len+1)`（`:1593-1607`）。
- BMP 產生：`ClsDrawText.DrawText4`：GDI+ 以字型畫字 → 轉 1bpp（灰階 0.4/0.3/0.3 權重，閾值 128）→ 裁四周白邊 → 再轉 1bpp（`SC/ClsDrawText.cs:124-155`、`SC/ClsPic.cs:91-145, 260-300`）。BMP 只在按「噴印」時產生一次（`IniSpecialSendDataParam` `:2590-2629`），整批不變。

**合成計算（每次按噴印都重做，`GetAdtMsgFile` `frmPrintCtrl.cs:1300-1455`、`GroupMsg_Whole` `:1760-1800`）**：
1. `W1` = 正嘜 MsgInfo.Width、`W2` = 側嘜 MsgInfo.Width（像素，須 >0）；`DPI` = `WidthDPI`（402）。
2. 表面間隙 `gapOut = (表面印字終了 − 表面印字開始) / 25.4 × DPI − W1 − W2`；裏面 `gapIn` 同理（`:1433-1434`）。
3. 表面模板 `Msg\Temp_OutSide.ms` = 正嘜原樣 + 側嘜每個物件 `X += W1 + gapOut`；Width = W1+W2+gapOut；Height 強制設為 `OneMsgMaxPrintHeadCountOnGroupMsg × SinglePrintHeadHeight` = 8×150 = 1200（`ResetDataHeight`）。裏面 `Temp_InSide.ms` 同理用 gapIn。
4. 整體模板 `Msg\AdtTemp.ms` = 表面（Y 不變）+ 裏面每物件 `Y += 1200`；Width = max(兩者)（取整數）、Height = 2400、Name=`Adt Msg`、物件重新編號（表面 1–24、裏面 25–48）（`:1782-1788`）。
5. DataSrc 合併：相同 ADT 欄位（52-byte 描述完全相同）共用同一欄位索引，新欄位附加到末尾，並改寫各物件 `MsgObjT0` 的欄位索引（`GroupMsgDataSrc` `:2235-2372`、`GroupMsgObj` `:2124-2233`）。因此表面與裏面共用同一組 23 欄，一頁資料同時印到兩面。
6. 以 `imCheckIsAdtMsg` 驗證；按噴印時另以 `imGetAdtMsgFieldCount(AdtTemp.ms) == Config AdtFieldCount(23)` 檢查（`:2491-2498`）。
7. 若延遲計算出負值，把三個合成檔複製到 `Log\*_yyyyMMdd_HHmmss.ms` 備查（`:2437-2481`）。

參考文件 `鶴見\合成模板计算方法.doc` 與上述一致（8 個噴頭 × 150 像素＝0.5 吋噴頭高度）。

### 5.4 噴頭延遲（印字起點）計算（`GetPrintHeadDelaySet` `frmPrintCtrl.cs:1162-1225`）
- 有 4 個邏輯噴頭（lgph）：PH1、PH2 印表面，PH3、PH4 印裏面（裏面方向相反）。
- `offset34 = (表面合成寬 − 裏面合成寬) / WidthDPI × 25.4`（僅在裏面 ≤ 表面時，否則 0）。
- `PH1 = PrintHead1FixedDelay1 + PrintHead1FixedDelay2 + 表面印字開始(mm)`；PH2 同理。
- `PH3 = PrintHead3FixedDelay1 + PrintHead3FixedDelay2 + 裏面印字開始(mm) − offset34`；PH4 同理。
  （設計文件寫「+延遲補償2」，程式實作為「−」，以程式為準。）
- 任一 <0 → 中止。只取 PrintingPro 實際回報的噴頭數（`imGetLgphDelay(i)` 直到回 <0）。
- 寫入：若與目前值不同且印表機不在噴印中，`imSetLgphDelay(i, (int)((mm+0.01)×100))`（單位 0.01 mm），失敗最多重試 5 次（`ClsAdt.cs:187-203`、`frmPrintCtrl.cs:1088-1138`）。讀取同理 `/100`。
- 垂直方向：`HeightDPI`=300（僅預覽尺規用）；150 px = 0.5 吋單一噴頭高度。

---

## 6. 送資料到印表機（ADT 序列）

`aimdtl.dll` 與 **Anser PrintingPro** 程式以命名共享記憶體 `AIM_DATA_TRANSMIT_MAP` 及事件 `AIM_DATA_TRANSMIT_READ_EVENT / _WRITE_EVENT / _CONTROL_IN_EVENT / _CONTROL_OUT_EVENT` 溝通（透過 PrintingPro 的 `ANSERMBS.DLL` `ApMapSharedMemory`），並用 Toolhelp 列舉程序（找獨佔者）。**本程式不直接碰觸序列埠、編碼器、電眼**；觸發（電眼 TM-1000）與同步器（400DPI 編碼器，見 `鶴見雙同步器接法.xls`）都在 PrintingPro / 噴印控制器端處理。

### 6.1 按「噴印」（`btnPrint_Click` `frmPrintCtrl.cs:2414-2549`）
1. 讀順序（順序/逆序）；`CheckPrintData`：Key code、製番、起/迄箱數必填，起 < 迄，機番必填；逆序時要求 `機番流水號 + 1 ≥ 箱數` 否則「噴印箱數超過可用機番」（`:2722-2784`）。非暫停時要求狀態列文字＝「噴印停止」（`:2700-2720`）。
2. 合成模板（§5.3）。
3. 取 `m_adtMutex`。非暫停：計算並設定噴頭延遲（§5.4）。
4. 驗證 ADT 欄位數 = `AdtFieldCount`。
5. 非暫停時：
   - `imInitADT(byte[39]{1,0,…})` → `imPrinterStart()` → `imClearReturnQueue()` → `imClearPrintQueue()` → `imSendCancel()`；
   - 每 3 秒 `imGetPrinterStatus` 直到狀態=1（噴印中）（**無逾時**）；
   - `imAppendMessage(整體模板檔內容, 檔長+1, 0, 0)` 送出範本（`SendAdtMsg` `:1457-1471`），≠1 則失敗；
   - 快取欄位值、`IniPrintParam`：`pageSn=1`、`boxBegin=起箱數`、`curBox=起箱數+1`、`boxEnd=迄箱數`、`inFlight=0`、`maxMem=PrintMaxMem`；
   - 新 `BatchNo=GUID`，寫 Master（§4.2 #16）；
   - 產生 3 張 BMP；啟動送資料執行緒；若 `DataBufferTime_s>0` 啟動 1 秒計時器，顯示「數據緩衝中，請在 N 秒後放入紙箱...」倒數（`:3408-3424`）——純提示，不影響送資料。
6. 清除暫停旗標、釋放 mutex。

### 6.2 送資料執行緒（`SendDataThread` `:2786-2875`）
每 500 ms：
- 若 `inFlight ≥ PrintMaxMem` → 跳過（**緩衝上限**，即「噴印最大緩存／緩衝頁數」）。
- 若暫停 → 只更新 UI。
- 若 `curBox ≤ boxEnd` 且未送過：
  - `imPageBegin(pageSn)` → 23 次 `imAppendPageField` → `imPageEnd()`（`SendDataSingle_Invoke` `:2929-2980`）；任一失敗 `imSendCancel()`。
  - 成功：寫 Detail（#17），`pageSn++`、`curBox++`、`inFlight++`，開啟頁序號回報監聽。
- 停止旗標時：`imSendCancel → imClearReturnQueue → imPrinterStop`，跳出並寫 Master 結束（#20）。
- （`imGetADTStatus(false)` 等待可寫的迴圈因條件寫反，實際不會執行，`:2807-2820`。）

送資料速率上限約每秒 2 頁；預先最多送 `PrintMaxMem` 頁，所以「機番」是在**送出時**就決定，不是在實際印出時。

### 6.3 已印回報（`GetPrintedPageSN` `:635-694`）
每 100 ms `imGetPrinttedPagesn(ref sn)`；`sn≠0` 時：`inFlight--`；更新 Detail.PageEndTime（失敗放重試清單，空閒時重試）；`已印 = COUNT(PageEndTime)`；畫面「箱數起」= `起箱數 + 已印`，「機番流水號」顯示 = `起機番 ± (已印−1)`（最後印出的機番）；更新 Master（status=1）。當「箱數起 == 迄」時停止送資料並詢問「噴印完成，是否清空箱數與機番?」。

### 6.4 暫停 / 停止 / 異常
- **暫停**（`:3101-3104`）：只設旗標，執行緒不再送新頁；已在 PrintingPro 緩衝的頁仍會印。再按「噴印」→ 不重設延遲、不重送範本、不重置計數，直接續送（但仍會重新合成模板檔）。
- **停止**（`:3061-3069`）：設旗標 → 執行緒 `imSendCancel/ClearReturnQueue/PrinterStop`，**未印出的緩衝頁被取消**，DB 只有已回報的頁有 PageEndTime；畫面起箱數已是「已印數」，可從下一箱續印。
- **狀態變化**：監聽執行緒看到狀態≠噴印中時解鎖 UI；若送資料仍在跑則以 `PrintStopStatus=-1` 結束紀錄。
- 狀態碼：`imGetPrinterStatus` 回傳 -1＝未開 PrintingPro（自動再 `imOpenPrinter`）、1＝成功，`status[0]`：0 離線、1 噴印中、2 停止、3 警告→`imGetPrinterAlarm`：0 正常、1 卡匣錯誤、2 系統錯誤、3 卡匣警告、4 系統警告（`frmPrintCtrl.cs:449-534`）。

---

## 7. 箱號 / 機番 / 計數器 / 跳號

- **箱數**：輸入「起」＝已完成箱數（0-based），本批印第 `起+1` 到 `迄` 箱；Detail 記錄 `CurrentPrintBoxNo`。生產記錄的「開始噴印箱號」= BeginBoxNo+1。
- **機番** = ASCII 前綴 + 數字流水號（保留位數左補 0），每頁 +1；選「按逆序噴印」則 −1（`GetNextMachineNoValue2` `:1586-1591`）。逆序時檢查流水號下限 0。
- **條碼內容**＝`CODE_value + 3 空白 + 機番`（ADT 欄 16）。
- **跳號處理：此版本沒有**。沒有「跳過某些號碼」或補印功能；PrintingPro 端的計數器 plugin（`adscounter.dll`）也未用（序號由本程式產生）。
- **觀察（可能的缺陷，重寫時注意）**：停止後畫面上的機番流水號被設為「最後已印的號碼」而非「下一號」（`:680-685, 722`），若操作員直接再按噴印，第一箱會重複上一號；起箱數則正確前進。`IniLastMachineNoValue` 算出的「前一號」未被使用（`:2565-2588`）。
- 頁序號 `PageNo` 每批從 1 開始，以 `BatchNo`（GUID）區分批次。

---

## 8. 原生 DLL 功能推斷

| DLL | 主要 export | 推斷用途 |
|---|---|---|
| **aimdtl.dll** | `imOpenPrinter/imClosePrinter, imInitADT, imPrinterStart/Stop, imAppendMessage, imCheckIsAdtMsg, imGetAdtMsgFieldCount/Type, imPageBegin/imPageBeginExt/imAppendPageField/imPageEnd, imSendCancel, imClearPrintQueue/imClearReturnQueue, imGetPrinttedPagesn, imGetPrinterStatus/Alarm, imGetADTStatus/Version, imGetLgphDelay/imSetLgphDelay, imGetPHsHeight, imGetDPI, imSetVMirrorCount, imCheckTrigger, imGetExclusiver`，以及低階 `imDtOpen/Close/ReadBegin/ReadEnd/WriteBegin/WriteEnd/WritePic/AppendField/GetField/Control/CmdOut/CheckCmdIn/FieldBound/GetExclusiveApp` | 與 PrintingPro 的 ADT（動態資料傳輸）用戶端：共享記憶體 `AIM_DATA_TRANSMIT_MAP` ＋ 4 個事件；import `ANSERMBS.DLL` 的 `ApMapSharedMemory/ApUnmapSharedMemory/ApTiffToBmp`。.NET 只用了其中 25 個（`ClsAdt.cs:89-162`），`imGetPHsHeight / imSetVMirrorCount / imGetExclusiver` 有宣告未使用。 |
| **acmmil.dll** | `miInitMsg, miInitMessage, miConvertMsgToBMP/Bin, miGetMsgBmp, miGetMsgInfo, miPreViewMessage*, miInitDynamicMsg, miDynMsgCheckData/MoveNext, miIsAllADTMessage, miMsgGetDelay(s), miShowMsgMg/miShowJobEditor…` | PrintingPro 的訊息（.ms）/作業管理與渲染函式庫（含編輯器 UI）；本程式只用預覽 4 函式。import ANSERCUI/ACDSL/ACBCL/ANSERMBS。 |
| **acbcl.dll** | `bcPluginLoad/Register/Count/Info, bcCreatePBC, bcDraw, bcSetContent, bcCheckContent, bcInitPluginsforPreveiw…` | 條碼 plugin 管理（載入 `plugin\bc\*.dll`）。 |
| **acdsl.dll** | `dsPluginLoad/Register, dsCreatePDS, dsGetFieldCount/Name/Type/Data, dsMoveFirst/Next, dsDoPrinted…` | 資料來源 plugin 管理（載入 `plugin\ds\*.dll`）。 |
| **ansercui.dll** | `auShowLogin, auShowChangePwd, auShowUserMg, auSetLanguage, auReadUnitLangString, auMessageDlg, auInputBox, auShowOpenFile/Folder…` 及大量 VCL 類別 | Anser 共用 UI／語系／使用者管理元件（Delphi 版 hejian 會用；.NET 版未直接呼叫）。 |
| plugin\ds\adsadt.dll | `dsGetPlugin` | ADT 資料來源（UID `ADSADT`），import `aimdtl.imDtGetField/ReadBegin/ReadEnd`；PrintingPro 印每頁時透過它取欄位；最多 50 欄。 |
| plugin\bc\* | `bcGetPlugin` | UPC-A/E、EAN8/13、ITF14、I2of5、Codabar、Code39/128、EAN128、SSCC18、DUN14、PDF417、QR、DataMatrix、GS1 系列、Postal 等。 |

全部原生 DLL 為 Borland C++Builder 6 編譯（2012-03～2012-07 時間戳），需 32-bit。

---

## 9. 記錄（Log）

- `ClsLog.LogWrite(msg)`：前綴「本地时间:…」，先寫 SQL `TLog`；DB 不通才寫檔 `<程式目錄>\Log\yyyyMMddHH.Log`（檔名在類別初始化時決定，程式不重啟就一直寫同一檔）（`SC/ClsLog.cs:20-47`）。
- 啟動時刪除 6 個月前的 TLog（`:101-118`）。
- 大量除錯紀錄：每頁送出前後（aaa1/bbb1/ccc1…）、每次回報 SN、在途數（A-by gw / B-by gw，每 500 ms 一筆）、紙箱規格、合成寬度、延遲值等——TLog 會成長很快。

---

## 10. 登入與密碼

- 唯一密碼 `SystemPwd` 以明文存在 `Config.xml`；用於「噴印資料管理」「紙箱規格管理」「系統設定」。
- 超級設定以熱鍵進入、**不需密碼**，可把密碼重設為硬編碼預設值。
- `Config.xml` 也明文存放 SQL 帳號密碼（`DatabasePwd`，SQL sa 等級帳號）。本文件不轉錄任何密碼。

---

## 11. Config.xml 鍵值說明

格式：`<Anser><ARow ID="n"><Key>鍵</Key><Value>值</Value></ARow>…</Anser>`（`SC/ClsXml.cs:203-280`；每次讀取都重新載入整個 XML）。現存 `鶴見\Config.xml`（2016-01-07）內容如下（密碼略）。

| 鍵 | 現值 | 程式預設 | 意義 / 使用處 |
|---|---|---|---|
| ServerName / DatabaseName / DatabaseUserID | 空 | — | SQL 連線；空則無法連 DB |
| DatabasePwd | （含 SQL 密碼，略） | — | SQL 密碼 |
| InitializDataBaseTabsFileSrc | `DataResource\InitializDataBaseTabsFileSrc.dll` | — | 建表欄位樣板（.xls） |
| PrintDataTabel / CartonSizeTable / ProductRecordMasterTabel / ProductRecordDetailTabel | TPrintData / TCartonSize / TProductRecordMaster / TProductRecordDetail | — | 表名（前兩字為欄位前綴） |
| ModelFolder / ModelName | 空 | — | 未使用 |
| SystemPwd | （含系統密碼，略） | — | 登入密碼 |
| SystemMusic*/MusicWarm* | — | — | 提示音效（UI 已移除） |
| ImportFolder / ExportFolder | 空 | — | 匯入/匯出對話框預設資料夾 |
| MainForm{X,Y,Width,Height}, ShowPrintStatusForm*, PreviewForm* | — | — | 視窗位置 |
| SourceMsg1Path | 空 | — | **正嘜**範本 .ms 路徑 |
| SourceMsg2Path | 空 | — | **側嘜**範本 .ms 路徑 |
| OneMsgMaxPrintHeadCountOnGroupMsg | 8 | — | 每面噴頭單元數（合成時單面高度 = 此值 × 單頭高） |
| SinglePrintHeadHeight | 150 | — | 單一噴頭高度（像素，0.5 吋 @300dpi） |
| PrintHead{1..4}FixedDelay{1,2} | 0 | 0 | 各邏輯噴頭固定延遲（mm），兩個值相加（安裝位置＋微調） |
| WidthDPI | 402 | 1.0 | 橫向（走紙/編碼器）解析度；mm↔像素換算 |
| HeightDPI | 300 | 300 | 縱向解析度（預覽尺規） |
| RestartAppFlag | FALSE | — | 還原後重啟旗標 |
| ModelValueFront{ObjIndex,FontName,FontSize,FontStyle} | 22, Arial, 140, 0 | 22/Arial/140/0 | 正嘜 MODEL_value 點陣字型（ObjIndex 讀了但未使用） |
| ModelValueSide{…} | 24, Arial, 280, 0 | 24/Arial/280/0 | 側嘜 MODEL_value 字型（注意程式裡 Size/Style 的鍵名常數誤寫成 `ModelValueSideFontName`，`SC/ClsXml.cs:148-150`，實際讀到的是字型名而轉數字失敗→用預設 280/0） |
| PlaceObjIndex/FontName/FontSize/FontStyle | 23, Arial, 80, 0 | 23/Arial/80/0 | PLACE 字型 |
| AdtFieldCount | 23 | 0 | 整體模板應有 ADT 欄位數（檢查用） |
| PreviewZoomScale | 10 | 100 | 預覽初始縮放 % |
| PrintMaxMem | 500 | 500 | **噴印最大緩存**：已送未印的最大頁數 |
| SpaceBetweenVoltageUnitAndHzValue | 5 | 5 | 電壓與頻率之間的空白數 |
| AppHasRunFlag | FALSE | — | 未使用 |
| PrintDataLen_* (20 個) | 見 §5.1 | 1 | 各欄位長度上限（警告用） |
| WidthRulerMaxLen / HeightRulerMaxLen | 1800 / 1000 | 1800/1000 | 預覽尺規長度 mm |
| DataBufferTime_s | 60 | 60 | 開始噴印後提示「N 秒後放入紙箱」的倒數秒數 |
| （程式有、檔案無）PrintWaitTotalTime / PrintWaitScanFrequency / AddSpaceLengthWhenTextToBmp | — | 1000/1000/1 | 未使用 |

---

## 12. hejian.xml（Delphi 版設定，供對照）

`鶴見\hejian.xml` 實為 INI 格式，屬 2015-12 之後的 Delphi 版 `hejian.exe`（`hejian_2.1.1.x` 資料夾），**.NET 程式不讀它**（反編譯碼中無任何引用）。內容對應關係：

- `[Server] User/Pass/Name/Data/DataName`：SQL 伺服器（含 sa 帳號密碼，略）、伺服器名、資料庫 `hejian`。
- `[Table] PrintData/CartonSize/ProductRecord`：表名（Delphi 版改用無 T 前綴的表）。
- `[SourceMsg] SourceMsg1Path/SourceMsg2Path`：`Test_8PH_A_Logo.ms` / `Test_8PH_B_Logo.ms`；`SavePathInput/Output`；`ExcelVersion`。
- `[LoginPass] LoginPassWord`：登入密碼（略）。
- `[Data]`：`DPI=400`（對應 .NET 的 WidthDPI 402）、`PerInch=25.4`、`PrintMaxMem=500`、`MaxPrintHeadCount=8`、`SinglePrintHeadHeight=150`、`iAdtDataMaxLen=2601`、`iOneAdtField=52`、`AdtFieldCount=18`（Delphi 版欄位較少）、**`PrintDataBuffer=30`（緩衝頁數）**、`SpaceBtVoltageHzValue=5`、各字型設定（ModelValueSide 255、Place 70）、`PrintHead1FixedDelay1=70, PrintHead2FixedDelay1=310, PrintHead3FixedDelay1=60, PrintHead4FixedDelay1=320`（實機固定延遲 mm，可作為新系統初始值參考）、`LanguageType=2`。

---

## 13. 給重寫的重點整理

1. **資料模型**：PrintData（20 欄文字，Key code 唯一）＋CartonSize（CASE-No → 4 個印字位置 mm）＋生產紀錄（批次 Master＋每箱 Detail，以印表機回報確認「已印」）。
2. **版面**：兩個設計好的範本（正嘜/側嘜）＋紙箱印字區間 → 計算正側嘜之間的空白，產生一個上下兩面的整體版面；印字起點用「噴頭延遲」實現，裏面需扣掉兩面寬度差。
3. **每箱資料**：23 個欄位（20 文字＋3 點陣圖），只有機番（以及條碼裡的機番）逐箱變化；其餘整批固定。
4. **流量控制**：預送上限 `PrintMaxMem` 頁；以印表機回報的頁序號確認並回寫 DB；停止時取消未印頁。
5. **已知缺陷**：停止後機番可能重複；送資料等待迴圈條件寫反；狀態等待無逾時；SQL 字串串接；超級設定無密碼；裏面寬度檢查用錯變數；`ModelValueSide` 字型大小鍵名寫錯；紙箱規格新增時未呼叫範圍檢查。

---

## 附錄 A：產出與工具
- 反編譯專案：`D:\dev_program\MITSU\analysis\decompiled_dotnet\HeJianPrintingConsole\`（含 .csproj、.resx）、`D:\dev_program\MITSU\analysis\decompiled_dotnet\NPOI\`。
- 未處理：`BACKUP\HeJianPrintingConsole_2013*.7z` 等較舊版本（選擇性項目，未解壓比對）。
