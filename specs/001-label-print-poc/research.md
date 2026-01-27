# Research: 南茂標籤列印 POC

**Date**: 2026-01-27
**Phase**: 0 - Research & Technical Decisions
**Status**: Complete

---

## 1. QuestPDF 字型嵌入

**研究目的**: 確認微軟正黑體嵌入 PDF 的方式與授權相容性 [ref: raw_spec 5.3, IC-022]

### Decision

使用 QuestPDF 的 `TextStyle.FontFamily()` 指定系統字型「Microsoft JhengHei」（微軟正黑體）。QuestPDF 預設會將使用的字型子集（Subset）嵌入 PDF，確保在任何環境開啟皆能正確顯示。

### Rationale

1. **QuestPDF 字型處理機制**：QuestPDF 使用 SkiaSharp 底層渲染，會自動從系統字型目錄載入指定字型並嵌入 PDF
2. **微軟正黑體授權**：該字型隨 Windows 10/11 授權，可用於 PDF 嵌入（非獨立散布字型檔案）
3. **字型子集嵌入**：QuestPDF 僅嵌入實際使用的字元，減少 PDF 檔案大小

### Implementation Notes

```csharp
// 字型註冊（App.xaml.cs 啟動時）
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

### Alternatives Considered

| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 系統字型直接引用 | 簡單、零部署成本 | 需確認目標環境已安裝字型 | ✅ 採用（Windows 環境） |
| 嵌入字型檔案至專案 | 不依賴系統環境 | 授權風險、檔案變大 | ❌ 不採用 |

---

## 2. ZXing.Net Code 128 參數配置

**研究目的**: 確認 Quiet Zone 保留方式與條碼下方文字配置 [ref: raw_spec 6.1, IC-008]

### Decision

使用 ZXing.Net `BarcodeWriter` 產生 Code 128 條碼圖片，設定 `Margin = 10`（像素，對應 10 倍最小單元寬度）。條碼下方文字由 QuestPDF 渲染引擎獨立繪製（非 ZXing.Net 產生）。

### Rationale

1. **Quiet Zone 規範**：raw_spec 6.1 要求左右各 10 倍最小單元寬度，ZXing.Net 的 `Margin` 參數以像素計算，可換算達成
2. **條碼下方文字**：ZXing.Net 的 `PureBarcode = true` 產生不含文字的純條碼圖片，文字由標籤渲染引擎控制位置與字型（8pt、置中）
3. **DPI 考量**：產生 300 DPI 解析度圖片以確保列印品質

### Implementation Notes

```csharp
var barcodeWriter = new BarcodeWriterPixelData
{
    Format = BarcodeFormat.CODE_128,
    Options = new EncodingOptions
    {
        Width = 300,     // 像素，依條碼內容長度動態調整
        Height = 118,    // 10mm @ 300DPI ≈ 118 像素
        Margin = 10,     // Quiet Zone
        PureBarcode = true
    }
};

var pixelData = barcodeWriter.Write(rawValue);
// 轉換為 System.Drawing.Bitmap 或 SkiaSharp.SKBitmap
```

### 條碼尺寸換算

| 項目 | mm | @ 300 DPI (px) |
|------|------|----------------|
| 條碼高度 | 10 | 118 |
| Quiet Zone (左右各) | ~3 | 10 (最小單元寬度 × 10) |

### Alternatives Considered

| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| ZXing.Net 內建文字 | 一體產生 | 字型無法控制、不支援中文 | ❌ 不採用 |
| 渲染引擎獨立繪製文字 | 字型/位置可控 | 需額外繪製邏輯 | ✅ 採用 |

---

## 3. ZXing.Net QR Code 參數配置

**研究目的**: 確認 Level M 容錯、UTF-8 編碼、Quiet Zone 設定 [ref: raw_spec 6.2, IC-009~010]

### Decision

使用 ZXing.Net `BarcodeWriter` 產生 QR Code，設定：
- `ErrorCorrection = ErrorCorrectionLevel.M`（15% 容錯）
- `CharacterSet = "UTF-8"`（支援中文）
- `Margin = 4`（四周各 4 倍單元寬度）

### Rationale

1. **容錯等級**：Level M (15%) 符合 raw_spec 6.2 規範
2. **UTF-8 編碼**：ZXing.Net 預設支援，需明確指定確保中文正確編碼
3. **Quiet Zone**：raw_spec 6.2 要求四周各 4 倍單元寬度

### Implementation Notes

```csharp
var qrCodeWriter = new BarcodeWriterPixelData
{
    Format = BarcodeFormat.QR_CODE,
    Options = new QrCodeEncodingOptions
    {
        Width = 236,      // 20mm @ 300DPI ≈ 236 像素
        Height = 236,
        Margin = 4,       // Quiet Zone (單元數)
        ErrorCorrection = ErrorCorrectionLevel.M,
        CharacterSet = "UTF-8"
    }
};

