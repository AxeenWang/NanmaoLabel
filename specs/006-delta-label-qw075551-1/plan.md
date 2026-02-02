# Implementation Plan: QW075551-1 出貨標籤 Delta Spec 實作

**Branch**: `006-delta-label-qw075551-1` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-delta-label-qw075551-1/spec.md`

## Summary

根據 Delta Spec 更新 QW075551-1 標籤的渲染邏輯，主要變更包含：標籤尺寸由 60mm 增至 80mm、CSCUSTPN 由 Barcode 改為 Text、日期格式改為 yyyy/MM/dd、QR Code 移至左下角並與 Remarks 文字並排、移除 Code 128 條碼、新增長文字縮小字體處理（最小 6pt）。

## Technical Context

**Language/Version**: C# / .NET 8 LTS
**Primary Dependencies**: CommunityToolkit.Mvvm、QuestPDF、ZXing.Net
**Storage**: JSON 檔案 (DataStore)
**Testing**: xUnit (NanmaoLabelPOC.Tests)
**Target Platform**: Windows (WPF)
**Project Type**: Single project (WPF Desktop)
**Performance Goals**: 標註操作回應時間 <100ms [ref: constitution IV]
**Constraints**: 應用程式啟動 <3 秒、記憶體穩定無洩漏 [ref: constitution IV]
**Scale/Scope**: POC 階段，兩種標籤格式 (QW075551-1, QW075551-2)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 狀態 | 說明 |
|------|------|------|
| I. 程式碼品質 | ✅ Pass | 遵循既有命名慣例、單一職責、無魔術數字 |
| II. 測試標準 | ✅ Pass | 核心業務邏輯 (LabelRenderer) 有對應測試 |
| III. 使用者體驗一致性 | ✅ Pass | 維持既有 UI 風格，使用共用資源 |
| IV. 效能要求 | ✅ Pass | 渲染操作在毫秒級完成 |
| A-1. 文件依據原則 | ✅ Pass | 所有變更依據 raw_delta_label_QW075551-1.md |
| A-2. 禁止預設設計原則 | ✅ Pass | 僅實作 Delta Spec 定義的變更，無預留擴充 |
| A-3. 可執行可驗證原則 | ✅ Pass | 每項需求有明確驗收條件（見 spec.md SC-001~SC-011） |
| A-4. 需求追溯原則 | ✅ Pass | 任務將標註對應的 FR 編號 |
| A-5. 格式遵循原則 | ✅ Pass | 依循 plan-template.md 格式 |

## Project Structure

### Documentation (this feature)

```text
specs/006-delta-label-qw075551-1/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── checklists/          # Quality checklists
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── Models/
│   ├── LabelField.cs        # [修改] 新增 MinFontSize 屬性
│   └── LabelTemplate.cs     # [無變更]
├── Services/
│   ├── ILabelRenderer.cs    # [修改] 新增日期格式轉換、長文字處理
│   ├── LabelRenderer.cs     # [修改] 實作日期格式轉換、長文字縮小邏輯
│   └── PdfExporter.cs       # [修改] 新增標籤外框渲染、長文字縮小渲染
├── Templates/
│   └── BuiltInTemplates.cs  # [修改] 更新 QW075551-1 模板定義
└── Views/
    └── LabelPrintView.xaml  # [修改] 預覽區縮放比例調整

NanmaoLabelPOC.Tests/
└── Services/
    └── LabelRendererTests.cs  # [修改] 新增測試案例
```

**Structure Decision**: 維持現有單一專案結構，僅修改既有檔案。無需新增專案或重大架構變更。

## Complexity Tracking

> 無 Constitution Check 違反需要辯護

---

## Phase 0: Research

### 技術研究項目

本次 Delta Spec 變更範圍明確，技術選型沿用現有架構，無需額外研究。以下為確認事項：

| 項目 | 結論 | 依據 |
|------|------|------|
| 長文字縮小字體實作 | QuestPDF 支援動態字體大小設定，可在渲染時計算適合的字體大小 | QuestPDF API 文件 |
| 標籤外框繪製 | QuestPDF 支援 Border() 方法繪製邊框 | 既有 PdfExporter 架構 |
| 日期格式轉換 | 在 LabelRenderer 的 ResolveContent 方法中處理 | Delta Spec §6.1 |
| 版面座標調整 | 直接修改 BuiltInTemplates.cs 中的座標定義 | Delta Spec §4.5 |

---

## Phase 1: Design

### 1. 資料模型變更

#### LabelField 擴充

新增屬性以支援長文字處理：

```csharp
// NanmaoLabelPOC/Models/LabelField.cs
public class LabelField
{
    // ... 現有屬性 ...

    /// <summary>
    /// 最小字體大小 (pt) - 長文字縮小時的下限
    /// [ref: FR-008, Clarification 2026-02-02]
    /// </summary>
    public double MinFontSize { get; set; } = 6;

    /// <summary>
    /// 是否啟用自動縮小字體
    /// [ref: FR-008]
    /// </summary>
    public bool AutoShrinkFont { get; set; } = false;
}
```

#### LabelTemplate (QW075551-1) 變更

```text
變更項目:
- HeightMm: 60 → 80 [FR-001]
- CSCUSTPN.FieldType: Barcode → Text [FR-005]
- QRCODE.X: 75 → 5 (移至左下角) [FR-012]
- 新增 RemarksLabel 欄位 [FR-013]
- 移除 MoLabel, DeviceLabel, RemarkLabel 獨立標籤 [FR-013]
- CSMO, OUTDEVICENO, CSREMARK 座標調整至 QR Code 右側 [FR-014]
- 欄位前綴標籤改為雙行顯示 [FR-019]
- 所有 Text 欄位啟用 AutoShrinkFont [FR-008]
```

### 2. 渲染邏輯變更

#### LabelRenderer 擴充

```text
新增功能:
1. 日期格式轉換 [FR-006]
   - 輸入: yyyy-MM-dd
   - 輸出: yyyy/MM/dd
   - 適用欄位: FINDPRTDC

