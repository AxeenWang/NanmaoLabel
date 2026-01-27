# Quickstart: 南茂標籤列印 POC

**Date**: 2026-01-27
**Phase**: 1 - Design & Contracts
**Status**: Complete

---

## 1. 開發環境需求

### 必要工具

| 工具 | 版本 | 說明 |
|------|------|------|
| Visual Studio 2022/2026 | 17.x+ | 建議使用 Community 或以上版本 |
| .NET SDK | 8.0 LTS | 目標框架 |
| Git | 2.x | 版本控制 |
| PowerShell | 7.x | 跨 WSL 編譯腳本（可選） |

### 系統需求

| 項目 | 需求 |
|------|------|
| 作業系統 | Windows 10/11 (x64) |
| 字型 | 微軟正黑體（Windows 內建） |
| 螢幕解析度 | 建議 1920×1080，最小 1024×576 |

---

## 2. 專案建置

### 2.1 Clone 專案

```bash
git clone <repository-url> NanmaoLabel
cd NanmaoLabel
git checkout 001-label-print-poc
```

### 2.2 開啟 Solution

```bash
# 使用 Visual Studio
start NanmaoLabel.sln

# 或使用 dotnet CLI
cd NanmaoLabelPOC
dotnet restore
dotnet build
```

### 2.3 NuGet 套件

專案會自動還原以下套件：

| 套件 | 版本 | 用途 |
|------|------|------|
| CommunityToolkit.Mvvm | 8.x | MVVM 框架 |
| ExcelDataReader | 3.x | Excel 讀取 |
| ExcelDataReader.DataSet | 3.x | DataSet 擴充 |
| ZXing.Net | 0.16.x | 條碼生成 |
| QuestPDF | 2024.x | PDF 輸出 |

---

## 3. 執行程式

### 3.1 Debug 模式

1. 在 Visual Studio 中按 F5
2. 程式啟動後自動載入 `data/data.json`
3. 若 `data.json` 不存在，顯示空白狀態

### 3.2 Release 模式

```bash
cd NanmaoLabelPOC
dotnet publish -c Release -r win-x64 --self-contained false
```

產出位置：`NanmaoLabelPOC/bin/Release/net8.0-windows/win-x64/publish/`

### 3.3 目錄結構（執行時）

```text
NanmaoLabelPOC/
├── NanmaoLabelPOC.exe
├── data/
│   └── data.json        # 自動建立（若不存在）
├── output/              # PDF 輸出目錄（自動建立）
└── samples/
    └── MockData.xlsx    # 測試資料
```

---

## 4. 測試執行

### 4.1 單元測試 [ref: raw_spec 7.2, 憲章 II]

```bash
cd NanmaoLabelPOC.Tests
dotnet test
```

### 4.2 測試專案結構

```text
NanmaoLabelPOC.Tests/
├── Services/
│   ├── ExcelImporterTests.cs      # Excel 匯入測試
│   ├── DataStoreTests.cs          # JSON 讀寫測試
│   ├── BarcodeGeneratorTests.cs   # 條碼生成測試
│   └── LabelRendererTests.cs      # 標籤渲染測試
└── NanmaoLabelPOC.Tests.csproj
```

### 4.3 測試情境 [ref: raw_spec 9]

POC 階段測試以 raw_spec 第 9 章定義之測試情境為準：

| 編號 | 測試項目 | 驗證方式 |
|------|----------|----------|
| TC-01 | JSON 自動載入 | 目視檢查 |
| TC-02~TC-05 | 變數對應、常數值、缺值、格式切換 | 目視檢查 |
| TC-06~TC-07 | 單擊預覽、雙擊輸出 | 目視檢查 + PDF 檔案 |
| TC-08~TC-09 | 條碼掃描 | **手機/掃描槍掃描成功** |
| TC-10~TC-12 | PDF 輸出、條碼掃描、批次輸出 | 開啟 PDF 檢查 |
| TC-13~TC-18 | 資料管理功能 | 目視檢查 + JSON 檔案 |

---

## 5. 技術參考

### 5.1 座標系統 [ref: IC-001~003]

**唯一真相**: 所有座標以 mm 定義

**換算公式**:

| 轉換 | 公式 | 範例 |
|------|------|------|
| mm → pt (PDF) | `pt = mm × 72 ÷ 25.4` | 100mm = 283.46pt |
| mm → px @ 300 DPI | `px = mm × 300 ÷ 25.4` | 100mm = 1181.1px |
| mm → WPF 邏輯單位 | `wpf = mm × 96 ÷ 25.4` | 100mm = 377.95 |

**使用方式**:

```csharp
// 座標換算（建議於 Services 層實作 UnitConverter 靜態類別）
// 位置：NanmaoLabelPOC/Services/UnitConverter.cs（可依實作需要新增）

var xPt = UnitConverter.MmToPt(5.0);     // 5mm → 14.17pt
var yPx = UnitConverter.MmToPx300(10.0); // 10mm → 118.1px
```

### 5.2 字型嵌入 [ref: IC-021~022]

**指定字型**: 微軟正黑體 (Microsoft JhengHei)