// 組合欄位值（使用 Raw Value，保留空值位置）
var qrContent = $"{pono};{ima902};{ogd09}";  // 如 "A;;C"
var pixelData = qrCodeWriter.Write(qrContent);
```

### QR Code 尺寸換算

| 項目 | mm | @ 300 DPI (px) |
|------|------|----------------|
| QR Code 尺寸 | 20 × 20 | 236 × 236 |

### Alternatives Considered

| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| Level L (7%) | 模組更大、更易掃描 | 容錯不足 | ❌ 不符合規範 |
| Level M (15%) | 平衡容錯與密度 | - | ✅ 採用 |
| Level H (30%) | 高容錯 | 模組過密、可能影響掃描 | ❌ 過度設計 |

---

## 4. WPF 雙擊事件處理（避免雙擊地獄）

**研究目的**: 確保單擊僅預覽、雙擊僅輸出一次，避免 WPF 雙擊地獄 [ref: raw_spec 8.8, IC-013]

### Decision

使用 `MouseDoubleClick` 事件搭配 `e.Handled = true` 阻止事件冒泡，並實作防抖（Debounce）機制：輸出操作後 500ms 內忽略重複觸發。

### Rationale

1. **WPF 雙擊行為**：WPF 雙擊事件觸發順序為 MouseDown → MouseUp → MouseDown → MouseDoubleClick → MouseUp，單擊 PreviewMouseLeftButtonUp 會在雙擊前觸發
2. **允許行為**：raw_spec 8.8 允許「雙擊前先觸發單擊（先預覽再輸出）」
3. **禁止行為**：雙擊被判成兩次單擊導致不輸出或輸出兩次

### Implementation Notes

實作位置依據 raw_spec 7.2：
- ViewModel: `NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs`
- View: `NanmaoLabelPOC/Views/LabelPrintView.xaml`

**關鍵實作要點**:
1. 使用 `MouseDoubleClick` 事件搭配 `e.Handled = true`
2. 實作 500ms 防抖機制
3. 允許「雙擊前先觸發單擊」行為（先預覽再輸出）
4. 禁止雙擊被判成兩次單擊導致不輸出或輸出兩次

### Alternatives Considered

| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 單純 MouseDoubleClick | 簡單 | 仍可能雙擊被誤判 | ❌ 不夠穩定 |
| Debounce + e.Handled | 穩定、可控 | 稍複雜 | ✅ 採用 |
| Click 延遲判斷 | 可區分單雙擊 | 單擊有延遲（體驗差） | ❌ 不採用 |

---

## 5. mm → pt 渲染一致性

**研究目的**: 預覽與 PDF 共用邏輯，確保誤差 ≤ ±0.5mm [ref: IC-001~004]

### Decision

1. **唯一真相**：所有座標以 mm 定義於 `LabelField` 模型
2. **統一換算**：建立 `UnitConverter` 靜態類別處理 mm ↔ pt ↔ px 換算
3. **共用渲染邏輯**：`LabelRenderer` 產生與座標系統無關的渲染指令，預覽（WPF Canvas）與 PDF（QuestPDF）各自轉換座標

### Rationale

1. **IC-001**: mm 為唯一真相
2. **IC-002**: 預覽與 PDF 共用同一套渲染定位邏輯
3. **IC-003**: 換算公式固定，避免各處各自換算造成誤差累積

### Implementation Notes

實作位置：建議於 `NanmaoLabelPOC/Services/` 新增 `UnitConverter.cs`（raw_spec 7.2 未明確定義，屬實作細節）

**換算公式**（固定，不得各處各自實作）:

| 轉換 | 公式 | 範例 |
|------|------|------|
| mm → pt (PDF) | `mm × 72 ÷ 25.4` | 100mm = 283.46pt |
| mm → px @ 300 DPI | `mm × 300 ÷ 25.4` | 100mm = 1181.1px |
| mm → WPF 邏輯單位 | `mm × 96 ÷ 25.4` | 100mm = 377.95 |

**標籤尺寸換算** [ref: raw_spec 5.1]:
- 寬度：100mm = 283.46pt
- 高度：60mm = 170.08pt

### 渲染架構 [ref: raw_spec 7.2]

```text
LabelTemplate (mm) [定義於 Templates/BuiltInTemplates.cs]
       │
       ▼
LabelRenderer.Render() [Services/LabelRenderer.cs]
       │
       ├──────────────────────────────────────┐
       ▼                                      ▼
WPF Preview                              QuestPDF Export
(MmToWpf 換算)                           (MmToPt 換算)
[Views/LabelPrintView.xaml]              [Services/PdfExporter.cs]
```

### Alternatives Considered

| 方案 | 優點 | 缺點 | 結論 |
|------|------|------|------|
| 各處自行換算 | 彈性 | 換算邏輯分散、易出錯 | ❌ 不採用 |
| 統一 UnitConverter | 單一來源、可測試 | - | ✅ 採用 |
| pt 為內部單位 | 減少換算次數 | 違反 IC-001（mm 為唯一真相） | ❌ 不採用 |

---

## Summary

| 主題 | 決策 | 對應規範 |
|------|------|----------|
| QuestPDF 字型嵌入 | 使用系統字型「Microsoft JhengHei」，QuestPDF 自動嵌入子集 | IC-021~022 |
| Code 128 條碼 | ZXing.Net + PureBarcode，文字由渲染引擎繪製，Margin=10 | IC-007~008 |
| QR Code | ZXing.Net + Level M + UTF-8，Margin=4 | IC-009~010 |
| 雙擊事件處理 | MouseDoubleClick + e.Handled + 500ms Debounce | IC-012~013 |
| 渲染一致性 | UnitConverter 統一換算，LabelRenderer 共用邏輯 | IC-001~004 |

**Phase 0 Status**: ✅ 所有研究主題已解決，無 NEEDS CLARIFICATION