2. 長文字縮小邏輯 [FR-008]
   - 初始字體: field.FontSize
   - 最小字體: field.MinFontSize (6pt)
   - 縮小策略: 逐步減少 0.5pt 直到文字適合或達到下限
   - 超出處理: 允許換行或截斷加省略號
```

#### PdfExporter 擴充

```text
新增功能:
1. 標籤外框渲染 [FR-003]
   - 單線矩形邊框
   - 線條粗細: 0.5pt
   - 無分隔線

2. 長文字縮小渲染 [FR-008]
   - 根據 RenderCommand 中的計算字體大小渲染
   - 達到最小字體後嘗試換行
   - 換行後仍無法容納則截斷加省略號
```

### 3. QW075551-1 新版面座標定義

根據 Delta Spec §4.5，以下為新版座標配置：

| 元素 | X (mm) | Y (mm) | Width (mm) | Height (mm) | 說明 |
|------|--------|--------|------------|-------------|------|
| Title | 5 | 2 | 90 | 6 | 標題（不變） |
| CustomerLabel | 5 | 10 | 20 | 8 | 雙行：Customer / 客戶名稱 |
| CSCUSTOMER | 28 | 10 | 67 | 5 | 客戶名稱值 |
| DateLabel | 5 | 19 | 10 | 4 | Date |
| FINDPRTDC | 16 | 19 | 25 | 5 | 日期 (yyyy/MM/dd) |
| QtyLabel | 55 | 19 | 10 | 4 | Q'ty |
| CSQTY | 66 | 19 | 29 | 5 | 數量 |
| ProductNoLabel | 5 | 26 | 20 | 8 | 雙行：Product NO. / 產品型號 |
| CSCUSTITEMNO | 5 | 35 | 90 | 5 | 客戶料號 |
| CSCUSTPN | 5 | 41 | 90 | 5 | 客戶 P/N (Text) |
| RemarksLabel | 5 | 50 | 20 | 4 | Remarks |
| QRCODE | 5 | 55 | 20 | 20 | QR Code 左下角 |
| CSMO | 28 | 55 | 67 | 4 | 製令單號（QR 右側） |
| OUTDEVICENO | 28 | 60 | 67 | 4 | 裝置編號（QR 右側） |
| CSREMARK | 28 | 65 | 67 | 4 | 備註（QR 右側） |
| Border | 0 | 0 | 100 | 80 | 外框（新增） |

### 4. 介面契約

本次變更不涉及新增介面，僅擴充現有 ILabelRenderer：

```csharp
// RenderCommand 擴充
public class RenderCommand
{
    // ... 現有屬性 ...

    /// <summary>
    /// 計算後的實際字體大小（經過縮小處理）
    /// </summary>
    public double ActualFontSize { get; init; }

    /// <summary>
    /// 是否需要換行
    /// </summary>
    public bool RequiresWrap { get; init; }
}
```

### 5. 測試案例

新增測試案例清單：

| 測試 | 對應需求 | 說明 |
|------|----------|------|
| Render_QW075551_1_DateFormat_ReturnsSlashFormat | FR-006 | 日期格式 yyyy/MM/dd |
| Render_QW075551_1_CSCUSTPN_ReturnsText | FR-005, FR-007 | CSCUSTPN 為 Text 非 Barcode |
| Render_QW075551_1_QRCode_Position | FR-012 | QR Code 位於 X=5 |
| Render_QW075551_1_QRCode_Content | FR-009, FR-010 | QR Code 內容格式正確 |
| Render_LongText_ShrinkFont_MinSize6pt | FR-008 | 長文字縮小至 6pt |
| Render_LongText_ExceedsMinFont_Truncate | FR-008 | 超長文字截斷加省略號 |
| Export_QW075551_1_PageSize_100x80mm | FR-001, FR-017 | PDF 頁面尺寸 |
| Export_QW075551_1_HasBorder_NoSeparator | FR-003 | 有外框無分隔線 |

---

## 需求追溯矩陣

| 需求 | 影響檔案 | 測試案例 |
|------|----------|----------|
| FR-001 | BuiltInTemplates.cs | Export_QW075551_1_PageSize_100x80mm |
| FR-003 | PdfExporter.cs | Export_QW075551_1_HasBorder_NoSeparator |
| FR-005 | BuiltInTemplates.cs | Render_QW075551_1_CSCUSTPN_ReturnsText |
| FR-006 | LabelRenderer.cs | Render_QW075551_1_DateFormat_ReturnsSlashFormat |
| FR-007 | BuiltInTemplates.cs | Render_QW075551_1_CSCUSTPN_ReturnsText |
| FR-008 | LabelField.cs, LabelRenderer.cs, PdfExporter.cs | Render_LongText_* |
| FR-009 | BuiltInTemplates.cs | Render_QW075551_1_QRCode_Content |
| FR-010 | LabelRenderer.cs | Render_QW075551_1_QRCode_Content |
| FR-012 | BuiltInTemplates.cs | Render_QW075551_1_QRCode_Position |
| FR-013, FR-014 | BuiltInTemplates.cs | （版面視覺驗收） |
| FR-017 | PdfExporter.cs | Export_QW075551_1_PageSize_100x80mm |
| FR-019 | BuiltInTemplates.cs | （版面視覺驗收） |

---

## 下一步

執行 `/speckit.tasks` 產生任務清單。
