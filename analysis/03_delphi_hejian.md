# 03 鶴見噴印控制台 hejian.exe（v2.1.x，2015–2016）逆向還原報告

> 分析對象（唯讀，未執行）：
> - 主要：`鶴見-出差紀錄\鶴見\改流水號判斷\hejian_20160530-2.exe`（2016-05-30，PE link time 2016-05-30 09:17 UTC，FileVersion 仍為 2.1.2.6）
> - 基準：`鶴見\hejian_2.1.2.6\hejian.exe`（link 2016-02-26 02:21 UTC）與同目錄 `aimdtl.dll`、`stlpmt45.dll`、`hejian.xml`、`lanage\*.ini`、`loginfo\*.log`、`msg\*.ms`、`hejian\*.xls`、`model\*.xlsx`
> - 歷史：`hejian_2.1.0.7` … `hejian_2.1.2.5`（26 個版本目錄，2.1.1.18 目錄為空）
>
> 所有中間產物在 `D:\dev_program\MITSU\analysis\delphi_hejian\`（見文末「產出檔案」）。下文位址若未特別註明，皆為 **2.1.2.6 的 VA**（ImageBase 0x400000）。

---

## 0. 重要更正：不是 Delphi，是 C++Builder 6

| 證據 | 位置 |
|---|---|
| 匯入 `CC3260MT.DLL`（BCB6 C RTL）、`BORLNDMM.DLL`、`STLPMT45.DLL`（STLport，BCB6 附帶） | PE import table |
| `.data` 起首字串 `Borland C++ - Copyright 2002 Borland Corporation` | 0x535000（20160530） |
| `C:\Program Files\Borland\CBuilder6\Imports\MSCommLib_OCX.cpp` / `ShockwaveFlashObjects_OCX.cpp` | .data |
| 匯出表含 `@@Ufrmctrl@Initialize` 等 BCB mangled 單元符號 | `exports_*.txt` |
| aimdtl.dll 匯入 `vcl60.bpl`/`rtl60.bpl` | aimdtl import table |

VCL 表單仍以 TPF0 二進位 DFM 存於 RCDATA，因此表單、元件、事件名稱 **100% 可還原**。另外每個函式的 `catch` 區塊都會寫 log 字串 `"<form>---<FunctionName>:"`，讓幾乎所有自訂函式（含非 published 的私有方法）都能被命名——這是本次還原精度高的主因。

---

## 1. 整體架構

```
hejian.exe (BCB6 VCL, 單一 EXE，靜態連結 VCL)
 ├─ TfrmMain      主框架：選單、ADOConnection、Timer1(100ms) 背景工作分派、log 寫檔、語系載入
 │    └─ ScrollBox1 內嵌各子表單（ShowSubForm）
 ├─ TfrmCtrl      噴印控制台（核心：組模板、設延遲、逐頁送資料、流水號/箱號）
 ├─ TfrmPrintMan  噴印資料管理（PrintData 表 CRUD、Excel 匯入/匯出）
 ├─ TfrmDataSelect 噴印資料選擇（分頁查詢 PrintData，雙擊帶回）
 ├─ TfrmCarton    紙箱規格管理（CartonSize 表 CRUD、Excel 匯入/匯出）
 ├─ TfrmProduct   生產記錄（ProductRecord 查詢/分頁/匯出）
 ├─ TfrmSystem    系統設定（密碼、DB 伺服器/建庫、合成模板路徑、匯入匯出路徑、備份還原、欄位最大長度）
 ├─ TfrmSuperset  超級設置（恢復預設密碼、噴頭延遲、橫向解析度、最大緩存、資料緩衝時間）
 ├─ TfrmLogin     密碼輸入（系統設定/超級設置前）
 ├─ TfrmPreview   預覽（把合成後 .ms 繪成圖，含 mm 尺規、縮放）
 ├─ TfrmStatus    噴印狀態小窗（已噴箱數/總箱數）
 ├─ TfrmAbout     關於（ProductName/版本/版權）
 └─ TSendThread   背景執行緒：每 400 ms 呼叫 frmCtrl->UpdatePageSN 回讀已噴頁序號
        │
        ▼ stdcall
 aimdtl.dll（Anser，2015-07-21，BCB6）：Printing PRO 的 ADT(Auto Data Transmit) 客戶端
        │  共用記憶體 "AIM_DATA_TRANSMIT_MAP" + 事件 AIM_DATA_TRANSMIT_{READ,WRITE,CONTROL_IN,CONTROL_OUT}_EVENT
        ▼
 Printing PRO（ctrler.exe，視窗類別 TfrmMain / 標題 "Printing PRO"）→ 4 個 Print Station（每站 2 噴頭，共 8 噴頭）
