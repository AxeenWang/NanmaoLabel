# NanmaoLabel

南茂標籤列印 POC - 讀取交易資料，依據預定義的標籤格式，產出正確且條碼可被掃描的標籤 PDF 檔案。

## 功能特點

- 從 Excel 匯入交易資料，儲存為 JSON 格式
- 支援兩種內建標籤格式（QW075551-1 出貨標籤、QW075551-2）
- 即時預覽標籤內容
- 生成 Code 128 一維條碼與 QR Code
- 單筆或批次輸出 PDF（100mm x 60mm 標籤尺寸）
- Kiosk 風格介面（大按鈕、大字體、高對比）

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
├── NanmaoLabelPOC/                    # 主程式 (WPF)
│   ├── Views/                         # XAML 畫面
│   ├── ViewModels/                    # MVVM ViewModel
│   ├── Models/                        # 資料模型
│   ├── Services/                      # 商業邏輯
│   ├── Templates/                     # 內建標籤格式
│   ├── Converters/                    # XAML 值轉換器
│   └── Resources/                     # 資源檔
├── NanmaoLabelPOC.Tests/              # 單元測試專案
└── samples/                           # 範例檔案
```

## 技術棧

| 套件 | 用途 |
|------|------|
| CommunityToolkit.Mvvm | MVVM 框架 |
| ExcelDataReader | Excel 讀取 |
| ZXing.Net | 條碼生成 |
| QuestPDF | PDF 輸出 |

## 使用流程

1. **匯入資料**：至「資料管理」分頁，點擊「匯入」選擇 Excel 檔案
2. **選取資料**：至「標籤列印」分頁，單擊 ListView 項目預覽標籤
3. **輸出 PDF**：雙擊項目直接輸出，或點擊「輸出 PDF」按鈕

## 標籤格式

| 格式 | 說明 |
|------|------|
| QW075551-1 | 出貨標籤（客戶名稱、日期、數量、產品型號、條碼、QR Code） |
| QW075551-2 | Customer PO、CS Number、ERP Part NO.、條碼、QR Code |

## 授權

私有專案，僅供內部使用。

## 版本

v0.0.1