**QuestPDF 設定**:

```csharp
// App.xaml.cs 啟動時
QuestPDF.Settings.License = LicenseType.Community;

// 文字樣式
var normalStyle = TextStyle.Default
    .FontFamily("Microsoft JhengHei")
    .FontSize(10);

var boldStyle = TextStyle.Default
    .FontFamily("Microsoft JhengHei")
    .Bold()
    .FontSize(11);
```

**嵌入行為**: QuestPDF 自動將使用的字元子集嵌入 PDF

### 5.3 條碼生成 [ref: IC-007~010]

**Code 128 設定**:

```csharp
var barcodeWriter = new BarcodeWriterPixelData
{
    Format = BarcodeFormat.CODE_128,
    Options = new EncodingOptions
    {
        Height = 118,      // 10mm @ 300 DPI
        Margin = 10,       // Quiet Zone (像素)
        PureBarcode = true // 不含文字
    }
};
```

**QR Code 設定**:

```csharp
var qrCodeWriter = new BarcodeWriterPixelData
{
    Format = BarcodeFormat.QR_CODE,
    Options = new QrCodeEncodingOptions
    {
        Width = 236,       // 20mm @ 300 DPI
        Height = 236,
        Margin = 4,        // Quiet Zone (單元數)
        ErrorCorrection = ErrorCorrectionLevel.M,
        CharacterSet = "UTF-8"
    }
};
```

### 5.4 Raw Value vs Display Value [ref: IC-005~006]

**處理原則**:

| 用途 | 使用值 | 範例 |
|------|--------|------|
| Barcode 編碼 | Raw Value | "6733" |
| QR Code 編碼 | Raw Value | "6733" |
| UI 顯示 | Display Value | "6,733" |
| 標籤文字 | Display Value | "6,733" |

**取值方法**:

```csharp
// 建議於 Models/DataRecord.cs 或 Services 層實作 Raw/Display Value 處理
// 位置依據 raw_spec 7.2 決定

public class DataRecord
{
    // ... 欄位定義 ...

    // 取得 Raw Value（用於 Barcode/QRCode 編碼）
    public string GetRawValue(string field)
    {
        return field switch
        {
            "ogd09" => Ogd09,  // 原始數值，不含千分位
            _ => GetPropertyValue(field)?.Trim() ?? ""
        };
    }

    // 取得 Display Value（用於畫面顯示、標籤文字）
    public string GetDisplayValue(string field)
    {
        return field switch
        {
            "ogd09" => FormatQuantity(Ogd09),  // 加千分位
            "obe25" => Obe25,  // 已是 yyyy-MM-dd
            _ => GetPropertyValue(field)?.Trim() ?? ""
        };
    }

    private static string FormatQuantity(string rawValue)
    {
        if (long.TryParse(rawValue, out var number))
            return number.ToString("N0");  // 千分位格式
        return rawValue;
    }
}
```

---

## 6. 常見問題

### Q1: PDF 輸出後條碼掃描失敗

**檢查項目**:
1. 確認使用 Raw Value 編碼（不含千分位、空白）
2. 檢查 Quiet Zone 是否被裁切
3. 以 300 DPI 列印後測試（螢幕顯示非最終驗收依據 [ref: IC-025]）

### Q2: 字型在 PDF 中顯示異常

**檢查項目**:
1. 確認目標環境已安裝微軟正黑體
2. QuestPDF 會自動嵌入字型子集
3. 檢查 `QuestPDF.Settings.License` 是否正確設定

### Q3: 預覽與 PDF 版面不一致

**檢查項目**:
1. 確認使用 `UnitConverter` 統一換算
2. 允許極小差異（抗鋸齒、縮放）[ref: IC-004]
3. 驗收以 PDF 輸出為準 [ref: IC-025]

### Q4: Excel 匯入後欄位缺失

**檢查項目**:
1. 欄位名稱僅允許英數字 (A-Z, a-z, 0-9) [ref: IC-016]
2. 欄位名稱比對不分大小寫
3. 含底線、空白、特殊符號者視為欄位缺失

---

## 7. 驗收檢查清單

開發完成後，執行以下驗收檢查：

- [ ] **SC-001**: 所有條碼與 QR Code 首次掃描即成功
- [ ] **SC-002**: 掃描結果與 data.json 資料完全一致
- [ ] **SC-003**: 預覽與 PDF 版面誤差 ≤ ±0.5mm
- [ ] **SC-006**: 兩種標籤格式皆能正確顯示與輸出
- [ ] **SC-007**: data.json 為唯一資料來源
- [ ] **SC-010**: 錯誤訊息以繁體中文顯示

---

## 8. 相關文件

| 文件 | 說明 |
|------|------|
| [spec.md](spec.md) | 功能規格書 |
| [plan.md](plan.md) | 實作計畫 |
| [research.md](research.md) | 技術研究 |
| [data-model.md](data-model.md) | 資料模型 |
| [contracts/data-schema.json](contracts/data-schema.json) | JSON Schema |
| [checklists/requirements.md](checklists/requirements.md) | 需求檢查清單 |
