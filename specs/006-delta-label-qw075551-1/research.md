# Research: QW075551-1 出貨標籤 Delta Spec 實作

**Feature**: 006-delta-label-qw075551-1
**Date**: 2026-02-02

## 研究摘要

本次 Delta Spec 變更範圍明確，技術選型完全沿用現有架構，無需引入新技術或套件。以下為各項技術確認結果。

---

## 1. 長文字縮小字體實作

**Decision**: 使用 QuestPDF 動態設定字體大小

**Rationale**:
- QuestPDF 的 `Text()` 元件支援動態設定 `FontSize()`
- 可在 LabelRenderer 計算適合的字體大小，傳遞至 RenderCommand
- PdfExporter 根據 RenderCommand 的 ActualFontSize 渲染

**Alternatives considered**:
- 使用 QuestPDF 的 `ScaleToFit` 功能 → 不適用，因為需要控制最小字體下限
- 使用 System.Drawing 測量文字寬度 → 增加依賴，且 QuestPDF 已有文字處理能力

**Implementation approach**:
```text
1. LabelRenderer.ResolveContent() 後進行字體計算
2. 使用 QuestPDF.Helpers.Fonts 測量文字寬度
3. 逐步縮小字體直到適合或達到 6pt 下限
4. 達到下限後允許換行或截斷
```

---

## 2. 標籤外框繪製

**Decision**: 使用 QuestPDF 的 Border() 方法

**Rationale**:
- QuestPDF 原生支援 Border 繪製
- 可指定邊框粗細、顏色、樣式
- 與現有渲染架構完全相容

**Alternatives considered**:
- 手動繪製線條 → 增加複雜度，無必要
- 使用 Layer 疊加 → 不如 Border 直接

**Implementation approach**:
```csharp
// PdfExporter.CreateDocument()
page.Content().Border(0.5f, Unit.Point).BorderColor(Colors.Black)
    .Layers(layers => { ... });
```

---

## 3. 日期格式轉換

**Decision**: 在 LabelRenderer.ResolveContent() 中實作

**Rationale**:
- 日期格式為顯示層邏輯，適合在 Renderer 處理
- 僅影響 FINDPRTDC 欄位，範圍可控
- 不需修改 DataRecord 或 DataStore

**Alternatives considered**:
- 在 DataRecord.GetDisplayValue() 處理 → 會影響其他標籤格式
- 在 BuiltInTemplates 定義格式字串 → 增加欄位定義複雜度

**Implementation approach**:
```csharp
// LabelRenderer.ResolveContent()
if (field.Name == "FINDPRTDC" && !string.IsNullOrEmpty(rawValue))
{
    // 輸入: yyyy-MM-dd，輸出: yyyy/MM/dd
    rawValue = rawValue.Replace("-", "/");
}
```

---

## 4. QR Code 組合模式

**Decision**: 維持現有 CombinePattern 機制，僅更新模板定義

**Rationale**:
- 現有 LabelRenderer.ResolveCombinePattern() 已正確實作
- QR Code 編碼內容不變（底層資料相同）
- 僅需更新 BuiltInTemplates 中的座標

**Alternatives considered**: 無需考慮其他方案

---

## 5. 版面座標調整

**Decision**: 直接修改 BuiltInTemplates.CreateQW075551_1() 中的座標定義

**Rationale**:
- 座標定義集中於 BuiltInTemplates，修改範圍可控
- 依據 Delta Spec §4.5 的參考座標配置
- 驗收以 PDF 輸出視覺呈現為準，允許微調

**Implementation approach**:
- 逐欄位更新 X, Y, Width, Height 值
- 新增 RemarksLabel 欄位
- 移除 MoLabel, DeviceLabel, RemarkLabel 獨立標籤
- CustomerLabel, ProductNoLabel 改為雙行文字

---

## 6. 預覽區縮放比例

**Decision**: 自動計算縮放比例以適應新尺寸

**Rationale**:
- 標籤高度由 60mm 增至 80mm
- 預覽區需維持等比例縮放
- 現有縮放邏輯應自動適應新尺寸（基於 LabelTemplate.WidthMm/HeightMm）

**Verification needed**:
- 確認 LabelPrintView.xaml 的縮放邏輯是否基於 template 尺寸
- 若為硬編碼則需調整

---

## 依賴項確認

| 套件 | 版本 | 用途 | 變更需求 |
|------|------|------|----------|
| QuestPDF | 現有版本 | PDF 渲染 | 無需更新 |
| ZXing.Net | 現有版本 | QR Code 生成 | 無需更新 |
| CommunityToolkit.Mvvm | 現有版本 | MVVM 架構 | 無需更新 |

---

## 結論

所有技術問題已確認可在現有架構內解決，無需引入新依賴或重大架構變更。可直接進入 Phase 1 設計與實作。
