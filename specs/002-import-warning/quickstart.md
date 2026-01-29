# Quickstart: Excel 匯入警告分級

**Feature Branch**: `002-import-warning`
**Date**: 2026-01-29

## 環境需求

- .NET 8 SDK
- Windows 10/11（WPF 應用程式）
- Visual Studio 2022+ 或 VS Code

## 建置與執行

```bash
# 從 WSL 執行（透過 PowerShell 穿透）
cd /mnt/d/Daomen/NanmaoLabel

# 建置
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet build NanmaoLabelPOC"

# 執行
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet run --project NanmaoLabelPOC"

# 測試
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet test NanmaoLabelPOC.Tests"
```

## 功能驗證步驟

### 1. 欄位名稱底線支援 (FR-001)

1. 準備包含 `nvr_cust`、`nvr_cust_item_no` 欄位的 Excel 檔案
2. 點擊「匯入」按鈕，選擇該檔案
3. **預期結果**：匯入成功，無「非法字元」警告

### 2. 訊息分級顯示 (FR-003~FR-006)

**Error 等級測試**：
1. 點擊「匯入」按鈕
2. 選擇不存在的檔案路徑（或非 .xlsx 檔案）
3. **預期結果**：顯示 Error 對話框，匯入中斷

**Warning 等級測試**：
1. 準備 Excel 檔案，其中 `pono` 欄位值包含分號（如 `ABC;123`）
2. 匯入該檔案
3. **預期結果**：匯入成功，顯示 Warning 訊息

**Info 等級測試**：
1. 準備 Excel 檔案，包含額外欄位（如 `extra_col`）
2. 匯入該檔案
3. **預期結果**：匯入成功，Info 訊息預設收合

### 3. 匯入結果摘要 (FR-007, FR-008)

1. 匯入包含多種訊息類型的 Excel 檔案
2. **預期結果**：
   - 顯示「匯入成功，共 N 筆資料」
   - Warning 訊息直接顯示
   - Info 訊息可展開查看

## 測試資料準備

### 測試用 Excel 結構

| ogb19 | nvr_cust | nvr_cust_item_no | pono | ogd09 |
|-------|----------|------------------|------|-------|
| G25A111577 | TSMC | ABC-001 | PO;123 | 6,733 |
| G25A111578 | UMC | DEF-002 | PO456 | 100 |

**說明**：
- 第 1 列包含 Warning 觸發條件（分號、千分位）
- 第 2 列為正常資料

## 關鍵程式碼位置

| 檔案 | 說明 |
|------|------|
| `Services/IExcelImporter.cs` | `MessageSeverity`、`ImportMessage`、`ImportResult` 定義 |
| `Services/ExcelImporter.cs:34` | 欄位名稱正規表達式 |
| `Services/ExcelImporter.cs:140` | `BuildColumnMapping()` 訊息分級邏輯 |
| `Views/DataManageView.xaml.cs:155` | `ImportButton_Click()` 結果顯示 |

## 除錯提示

### 常見問題

1. **匯入失敗：找不到指定的檔案**
   - 確認檔案路徑正確
   - 確認檔案未被其他程式鎖定

2. **底線欄位仍顯示警告**
   - 確認 `FieldNamePattern()` 正規表達式已更新為 `^[A-Za-z0-9_]+$`

3. **Info 訊息無法展開**
   - 確認 `ImportResultDialog.xaml` 已正確引用 `Expander` 控件