```

- 資料庫：MS SQL Server（預設 `.\SQLEXPRESS`，DB `hejian`），ADO `Provider=SQLOLEDB.1`（字串 0x5352CD 起，20160530 位址）。
- Excel：匯入用 **Excel COM Automation**（`Excel.Application` → `workbooks.open` → `UsedRange.Rows.Count` → `Cells(r,c).Value`）；讀工作表名稱用 ADO Jet 4.0（.xls）/ ACE 12.0（.xlsx）。匯出先 `CopyFileA(model\ExcelXXX.xlsx → 匯出路徑)` 再以 COM 寫 Cells。
- 設定檔 `hejian.xml` **實為 INI 格式**（TIniFile/TMemIniFile 讀寫，`GetHeJianInfo`/`GetIniData`/`SetIniData` 0x402678/0x40240C/0x402188）。
- 未使用序列埠：雖連結了 TMSComm（2.1.2.6）/TShockwaveFlash（20160530）元件包，但沒有任何 DFM 或程式碼使用，屬套件殘留。
- 未使用 Modbus、Winsock：無相關 import/字串。與噴印機唯一的介面是 aimdtl.dll。

### 1.1 主框架行為（TfrmMain）

| 函式 | VA | 行為 |
|---|---|---|
| 建構子 `frmMain---TfrmMain` | 0x401BD4 | 讀 `hejian.xml` [Data] `PrintMaxMem`→frmMain+0x34C、`PrintDataBuffer`→+0x350、`LanguageType`：1=`lanage\language.ini`(簡)，2=`lanage\TWlanguage.ini`(繁)，其他=`lanage\enlanguage.ini`；標題取語系鍵 `MainName` |
| `SetOpenADOCon`/`SetStringToData` | 0x4029B8/0x402D8C | `Provider=SQLOLEDB.1;Persist Security Info=False;User ID=<User>;Password=<Pass>;Data Source=<Name>;Initial Catalog=<Data>`（取自 [Server]） |
| `ShowSubForm` | 0x4030E8 | 子表單嵌入 ScrollBox1 |
| `FormShow` | 0x404A58 | 建立/套語系於 frmCtrl、frmPrintMan、frmCarton、frmProduct、frmSystem、frmSuper、frmAbout，預設開啟噴印控制 |
| `Timer1Timer`(100ms) | 0x404E28 | **延遲工作分派器**：依旗標呼叫 `SetOpenADOCon`、`frmCarton::SetTimerCartonData`（匯入/匯出）、`frmCtrl::SetPrintTimer`（啟動噴印）、`frmSystem::SetTimerSend`（建庫建表）、`frmCtrl::ShowYLTimer`（預覽）、`frmCtrl::SetStopPrint`。按鈕先顯示「請稍候」再由 Timer 執行長工作 |
| log 寫檔 | 0x404FD8 | `loginfo\YYYYMMDD.log`，每行 `<MainLogTime 語系字>YYYY-MM-DD hh:mm:ss   <訊息>` |
| `ShowMessage`(取語系字) / `ShowAlert`(MessageBox) | 0x4056D4/0x40450C | 所有 UI 訊息皆以鍵名查語系 ini [Language] |
| `FormCloseQuery` | 0x404804 | 噴印中 → `MainIsPrinting` 確認 → `SetStopPrint`；否則 `MainClearShow` |
| `GetSLJZToSJZ`/`GetSJZToSLJZ`/`GetShiLiuJZ`/`GetShiWei` | 0x403724… | 十六進位⇄十進位字串轉換（用於 .ms 的 DataSrc `Data=` hex 欄位表） |

---

## 2. 表單與控制項（含語系對照）

完整 DFM 文字檔：`delphi_hejian\dfm_2.1.2.6\*.dfm.txt`。標籤文字在執行期由語系 ini 依「控制項名稱=鍵」覆寫（例如 `frmCtrl---FormShow` 0x41AB20 依序套用 `txtCaseNo, txtMadeNoValue, txtBoxNoBeginValue, txtMachineNoValue1, btnReadData, gboxSet, gboxZM, gboxCM, gboxZXGG, gboxPSet, btnPreview, btnPrint, btnStop, cbxtype0, cbxtype1`）。三語對照表見 `language_map_2.1.2.6.tsv`（272 鍵）。

### 2.1 TfrmMain（鶴見噴印軟體）
- MainMenu1：`NPrintCtrl` 噴印控制 / `NPrintMan` 噴印資料管理 / `NCartonMan` 紙箱規格管理 / `NProduct` 生產記錄 / `NSystem` 系統設定 / `NSuperSet` 超級設置 / `NAbout` 關於（2.1.2.3 起）
- 非視覺：`ADOCon`(TADOConnection, LoginPrompt=False)、`TmpAdoQry`、`AdoQryEdit`（各子表單共用的查詢物件）、`Timer1`(100ms)、`StatusBar1`(2 panels，顯示連線/噴印狀態與「總發送頁數」)
- 事件：FormCloseQuery、FormResize、FormShow、各選單 OnClick

### 2.2 TfrmCtrl（噴印控制台）— 核心
| 群組 | 控制項（欄位偏移） | 意義 |
|---|---|---|
| gboxSet 基本設置 | `txtKeyCodeValue`(+2F4) Key Code；`txtMadeNoValue`(+2F8, MaxLen15) 製番NO；`txtBoxNoBeginValue`(+2FC, MaxLen6) 箱數（已噴/起始）；`txtBoxNoEndValue`(+300, MaxLen6) 「/ 總箱數」；`txtMachineNoValue1`(+304, MaxLen12) 機番前綴；`txtMachineNoValue2`(+308, MaxLen14, 僅數字) 機番流水號；`btnReadData` 讀取資料；`cbxtype`(+39C) 0=按順序噴印 / 1=按逆序噴印 | 作業輸入 |
| gboxZM 正嘜（唯讀顯示） | `txtModel/ModelValue`、`txtBore/BoreValue/BoreUnit`、`txtOutput/OutputValue/OutputUnit`、`txtPhaseValue/PhaseUnit`、`txtVoltageValue/VoltageUnit`、`txtHzValue/HzUnit`、`txtMadeNoValueResult`(+34C)、`txtTokki`(+350)、`txtCode/CodeValue`、`lblMacineNo`("NO:")、`txtMachineNoValueResult`(+35C) | 正面嘜頭欄位 |
| gboxCM 側嘜 | `txtModelValueSide`(+364)、`txtPlace`(+368) | 側面嘜頭 |
| gboxZXGG 紙箱規格 | `txtCaseNo`(+370)、`txtPrintWordHeight`、`txtOutSidePrintWordBegin/End`、`txtInsidePrintWordBegin/End`（唯讀） | 由 CartonSize 帶出 |
| gboxPSet 噴印操作 | `btnPrint` 噴印、`btnStop` 停止、`btnPreview` 預覽 | |
| 其他 | `gbxMsg/lblMSGDJS`（「正在停止，請稍候...」浮動訊息）、`TimerPrintStart`(500ms)、`TimerSendMsg`(400ms) | |

事件：FormClose(→SetStopPrint)、FormResize、FormShow、txtMadeNoValueChange(→複製到 txtMadeNoValueResult)、txtBoxNoBegin/EndValueChange(→SetValueToStatus 更新 frmStatus)、txtBoxNoBeginValueKeyPress(只允許數字)、txtMachineNoValue1/2Change(→ChangeNoText：Result = Value1 + Value2)、btnReadDataClick、btnPrintClick、btnStopClick、btnPreviewClick、TimerPrintStartTimer、TimerSendMsgTimer。

### 2.3 TfrmPrintMan（噴印資料管理）
與 frmCtrl 同版面欄位（可編輯）＋ gboxPSet「操作」：`btnAdd` 新增 / `btnEdit` 修改 / `btnDelete` 刪除 / `btnInsert` 導入 / `btnOutPut` 導出；`OpenDialog1`；`txtModelValueChange`（正嘜 ModelValue 同步到側嘜 txtModelValueSide，側嘜欄唯讀）。

### 2.4 TfrmDataSelect（噴印資料選擇）
`txtCondition` 搜尋條件、`btnSearch`、`btnPro` 上一頁、`btnNext` 下一頁、`lblpage/lblPageCount`、`DBGrid1`(OnDblClick 帶回呼叫端欄位)。

### 2.5 TfrmCarton（紙箱規格管理）
`Image1`（紙箱示意圖，DFM 內含 ~600KB 圖片）、`txtCaseNo`、`txtCartonSizeLength/Width/Height`、`txtPrintWordHeight` 印字高度、`txtOutSidePrintWordBegin/End` 表面印字開始/終了、`txtInsidePrintWordBegin/End` 裏面印字開始/終了（單位 mm，KeyPress 僅數字與小數點）、`DBGrid1`(OnCellClick 帶入編輯框)、新增/修改/刪除/導入/導出。

### 2.6 TfrmProduct（生產記錄）
`DateTimeBegin`~`DateTimeEnd`、`btnSearch`、`dbgList`、`btnPro/btnNext`、`lblpage`、`btnOutput` 導出、`Timer1`（延遲執行匯出）。

### 2.7 TfrmSystem（系統設定，PageControl）
- TabShPWD 密碼管理：舊/新/確認密碼（寫 [LoginPass] LoginPassWord）
- TabShServer 服務器：服務器/數據庫/用戶名/密碼（寫 [Server]），會檢查 DB 是否存在並重建
- TabShModel 合成模板：`txtSourceMsg1Path` 正嘜模板、`txtSourceMsg2Path` 側嘜模板（`Msg(*.ms)` 開檔）
- TabShInputOut 導入導出路徑：`txtImportPath`/`txtExportPath`（SHBrowseForFolder）
- TabShBakSystem 系統備份與還原：`RadioBtnbf`/`RadioBtnhy`
- TabShPrintMsgLen 噴印資料欄位最大長度（2.1.1.12 起）：20 個 `TCSpinEdit`（EditKeyCode…EditPlace）→ [DataLen]
- `btnSetData` 保存：依目前分頁呼叫 SetSavePWDClick / SetSaveServerClick / SetSaveModelClick / SetSavePathClick / SetSystemBFHY / SetInputDataLen（0x439BC4）

### 2.8 TfrmSuperset（超級設置）
tabEditPWD `cbx_defaultPWD` 恢復默認密碼；tabSetDelay 延遲1/延遲2 × 噴頭1~4（`txtYCOne1..txtYCFour2`，mm）；tabEditFBL `txtWidthResolution` 橫向分辨率（DPI，>0 且 ≤450）；TabSZDFC `txtEditMaxHC` 最大緩存（PrintMaxMem）；TabShHC `txtHCSJ` 數據緩衝時間（PrintDataBuffer，秒）；`btnSet` 設置。

### 2.9 其他
- TfrmLogin：`txtPassWord`、確定/取消；`btnOKClick` 比對 [LoginPass] `LoginPassWord`（系統設定）或 `CJSZPass`（超級設置），錯誤 `LoginPassError`。
- TfrmPreview：左 Y 尺規 `IMG_RulerY`、上 X 尺規 `IMG_RulerX`、`imgShow`、`TrackBar1` 縮放 10–100%，`lblfbl` 顯示 DPI，單位 mm。
- TfrmStatus：噴印箱數 / 總箱數 / 狀態文字。
- TfrmAbout：ProductName、檔案版本（VerQueryValue `\StringFileInfo\...\FileVersion`）、SoftShengMing。
- TLoginDialog / TPasswordDialog：VCL 內建對話框（ADO 預設），未使用。

---

## 3. 資料模型（SQL Server）

由 `frmSystem---CreatePrintDataTable`(0x43FDD0)、`CreateCartonSizeTable`(0x44083C)、`CreatePrintRecord`(0x440FA8) 動態組出 DDL（表名取 [Table]）：

```sql
create database <[Server]Data>;
create table PrintData (ID nvarchar(512) not null primary key,
  [Key Code] nvarchar(512) null, [Case-No] .., [MODEL] .., [MODEL_value] .., [BORE] .., [BORE_value] .., [BORE_unit] ..,
  [OUTPUT] .., [OUTPUT_value] .., [OUTPUT_unit] .., [PHASE_value] .., [PHASE_unit] .., [VOLTAGE_value] .., [VOLTAGE_unit] ..,
  [Hz_value] .., [Hz_unit] .., [CODE] .., [CODE_value] .., [TOKKI] .., [PLACE] ..);           -- 全部 nvarchar(512)
