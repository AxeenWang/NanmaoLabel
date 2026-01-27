# NanmaoLabel Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-01-27

## Active Technologies

- C# / .NET 8 LTS + CommunityToolkit.Mvvm、ExcelDataReader、ZXing.Net、QuestPDF (001-label-print-poc)

## Project Structure [ref: raw_spec 7.2]

```text
NanmaoLabel/
├── NanmaoLabel.sln
├── NanmaoLabelPOC/                    # 主程式 (WPF)
│   ├── Views/                         # XAML 畫面
│   ├── ViewModels/                    # MVVM ViewModel
│   ├── Models/                        # 資料模型
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
│   │   └── PdfExporter.cs             # PDF 輸出實作
│   ├── Templates/                     # 內建標籤格式
│   │   └── BuiltInTemplates.cs        # QW075551-1、QW075551-2 定義
│   ├── Converters/                    # XAML 值轉換器
│   └── Resources/                     # 資源檔
├── NanmaoLabelPOC.Tests/              # 單元測試專案
│   └── Services/                      # 測試檔案
└── samples/                           # 範例檔案
```

## Commands

```bash
# Build
cd NanmaoLabelPOC && dotnet build

# Run
cd NanmaoLabelPOC && dotnet run

# Test
cd NanmaoLabelPOC.Tests && dotnet test

# Publish (Portable)
cd NanmaoLabelPOC && dotnet publish -c Release -r win-x64 --self-contained false
```

## Code Style

C# / .NET 8 LTS: Follow standard conventions

## Key Constraints [ref: raw_spec 13]

- 座標單位：mm 為唯一真相
- 條碼/QR Code：禁止裁切、禁止使用 Display Value
- 數量欄位：Raw Value (無千分位) vs Display Value (有千分位)
- 字型：微軟正黑體，PDF 嵌入
- 驗收：PDF 輸出為唯一權威來源

## Key Files

| 檔案 | 說明 |
|------|------|
| `.dao/raw_spec.md` | 需求規格書 |
| `specs/001-label-print-poc/spec.md` | 功能規格 |
| `specs/001-label-print-poc/plan.md` | 實作計畫 |

## Recent Changes

- 001-label-print-poc: 專案結構對應 raw_spec 7.2
- 2026-01-27: 新增 NanmaoLabelPOC.Tests 測試專案
- 2026-01-27: 介面更名 IExcelImporter、模板更名 BuiltInTemplates

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
