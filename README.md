# NanmaoLabel

南茂標籤列印 POC - 讀取交易資料，依據預定義的標籤格式，產出正確且條碼可被掃描的標籤 PDF 檔案。

## 功能特點

**核心功能**
- 從 Excel 匯入交易資料，儲存為 JSON 格式
- 支援兩種內建標籤格式（QW075551-1 出貨標籤、QW075551-2）
- 即時預覽標籤內容（格式切換即時更新）
- 生成 Code 128 一維條碼與 QR Code
- 單筆或批次輸出 PDF（100mm x 60mm 標籤尺寸）

**匯入警告分級**
- 訊息分級顯示（Error/Warning/Info）
- 支援含底線欄位名稱（如 `nvr_cust`）
- 匯入結果摘要對話框
- 分號欄位值警告提示

**Kiosk 風格介面**
- 深藍/白色系統統一設計
- 按鈕位置分區（建立區 | 危險區 | 確認區）
- 刪除按鈕二次確認保護
- 未儲存變更脈動光暈提醒
- 操作回饋動畫與 Loading 狀態
- 鍵盤 Focus 狀態指示

**ListView/DataGrid 樣式**
- 文字居中對齊
- 欄位寬度最佳化
- 統一視覺風格

**標籤預覽渲染**
- 即時預覽渲染
- 格式切換即時更新
- 空白狀態與錯誤處理

## 系統需求

- Windows 10/11
- .NET 8.0 Runtime
- 微軟正黑體字型

## 快速開始

### 編譯

```bash
cd NanmaoLabelPOC
dotnet build
```

### 執行

```bash
cd NanmaoLabelPOC
dotnet run
```

### 測試

```bash
cd NanmaoLabelPOC.Tests
dotnet test
```

### 發佈

```bash
cd NanmaoLabelPOC
dotnet publish -c Release -r win-x64 --self-contained false
```

## 專案結構

```
NanmaoLabel/
├── NanmaoLabel.slnx                   # 解決方案檔
├── NanmaoLabelPOC/                    # 主程式 (WPF)
│   ├── Views/                         # XAML 畫面
│   │   ├── MainWindow.xaml            # 主視窗
│   │   ├── DataManageView.xaml        # 資料管理分頁
│   │   ├── LabelPrintView.xaml        # 標籤列印分頁
│   │   └── ImportResultDialog.xaml    # 匯入結果對話框
│   ├── ViewModels/                    # MVVM ViewModel
│   │   ├── MainViewModel.cs
│   │   ├── DataManageViewModel.cs
│   │   └── LabelPrintViewModel.cs
│   ├── Models/                        # 資料模型
│   │   ├── DataRecord.cs              # 交易資料
│   │   ├── LabelTemplate.cs           # 標籤格式
│   │   └── LabelField.cs              # 標籤欄位
│   ├── Services/                      # 商業邏輯
│   │   ├── IExcelImporter.cs          # Excel 匯入介面
│   │   ├── ExcelImporter.cs           # Excel 讀取實作
│   │   ├── IDataStore.cs              # 資料儲存介面
│   │   ├── DataStore.cs               # JSON 讀寫實作
│   │   ├── ILabelRenderer.cs          # 標籤渲染介面
│   │   ├── LabelRenderer.cs           # 標籤渲染實作
│   │   ├── IBarcodeGenerator.cs       # 條碼生成介面
│   │   ├── BarcodeGenerator.cs        # 條碼生成實作
│   │   ├── IPdfExporter.cs            # PDF 輸出介面
│   │   ├── PdfExporter.cs             # PDF 輸出實作
│   │   ├── PerformanceMonitor.cs      # 效能監測工具
│   │   └── UnitConverter.cs           # 單位轉換工具
│   ├── Templates/                     # 內建標籤格式
│   │   └── BuiltInTemplates.cs        # QW075551-1、QW075551-2 定義
│   ├── Converters/                    # XAML 值轉換器
│   │   └── BoolToVisibilityConverter.cs
│   └── Resources/                     # 資源檔
│       ├── ButtonStyles.xaml          # 按鈕樣式
│       └── ListViewStyles.xaml        # ListView 樣式
├── NanmaoLabelPOC.Tests/              # 單元測試專案
│   └── Services/                      # 服務層測試
│       ├── BarcodeGeneratorTests.cs
│       ├── DataStoreTests.cs
│       ├── ExcelImporterTests.cs
│       ├── LabelRendererTests.cs
│       └── PerformanceMonitorTests.cs
├── specs/                             # 功能規格文件
│   ├── 001-label-print-poc/           # 標籤列印核心功能
│   ├── 002-import-warning/            # 匯入警告分級
│   ├── 003-button-style/              # 按鈕樣式規範
│   ├── 004-listview-style/            # ListView 樣式規範
│   └── 005-label-display/             # 標籤預覽渲染
└── samples/                           # 範例檔案
    └── MockData.xlsx                  # 示例資料
```

## 技術棧

| 套件 | 版本 | 用途 |
|------|------|------|
| CommunityToolkit.Mvvm | 8.2.2 | MVVM 框架 |
| ExcelDataReader | 3.7.0 | Excel 讀取 |
| ZXing.Net | 0.16.11 | 條碼生成 |
| QuestPDF | 2024.10.2 | PDF 輸出 |
| xUnit | 2.9.3 | 單元測試 |
| Microsoft.NET.Test.Sdk | 18.0.1 | 測試基礎設施 |
| coverlet.collector | 6.0.4 | 代碼覆蓋率 |

## 使用流程

1. **匯入資料**：至「資料管理」分頁，點擊「匯入」選擇 Excel 檔案
2. **選取資料**：至「標籤列印」分頁，單擊 ListView 項目預覽標籤
3. **輸出 PDF**：雙擊項目直接輸出，或點擊「輸出 PDF」按鈕

## 標籤格式

| 格式 | 說明 |
|------|------|
| QW075551-1 | 出貨標籤（客戶名稱、日期、數量、產品型號、條碼、QR Code） |
| QW075551-2 | Customer PO、CS Number、ERP Part NO.、條碼、QR Code |

## 變更紀錄

### v0.3.0 (2026-02-01)
- 版本號統一升級

### v0.2.0 (2026-01-31)
- 新增標籤預覽渲染功能（005-label-display）
- 新增 ListView/DataGrid 樣式規範（004-listview-style）
- 格式切換即時更新
- 空白狀態與錯誤處理

### v0.1.0 (2026-01-30)
- 完成標籤列印核心功能（001-label-print-poc）
- 新增 Excel 匯入警告分級（002-import-warning）
- 新增按鈕樣式規範 - 極簡 Kiosk 風格（003-button-style）
- 117 個單元測試，編譯 0 警告 0 錯誤

## 授權

私有專案，僅供內部使用。

## 版本

v0.3.0