create table CartonSize (ID nvarchar(512) not null primary key,
  [Case-No], [Carton size-L], [Carton size-W], [Carton size-H],
  [CartonYZGD] /*印字高度*/, [CartonBMYZKS] /*表面印字開始*/, [CartonBMYZZL] /*表面印字終了*/,
  [CartonLMYZKS] /*裏面印字開始*/, [CartonLMYZZL] /*裏面印字終了*/);                          -- 全部 nvarchar(512)
create table ProductRecord (ID int primary key IDENTITY(1,1) not null,
  [Key Code], [MadeNo], [Case-No] nvarchar(512),
  [BeginPrintTime] datetime, [EndPrintTime] datetime,
  [BeginBoxNo], [EndBoxNo], [BeginMachineNo], [EndMachineNo] nvarchar(512));
```
（欄位型別片段 `" nvarchar(512) "`、`" datetime "`、`"primary key not null,"` 見 0x54ABAC/0x54ADD3 等；精確欄位順序以 CREATE 字串順序為準。）

主鍵 ID 以 `newid()` 產生（GUID 字串）。業務上的唯一鍵：PrintData 為 `[Key code]`，CartonSize 為 `[CASE-No]`（程式在 insert 前先 `select * ... where` 檢查）。**PrintData.[Case-No] → CartonSize.[CASE-No]** 為邏輯外鍵（無 FK 約束）。

### 3.1 全部 SQL（字串常值，以 `+` 串接、無參數化 → 有 SQL injection 風險）

| 用途 | SQL（`<T>` = 表名） | 來源函式 |
|---|---|---|
| 讀紙箱印字參數 | `select [CartonYZGD] as YZHeight,[CartonBMYZKS] as BMBegin,[CartonBMYZZL] as BMEnd,[CartonLMYZKS] as LMBegin,[CartonLMYZZL] as LMEnd from <CartonSize> where [CASE-NO]='<CaseNo>'` | frmCtrl/frmPrintMan `GetPrintHeight` 0x40C7A8/0x41DAC8 |
| 啟動前檢查紙箱存在 | `select * from <CartonSize> where [CASE-No]='…'`（不存在 → CartonNoThere） | `GetCheckIsNULL` |
| 資料選擇（分頁） | `select count(1) as allcount from <PrintData> where [Key code] like '%…%'`；`select [Key Code],[Case-No],…,PLACE from (select ROW_NUMBER() OVER(ORDER BY GETDATE()) AS Number,* from <PrintData> where [Key code] like '%…%') as TT where TT.Number between a and b` | `frmDataSelect---GetPrintData` 0x42CF04 |
| 噴印資料 新增 | `select * from <T> where [Key code]='…'`（存在 → PrintManDataCZ）；`insert into <T> (ID,[Key code],[CASE-No],[MODEL],…,PLACE) values (newid(),'…',…)` | `btnAddClick` 0x41E070 |
| 修改 | `update <T> set [CASE-No]='…',[MODEL]='…',…,[PLACE]='…' where [Key code]='…'` | `btnEditClick` 0x4204FC |
| 刪除 | `delete <T> where [Key code]='…'` | `btnDeleteClick` 0x41F794 |
| 紙箱 CRUD | `select [CASE-No],[Carton size-L],…,CartonYZGD as <語系字>,…` 列表；`insert/update/delete … where [CASE-No]='…'` | frmCarton 0x42EE70/0x4301BC/0x43119C/0x430C38 |
| Excel 匯入（通用） | 每列：`select * from <T> where [<key>]='…'` → `delete <T> where [<key>]='…'` → `insert into <T> ([ID],<cols>) values (newid(),…)`（**同鍵覆蓋**） | `GetExcelDataToBase` 0x406690（PrintData）/0x435EB8（CartonSize） |
| 匯出 | `select [Key Code],…,PLACE from <PrintData>` / `select [CASE-No],…,[CartonLMYZZL] from <CartonSize>` / `select [Key Code],[MadeNo],[Case-No],BeginBoxNo,EndBoxNo,BeginMachineNo,EndMachineNo,BeginPrintTime,EndPrintTime from <ProductRecord>` | `GetDataToExcel` ×3 |
| 開始噴印記錄 | `insert into <ProductRecord> ([Key code],[MadeNo],[Case-No],[BeginPrintTime],[BeginBoxNo],[BeginMachineNo]) values ('KeyCode','MadeNo','CaseNo',CONVERT(VARCHAR(100), GETDATE(), 121),'<txtBoxNoBegin>','<txtMachineNoValueResult>')` | `InserProductRecord` 0x4185CC |
| 結束噴印記錄 | `update <ProductRecord> set [EndPrintTime]=CONVERT(VARCHAR(100), GETDATE(), 121),[EndBoxNo]='<txtBoxNoBegin 當下值>',[EndMachineNo]='<txtMachineNoValueResult 當下值>' where ID=(select max(ID) as MaxID from <ProductRecord>)` | `SetEndProductInfo` 0x417964 |
| 生產記錄查詢 | 條件 `BeginPrintTime>='yyyy-MM-dd 00:00:00' and EndPrintTime <= 'yyyy-MM-dd 23:59:59' and EndBoxNo!=''`；`select [Key Code],[MadeNo] as [製番],[Case-No] as [紙箱編號],BeginBoxNo as […],EndBoxNo,BeginMachineNo,EndMachineNo,BeginPrintTime,EndPrintTime … ROW_NUMBER() … order by TT.BeginPrintTime desc`，每頁 [Table]ProductPageSize | `GetProductData` 0x4463C0 |
| 清理未結束記錄 | `delete ProductRecord where EndBoxNo=''`（**表名寫死**，開啟/查詢生產記錄時執行） | `SetDeleteProduct` 0x4478F4 |
| 檢查/刪除 DB | `USE MASTER SELECT COUNT(*) as DataCount FROM MASTER..SYSDATABASES WHERE NAME ='…'`；`USE MASTER ALTER DATABASE <db> SET SINGLE_USER WITH ROLLBACK IMMEDIATE  DROP DATABASE <db>` | `CheckDataBaseExist` 0x43D370 / `DeleteDataBase` 0x43D114 |

### 3.2 Excel 檔格式
- `hejian\PrintData.xls`（工作表 PrintData，20 欄）：`Key code, CASE-No, MODEL, MODEL_value, BORE, BORE_value, BORE_unit, OUTPUT, OUTPUT_value, OUTPUT_unit, PHASE_value, PHASE_unit, VOLTAGE_value, VOLTAGE_unit, Hz_value, Hz_unit, CODE, CODE_value, TOKKI, PLACE`。範例：`IA01HN-2DS51, 01156, MODEL, HSN2.4S-51, BORE, 50, mm, OUTPUT, 0.4, kW, 1, 相, 100, V, 50, Hz, CODE, A01HN-2DS51, , MADE BY TSURUMI TAIWAN`（MODEL/BORE/OUTPUT/CODE 欄存的是「標籤文字」）。
- `hejian\CartonSize.xls`（工作表 Carton-Size，9 欄）：`CASE-No, Carton size-L, Carton size-W, Carton size-H, 印字高度, 表面印字開始, 表面印字終了, 裏面印字開始, 裏面印字終了`（例 `01153, 330, 225, 538, 248, 25, 530, 25, 530`）。
- 匯入時驗證：表面/裏面印字終了不得大於 L+W（`CartonOutSideDYLW`/`CartonInSideDYLW`），欄位長度不得超過 [DataLen]（`GetCheckDataLen` 0x425CC8，`PrintManMaxLenLeft` 列出超長欄位）。
- 匯出模板：`model\ExcelPrintData.xlsx`、`ExcelCarton.xlsx`、`ExcelProduct.xlsx`（[SourceMsg] `ExcelVersion`=2 用 .xlsx，否則 .xls），先 CopyFile 至 `SavePathOutput`，部分欄位先設 `NumberFormat`（文字格式，保留前導 0）。

---

## 4. 設定檔 hejian.xml（INI 格式）鍵說明

| 區段/鍵 | 範例值 | 意義（使用處） |
|---|---|---|
| [Server] User / Pass / Name / Data / DataName | sa / (略) / `.\SQLEXPRESS` / hejian / hejian | ADO 連線；DataName 用於建庫/檢查存在 |
| [Table] PrintData / CartonSize / ProductRecord | 同名 | 表名（可改名） |
| [Table] ProductPageSize | 20 | 生產記錄每頁筆數 |
| [SourceMsg] SourceMsg1Path / SourceMsg2Path | `...\msg\Test_8PH_A_Logo.ms` / `..._B_Logo.ms` | 正嘜 / 側嘜 原始模板 |
| [SourceMsg] SavePathInput / SavePathOutput | `E:\lxsp2000` / `D:\bak` | Excel 匯入預設目錄 / 匯出目錄 |
| [SourceMsg] ProductName / SoftShengMing | HeJianPrintingConsole / Copyright Anser 2016 | 關於畫面 |
| [SourceMsg] ExcelVersion | 2 | 匯出模板 .xlsx(2) / .xls |
| [LoginPass] LoginPassWord / CJSZPass | (略) | 系統設定密碼 / 超級設置密碼（明文存放）；「恢復默認密碼」會把 LoginPassWord 寫回程式內建預設值（0x54F6EA 字串，報告不列出） |
| [DataLen] KeyCode…Place（20 鍵） | 50,49,48…31 | 各欄最大字元數（匯入/新增/修改驗證）。目前值為遞減序列，像測試資料 |
| [Data] DPI | 400 | 橫向解析度 dots/inch（mm→dot 換算、延遲換算、預覽） |
| [Data] PerInch | 25.4 | mm/inch |
| [Data] PrintMaxMem | 20 | **在途頁數上限**：已送出未噴出的頁數 ≤ 此值才再送（frmMain+0x34C，TimerSendMsgTimer） |
| [Data] MaxPrintHeadCount / SinglePrintHeadHeight | 8 / 150 | 噴頭數與單噴頭高度（dot），合成模板高度 = 8×150 = 1200 / 側 |
| [Data] iAdtDataMaxLen / iOneAdtField / AdtFieldCount | 2601 / 52 / 18 | .ms `[DataSrc0] Data=` 的 hex 結構：1 byte 欄位數 + N×52 bytes（1 byte 旗標 + 51 bytes 名稱），最大 1+50×52=2601；ADT 欄位數必須 = 18 |
| [Data] PrintDataBuffer | 30 | 啟動後「資料緩衝中，請在 N 秒後放入紙箱」倒數秒數 |
| [Data] SpaceBtVoltageHzValue | 5 | 電壓與頻率間插入的空白數（`220V     60Hz`） |
| [Data] PrintHead{1..4}FixedDelay{1,2} | 70/0, 310/0, 60/0, 320/0 | 各噴頭組固定延遲（mm）兩段相加 |
| [Data] LanguageType | 2 | 1 簡體、2 繁體、其他 英文 |
| （歷史鍵，2.1.1.11 以前）ModelValueFrontFontName/Size/Style、ModelValueSideFont*、PlaceFont* | | 2.1.1.12 移除，改由側嘜模板 Object1/Object2 自身字型決定 |

語系檔 `lanage\language.ini`(GBK)、`TWlanguage.ini`(Big5)、`enlanguage.ini`(ASCII)，單一區段 [Language]，鍵名 = 控制項名稱或訊息代碼。

---

## 5. 噴印模板（.ms）與欄位組成

### 5.1 .ms 檔格式（Printing PRO 訊息檔，INI 文字）
```
[MsgInfo]  Ver=6 No ID Name Width(dot) Height(dot) Angle Count(物件數) daCount(資料源數) dsid
[ObjectN]  Type(1=文字,7=條碼…) X Y ZoomX ZoomY Width Height Angle Hide Space UsedPSFont FontSize Text FontName StyleType SimpleData
           MsgObjT0=<資料源群組>,<ADT 欄位索引>,<範例文字>,      ← 物件綁定 ADT 第 n 欄
[DataSrc0] UID=ADSADT  Name=ADT  Group=0  Data=<hex：欄位數 + 52B×N 欄位名>
```
`msg\AdtTemp.ms` 的 18 個 ADT 欄位（索引→名稱）：0 Model, 1 ModelValueFront, 2 Bore, 3 BoreValue, 4 Output, 5 OutputValue, 6 PhaseValue, 7 PhaseUnit, 8 VoltageValue, 9 MadeNo, 10 Tokki, 11 CodeValueAndMachineNo, 12 Code, 13 CodeValue, 14 MahcineNo(原文拼錯), 15 MachineNoValue, 16 ModelValueSide, 17 Place。正嘜模板 Test_8PH_A_Logo.ms 含 0–15，側嘜 Test_8PH_B_Logo.ms 含 16–17。條碼物件（Type=7）綁 9 MadeNo 與 11 CodeValueAndMachineNo。

### 5.2 模板合成（啟動噴印時，每次重做）
`SetPrintTimer`(0x417D44) 流程中：
1. `EditOutSideObj`(0x40F83C)：讀側嘜模板 Object1/Object2 的 FontName/FontSize/StyleType，用 `SetTextToImage`(0x40F3BC) 以該字型把 ModelValueSide / Place 文字畫到 TImage（另存 .bmp）以量出寬高，回寫 Object1/2 的 Width/Height/Text/SimpleData/MsgObjT0 與 MsgInfo Width/Height（側嘜文字長度可變）。
2. `GetAllGroupMSG`(0x410E3C)：
   - 表面間距(dot) = (表面印字終了 − 表面印字開始)/PerInch×DPI − 正嘜寬(+0xAD8) − 側嘜寬(+0xAE0)
   - 裏面間距同理用裏面印字開始/終了
   - `GroupMsgWhole`(0x4112E8) → `GroupMsgModel`(0x4115F4)×3：正嘜 + 間距 + 側嘜 橫向拼成 `msg\TempOutSide.ms`、`msg\TempInSide.ms`（各高 MaxPrintHeadCount×SinglePrintHeadHeight=1200）；再把兩者縱向疊成 `msg\AdtTemp.ms`（高 2400，Count=38）。`GetDataFormMsg`(0x4127E8) 解析物件/DataSrc（用 iAdtDataMaxLen、iOneAdtField、hex 轉換），`GetMsgInfoText`(0x414964) 重寫 [MsgInfo]，`SetMsgOBJT` 調整 MsgObjT0。
   - 含意：一側噴頭從紙箱 L 面「印字開始」處印正嘜，到 L+W 範圍內「印字終了」處結束於側嘜（跨兩個面），因此 終了 ≤ L+W。
3. 檢查三檔存在，`imGetAdtMsgFieldCount(AdtTemp.ms)` 必須等於 [Data]AdtFieldCount（否則 `CtrlNoAllADTCount`）。

### 5.3 每頁（每箱）送出的 18 個欄位 — `SetValueToStr`(0x40D714) + `SetDataToPrintPro`(0x40E5B8)
`SetValueToStr` 在啟動時把畫面值快取到成員字串；`SetDataToPrintPro` 依下列**固定順序**呼叫 `imAppendPageField(1, text, len+1)`（順序即 ADT 欄位索引）：

| # | 送出內容 | 來源 |
|---|---|---|
|0| Model 標籤 | txtModel |
|1| ModelValue | txtModelValue |
|2| Bore 標籤 | txtBore |
|3| BoreValue（+BoreUnit） | txtBoreValue/Unit（GetNULLStr 處理空值） |
|4| Output 標籤 | txtOutput |
|5| OutputValue（+Unit） | txtOutputValue/Unit |
|6| PhaseValue | txtPhaseValue |
|7| PhaseUnit | txtPhaseUnit |
|8| AllValueData = VoltageValue+VoltageUnit + 空白×SpaceBtVoltageHzValue + HzValue+HzUnit | 例 `220V     60Hz` |
|9| MadeNo（製番，條碼） | txtMadeNoValue |
|10| Tokki | txtTokki |
|11| Code = CodeValue + `"   "` + 機番前綴 + 流水號 | 例 `A01RHE-DS02   ST-123450001`（條碼） |
|12| CodeDian（"CODE:" 標籤） | txtCode |
|13| CodeValue | txtCodeValue |
|14| MacineNo（"No:" 標籤） | lblMacineNo |
|15| MacineNoValue = 機番前綴 + 流水號 | txtMachineNoValue1 + 序號 |
|16| ModelValueSide | txtModelValueSide |
|17| Place | txtPlace |

任一欄 `imAppendPageField` ≠1 → 該頁失敗（回傳 false）。

---

## 6. 噴印機（Printing PRO / ADT）互動

### 6.1 aimdtl.dll API（stdcall；由呼叫端推得的參數）

| 匯出 | 呼叫方式 | 底層 `imDtControl(cmd, in, inLen, out, outLen, timeout)` |
|---|---|---|
| imOpenPrinter() | 啟動時 `GetOpenPrintStatus` 0x40C270；回傳 0/−1/−2/其他 對應 `CtrlPrintOpenSucc/CtrlPrintNoMem/CtrlPrintOtherZY/CtrlLJFail`（對應關係為推測） | imDtOpen：開共用記憶體、檢查 ctrler.exe |
| imInitADT(struct*) | struct 0x2C bytes：byte[0]=1, dword[4]=0, byte[8]=1, 其餘 0 | cmd 0x13，回 0xB→2 |
| imAppendMessage(buf, len, 0, 0) | 傳整個 AdtTemp.ms 檔內容（ReadFile 全檔+NUL） | 寫共用記憶體 |
| imPrinterStart()/imPrinterStop() | | cmd 1 / 2 |
| imSendCancel(), imClearPrintQueue(), imClearReturnQueue() | 停止時依序呼叫 | cmd 6 / 7 |
| imGetADTStatus(0) | 回 1 = 可寫入下一頁（WaitForSingleObject 寫事件 1ms） | |
| imPageBegin(pageSN) / imAppendPageField(1, str, len) / imPageEnd() | 每頁一次 | imDtWriteBegin/AppendField/WriteEnd |
| imGetPrinttedPagesn(int*) | 已噴頁序號 | cmd 0x10 |
| imGetPrinterStatus(byte*) | 0 停止、1 噴印中（狀態列跑點點）、2 停止類、3 → 查警報（推測） | cmd 0x11 |
| imGetPrinterAlarm(byte*) | 0..4 → 無警報/卡匣警告/卡匣錯誤/系統警告/系統錯誤（`CtrlNoAlarm…CtrlSysError`） | cmd 0x12 |
| imGetAdtMsgFieldCount(path) | 讀 .ms 的 DataSrc 欄位數 | 本地解析 |
| imGetLgphDelay(i) | 迴圈 i=0.. 直到回傳 <0 → 邏輯噴頭組數 | cmd 0x15 |
| imSetLgphDelay(i, delay) | delay 單位 0.01 mm：`(long)((mm+0.01)*100)`，失敗重試 3 次（0x419ACC） | cmd 0x16 |
| （未被 hejian 使用）imGetDPI 0x14、imGetPHsHeight 9、imCheckTrigger 8、imSetVMirrorCount 0xA、imCheckRegFunc 0x17、imGetNewPrinttedPagesn 0x18、imFetchInkInfo（讀 InkInfo.INI：VolumeLoad/VolumeCur/DotsPrinted…）、imClosePrinter、imPageBeginExt、imCheckIsAdtMsg、imGetAdtMsgFieldType、imGetADTVersion("V2.0") | | |

Printing PRO 端 log（`鶴見\20160122\logs\Log_COM*.log`）印證：`DoLoadMsgStruct` → 每頁 `DoConvertMsg pagesn=N` → `SendPageToPs [站序號] pagesn` → `imPrintData index=0..3,sn=95010252/95010250/95010226/95010402` → `DoSend SendOK pagesn=N`，即 4 個 Print Station（index 0–3 = 4 個邏輯噴頭組）。

### 6.2 噴印啟動序列
1. `btnPrintClick`(0x40CEAC)：`GetCheckIsNULL` 驗證 → 顯示 `CtrlIsPrepareModel` → 設旗標，交由 frmMain.Timer1 呼叫 `SetPrintTimer`。
2. `SetPrintTimer`(0x417D44)：模板合成（§5.2）→ `CtrlIsSetDelaying` → `SetPrintDelayToPro` → `SetPrintToPrintPro` → 記錄旗標、`b4c = PrintDataBuffer`（倒數秒）、Sleep。
3. `SetPrintDelayToPro`(0x418B30) 延遲計算：
   - wIn/wOut = TempInSide/TempOutSide 的 MsgInfo Width；若 wIn ≤ wOut：diff = (wOut − wIn)/DPI×PerInch（mm）
   - 噴頭組1 = H1D1+H1D2+表面印字開始；組2 = H2D1+H2D2+表面印字開始
   - 組3 = H3D1+H3D2+裏面印字開始 − diff；組4 = H4D1+H4D2+裏面印字開始 − diff
   - 任一 <0 → `CtrlDelayNoInt`；對 i < imGetLgphDelay 計數逐一 `SetPrintHeadDelay(i, d)`。
   - 即：組1/2 = 表面（外側）噴頭，組3/4 = 裏面（內側）噴頭；以「印字開始(mm)」作為從光電觸發到開始噴的距離。
4. `SetPrintToPrintPro`(0x40D0C4)：`imInitADT` → `SetStopPrint`（清佇列）→ `SendMessageToADT(AdtTemp.ms)`（`imAppendMessage`）→ `SetValueToStr` → `imPrinterStart` → 噴印中旗標 +0xB6C=1 → `InserProductRecord`。
5. `TimerPrintStart`(500ms) → `GetPrintStatus` + `SetTimerPageSN`：顯示「數據緩衝中，請在 N 秒後放入紙箱...」倒數（每累積 1000ms 減 1），之後「正在準備噴印中」。
6. **`TimerSendMsgTimer`(400ms, 0x40D334) 送頁迴圈**（即「buffer pages」機制）：
   ```
   if 噴印中 && 在途頁數(+B3C) <= PrintMaxMem && imGetADTStatus(0)==1:
       if 已噴箱數(+3F4) >= 總箱數(+3F8):  訊息 CtrlPrintClearCarton1（噴印完成）→ 結束
       elif [僅 2.1.2.6] 總箱數 >= 已噴流水號(+408):  訊息 CtrlPrintClearCarton2 → 結束
       if 結束: SetStopPrint；MessageBox(OK/Cancel)「是否清空箱數與機番?」→ OK 則清空 txtBoxNoBegin/End、txtMachineNoValue1/2
       elif 已送頁數(+B40) < 待送頁數(+400 = 總箱數−起始箱數):
           imPageBegin(pageSN +48C)；if SetDataToPrintPro(): pageSN++, 已送++, 在途++；imPageEnd()
   ```
7. **`TSendThread`**（0x44C504）：CreateEvent，每 400 ms（`WaitForSingleObject(evt, 0x190)`）呼叫 `UpdatePageSN`(0x419BBC)：`imGetPrinttedPagesn` 成功且 已噴箱數<總箱數 → 已噴箱數++ 並寫回 `txtBoxNoBeginValue`；顯示用流水號(+408) 依 cbxtype ±1 並寫回 `txtMachineNoValue2`；在途頁數−−；狀態列顯示 `CtrlAllSendCount`+已送頁數。
8. 停止 `btnStopClick`→`SetStopPrint`(0x417898)：`imSendCancel` → `imClearPrintQueue` → `imClearReturnQueue` → `imPrinterStop` → 若原本噴印中則 `SetEndProductInfo`（更新最後一筆 ProductRecord）。

### 6.3 狀態偵測
`GetPrintStatus`(0x40B760) 依 `imGetPrinterStatus` 切換 UI 致能（`SetControlEnable` 0x417548，噴印中鎖住輸入）、狀態列動畫；狀態 3 時 `GetAlarmStatus` 顯示卡匣/系統警告錯誤（提示「請打開 Printing PRO 查看」）。

---

## 7. 流水號（機番）與箱號邏輯，含 20160530 變更

### 7.1 名詞
- **箱數** `txtBoxNoBeginValue` = 已噴（起始）箱數；`/ txtBoxNoEndValue` = 總箱數。待噴頁數 = 總 − 起始（必須起始 < 總，`CtrlCaptionBeginAndEnd`）。
- **機番** = `txtMachineNoValue1`（前綴，如 `ST-12345`）+ `txtMachineNoValue2`（純數字流水號）。內部以 **double** 保存（`atof`，+0xAF0 送出用、+0x408 已噴顯示用），轉字串用 FloatToStr → **前導 0 會遺失**（例：`0001` → `1`）；log 中可見 `Printing Serial Number:87878 XiangHao:0`。
- 每送一頁（`SetDataToPrintPro`）：log `Printing Serial Number:<序號>   XiangHao:<箱號>`；cbxtype=0 序號 +1，=1（逆序）序號 −1；箱號 +3FC +1；MachineNoValue 重新組合。
- 每確認噴出一頁（`UpdatePageSN`）：畫面箱數 +1、畫面流水號 ±1 → 停止後畫面即為「下一箱」，可直接續噴。

### 7.2 啟動驗證 `GetCheckIsNULL`（2.1.2.6：0x415704，長 0x1E42）
依序：Key Code 不可空 → 製番不可空 → 起始箱數不可空（並設 pageSN +48C = 起始箱數）→ 總箱數不可空 → 起始 < 總 → 流水號空則視為 0（存入 +0xAF0 與 +0x408）→ 前綴為空 **或** 流水號=0 → `CtrlNoValueMachine`「機番不可為空」 → 流水號字串長度 > 欄位 MaxLength（推測）→ `CtrlPrintCGMax` → **[僅 2.1.2.6] `(double)總箱數 >= atof(流水號)` → `CtrlPrintCGCanPrint`「噴印箱數超過可用機番」** → 印字高度/表面開始/表面終了/裏面開始/裏面終了不可空 → 正嘜模板存在且 Width>0 → 側嘜模板存在且 Width>0 → CartonSize 中紙箱編號存在。

### 7.3 「改流水號判斷」（20160530）實際差異
二進位比對（`diff_20160530_serialcheck.txt`）：整支程式只有兩處邏輯改變，其餘函式長度完全相同：

| 位置 | 2.1.2.6 | 20160530 |
|---|---|---|
| `GetCheckIsNULL` | 0x41606F–0x4161E2 有「總箱數 ≥ 流水號 → 拒絕啟動」區塊（字串 `CtrlPrintCGCanPrint`） | 整段移除（函式短 0xB0；0x415F74 `CtrlPrintCGMax` 後直接接 `CtrlCartonCGMaxHeight`），字串也從 .data 消失 |
| `TimerSendMsgTimer` | 0x40D435：`fild [總箱數]; fcomp [已噴流水號]` → 若 總箱數 ≥ 流水號 → `CtrlPrintClearCarton2`「噴印機番不能小於箱數」並停止 | 比較指令仍在但結果不再使用（0x40D44A 直接判斷旗標），`CtrlPrintClearCarton2` 字串移除 |

**解讀**：舊規則要求「流水號數值 > 總箱數」（原意應是逆序噴印時流水號遞減不可小於 1），但它不分順序/逆序一律套用，導致小流水號（如新批次從 0001 起）在順序模式也無法開始或中途被停。20160530 版把兩處檢查都拿掉，流水號不再與箱數比較。保留的只有長度上限與非空檢查。新系統應：依方向檢查「逆序時 起始流水號 − 待噴數 ≥ 下限」，並以字串＋固定位數保存流水號（保留前導 0）。

### 7.4 其他觀察到的流水號/記錄風險（供新系統避免）
- 送出序號(+AF0)與確認序號(+408)分離；若中途停止，已送未噴（在途 ≤ PrintMaxMem）的頁會被 `imClearPrintQueue` 丟棄，畫面以「已噴」為準，故續噴不跳號——但「Printing Serial Number」log 是在**送出**時寫的，重送時同一序號會再出現一次（`hejian_2.1.2.6\loginfo\20160308.log` 中同一序號出現 3–4 次，推測為測試時多次停止/續噴，未逐筆驗證）。
- `SetEndProductInfo` 以 `max(ID)` 更新最後一筆，不保證是本次那筆。
- `delete ProductRecord where EndBoxNo=''` 會刪掉程式異常結束時的進行中記錄。
- 所有 SQL 為字串串接。

---

## 8. 其他功能流程

- **讀取資料**（frmCtrl/frmPrintMan `btnReadDataClick` 0x40C434/0x41D770）：以 Key Code 模糊查詢開 frmDataSelect，雙擊一列（`DBGrid1DblClick` 0x42DCC0）帶回 20 欄到呼叫端，再 `GetPrintHeight(CaseNo)` 帶出紙箱印字參數。
- **預覽**（`btnPreviewClick` 0x41A490 → Timer → `ShowYLTimer` 0x41A62C）：合成到 `msg\Preview.ms`，`frmPreview---ShowImageToUi`/`ShowMsgTolist` 依物件畫文字與條碼（程式內含 Code128 編碼表 0x555758 起），`DrawRulerX` 畫 mm 尺規（舊版 log 有 EDivByZero/EInvalidOp）。
- **系統備份/還原**（`SetSystemBFHY` 0x43E198、`SystemDataSet` 0x43EBA0）：只複製 `hejian.xml` 與 `msg\`（含正/側嘜模板）到選定資料夾或從中還原（DeleteFile→Sleep→CopyFile）；**不備份資料庫**。
- **服務器設定**（`SetSaveServerClick` 0x43C330）：驗證非空 → 測試連線 → 寫 [Server] → 若 DB 已存在詢問是否清空重建（DROP）→ 由 Timer 呼叫 `SetTimerSend` 建庫建三表。
- **超級設置**：各分頁驗證後寫 [Data]（延遲 8 值、DPI、PrintMaxMem、PrintDataBuffer），或把登入密碼恢復預設。
- **生產記錄匯出**：Timer 觸發 `GetDataToExcel` 寫 `ExcelProduct.xlsx` 複本。

---

## 9. 版本歷史（由 PE link time、FileVersion、DFM 雜湊、字串集合差異推得；`version_history_raw.txt`）

| 版本 | link (UTC) | 主要變化 |
|---|---|---|
| 2.1.0.7 | 2015-11-17 00:49 | 初版：全部 13 表單；cbxtype 項目寫死於 DFM；機番前綴 MaxLen16、流水號 MaxLen9；表單控制項命名 `textBox*` |
| 2.1.0.8 | 11-17 03:28 | frmMain 表單調整 |
| 2.1.0.9 | 11-17 07:29 | 新增 `LanguageType`、`GetHeJianInfo`；移除中文欄名 SQL（`[印字高度]`…）改英文欄名 |
| 2.1.1.0/2.1.1.1 | 11-17 | CartonSize 欄位改為 `CartonYZGD/BMYZKS/BMYZZL/LMYZKS/LMYZZL` |
| 2.1.1.3 | 11-18 | frmCtrl 調整 |
| 2.1.1.4 | 11-19 | 加 `ExcelVersion`、`SetTextToImage`、`SetAppendPageFieldPic`、`txtModelValueChange` |
| 2.1.1.5 | 11-19 | 全面語系化（控制項名稱作為語系鍵）、加 FormShow |
| 2.1.1.7 | 12-07 | 移除「噴頭數必須為4」(`CtrlNoFourPrintHead`)、`CtrlNoSendAllDelay` 檢查 |
| 2.1.1.8–2.1.1.10 | 12-21~12-23 | frmCtrl/frmPrintMan/frmCarton 版面 |
| 2.1.1.11 | 12-25 | 送頁欄位改為固定 18 欄名（AllValueData、CodeDian、MacineNoValue…） |
| 2.1.1.12 | 2016-01-06 | 加 [DataLen] 欄位長度設定頁（TCSpinEdit）、ProductRecord 分頁（ROW_NUMBER）、`delete ProductRecord where EndBoxNo=''`；移除側嘜字型 xml 鍵 |
| 2.1.1.14 | 01-13 | Excel 匯出改用 model\ 模板 + COM（`GetDataToExcel`） |
| 2.1.1.15 | 01-13 | 路徑 `MSG\`→`msg\` |
| 2.1.1.16 | 01-13 | Excel 匯入改為 COM `GetExcelDataToBase`（移除 ADO `SetInsertData`），同鍵 delete+insert |
| 2.1.1.17–2.1.2.2 | 01-15~01-18 | 路徑選擇 `CheckDir`、ExcelProduct.xls |
| 2.1.2.3 | 01-21 | 新增 TfrmAbout、紙箱「終了 ≤ L+W」驗證、匯入/匯出路徑存在檢查 |
| 2.1.2.5 | 01-22 | 生產記錄欄位順序調整 |
| 2.1.2.6 | 02-26 | frmCtrl 僅 DFM 屬性（ImeName）改變 |
| 20160530-2 | 05-30 | FileVersion 仍 2.1.2.6；移除 `CtrlPrintCGCanPrint` 與 `CtrlPrintClearCarton2` 兩個流水號 vs 箱數檢查（§7.3）；元件包 TMSComm→TShockwaveFlash（未使用） |

aimdtl.dll（2015-07-21）與 stlpmt45.dll（2014-09-15）在所有版本相同；hejian.exe 匯入的 18 個 aimdtl 函式各版不變。

---

## 10. 無法還原 / 不確定事項

1. **原始碼層級細節**：私有成員變數名稱（僅有偏移，如 +0x3F4/+0x3F8/+0x408/+0xAF0/+0xB3C/+0xB40/+0x48C/+0xB6C，本報告依用途命名）、區域變數、註解。
2. **VCL/RTL 內部呼叫**為靜態連結且無符號，部分呼叫（如 0x50A000=GetText、0x50A030=SetText、0x4A9BC8=StrToInt、0x4047E0=Length、0x4AB560=FloatToStr）是依行為推定。
3. `imOpenPrinter` 回傳值與四個訊息的精確對應、`imGetPrinterStatus` 狀態 2 的意義、`imGetPrinterAlarm` 0–4 對應順序：依字串載入順序推測，未以執行驗證。
4. `CtrlPrintCGMax` 的比較對象（推測為 txtMachineNoValue2 的 MaxLength）。
5. Printing PRO（ctrler.exe）端的共用記憶體版面、ADT 協定位元組格式、各 cmd 的回應結構：只還原到 aimdtl 的 cmd 代碼層級；`AIM_DATA_TRANSMIT_MAP` 內部結構（如 +0x238、+0x28C 偏移）未完整解析。ctrler.exe 本身不在分析範圍內。
6. 條碼繪製（Code128 表）只用於預覽的推論；實際噴印條碼由 Printing PRO 依 .ms Type=7 物件產生。
7. `SetTextToImage` 產生的 .bmp 檔名/存放位置、`GroupMsgModel` 物件座標平移與 X 偏移的精確公式（只確認了間距計算與三次合成呼叫）。
8. `ExcelInputSucc1/2/21`（成功/重複筆數）訊息在目前版本已無引用，重複判定細節以「同鍵先刪後插」為準。
9. 2.1.1.18 目錄為空（僅有安裝程式 .exe 未解包），該版差異未知；各 `.rar/.7z/.zip/安裝檔` 未解開比對。
10. 未執行程式，所有 UI 外觀以 DFM 座標/屬性為準；執行期以語系覆寫的實際文字以 ini 為準。
11. 未讀 `鶴見噴印控制臺使用手冊.doc`、`軟體需求規格書` 等文件（非本任務輸入），其中可能有更正式的業務定義可交叉驗證。

---

## 11. 產出檔案（`D:\dev_program\MITSU\analysis\delphi_hejian\`）

| 檔案 | 內容 |
|---|---|
| `dfm_2.1.2.6\`、`dfm_20160530\`、`dfm_2.1.2.5\`、`dfm_2.1.0.7\` | 15 個 TPF0 表單轉文字 DFM |
| `strings_2.1.2.6.txt`、`strings_20160530.txt`、`strings_aimdtl.txt` | ASCII/GBK 與 UTF‑16 字串含 VA |
| `exports_*.txt` | 匯出（含位址）與匯入表 |
| `code_<ver>.summary.txt` | 每個函式：名稱（VMT/log 字串推得）、呼叫、引用字串 |
| `code_<ver>.disasm.txt` | 全部應用程式函式反組譯，註記字串、API、published 欄位名 |
| `code_<ver>.fields.txt`、`.names.json` | 各表單 published 欄位偏移、事件處理常式位址 |
| `code_aimdtl.*` | aimdtl.dll 反組譯 |
| `key_functions_brief_2.1.2.6.txt` | 核心噴印函式精簡反組譯 |
| `diff_20160530_serialcheck.txt` | 流水號檢查兩版對照 |
| `language_map_2.1.2.6.tsv` | 272 個語系鍵 簡/繁/英 對照 |
| `version_history_raw.txt` | 各版自動差異 |
| `tools\*.py` | dfm.py（TPF0 解析器）、ana.py（capstone 反組譯/命名）、strs.py、exports.py、hist.py、getfn.py |
