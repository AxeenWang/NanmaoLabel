# Research: QW075551-2 物料標籤渲染

**Feature**: 007-delta-label-qw075551-2
**Date**: 2026-02-02
**Status**: Complete

## 1. 現有 QW075551-2 模板結構分析

### 1.1 現有實作 (BuiltInTemplates.cs)

**Decision**: 需完全重寫 `CreateQW075551_2()` 方法

**Rationale**:
- 現有實作採用上下分區式佈局，與 Delta Spec 的左右兩欄式佈局不符
- 現有包含 Barcode (ERPPARTNO) 和 QR Code，需移除
- 現有標題為「出貨標籤 Shipping Label」，需改為「物料標籤」
- 現有欄位前綴為英文，需改為中文

**Alternatives considered**:
- 漸進式修改：逐欄位調整 → 拒絕，因版面結構差異過大，不如完全重寫清晰

### 1.2 現有欄位數量

| 項目 | 現有實作 | Delta Spec |
|------|----------|------------|
| 欄位總數 | 14 | 12 (6 欄位 × 2 行) |
| Barcode | 1 (ERPPARTNO) | 0 |
| QR Code | 1 | 0 |
| Text | 12 | 12 |

## 2. 雙行顯示格式實作策略

### 2.1 欄位定義方式

**Decision**: 每個資料欄位定義為 2 個 LabelField（小字行 + 大字行）

**Rationale**:
- 現有 LabelField 模型支援獨立的座標、字型設定
- 無需修改 LabelRenderer 核心邏輯
- 與 QW075551-1 的雙行標籤 (CustomerLabel, ProductNoLabel) 實作方式一致

**Alternatives considered**:
- 新增 `DualLineField` 類型 → 拒絕，需修改 Model 與 Renderer，增加複雜度
- 在 Renderer 中處理雙行邏輯 → 拒絕，違反單一職責原則

### 2.2 小字行格式

```csharp
// 格式：「標籤:實際值」
DataSource = "單號:{cscustpo}"  // 需支援 placeholder
```

**Decision**: 小字行採用組合文字，標籤為常數部分，值為變數部分

**實作方式**:
- 小字行的 DataSource 設為「標籤:」+ 資料來源組合
- 由於現有 LabelRenderer 不支援 Text 類型的 CombinePattern，需擴充或改用替代方案

**替代方案評估**:
1. 擴充 LabelRenderer 支援 Text CombinePattern → 需修改核心服務
2. 小字行拆為兩個欄位（標籤 + 值）→ 座標計算複雜
3. **採用方案**: 小字行僅顯示「標籤:實際值」格式，由 PdfExporter 在渲染時組合

## 3. PdfExporter 雙行渲染支援

### 3.1 現有渲染流程

```
LabelRenderer.Render() → RenderCommand[] → PdfExporter.Export()
```

**Decision**: 在 RenderCommand 中新增屬性支援小字行組合

**新增屬性**:
```csharp
// RenderCommand.cs (若需要)
public string? LabelPrefix { get; set; }  // 小字行的標籤前綴
public string? PrefixDataSource { get; set; }  // 小字行的值來源
```

**替代方案**:
- 直接在 BuiltInTemplates 中預先組合 → 問題：常數值無法動態組合
- **最終決定**: 小字行分成兩個欄位（標籤常數 + 值），座標並列

## 4. 版面座標計算

### 4.1 標籤尺寸

| 項目 | 值 |
|------|-----|
| 寬度 | 100mm |
| 高度 | 80mm (變更自 60mm) |

### 4.2 兩欄佈局座標

| 區域 | X 起始 | 寬度 |
|------|--------|------|
| 左欄 | 5mm | 45mm |
| 右欄 | 55mm | 40mm |

### 4.3 列間距計算

標籤高度 80mm，扣除標題區域後：
- 標題區：Y = 3mm, 高度 = 8mm
- 資料區起始：Y = 14mm
- 可用高度：80 - 14 - 5 (底部邊距) = 61mm
- 三列，每列兩行（小字 + 大字）
- 每列高度：61 ÷ 3 ≈ 20mm
- 小字行高度：4mm (8pt)
- 大字行高度：6mm (11pt Bold)
- 行間距：約 5mm

### 4.4 詳細座標配置

| 區域 | 元素 | X | Y | Width | Height | FontSize |
|------|------|---|---|-------|--------|----------|
| 標題 | "物料標籤" | 5 | 3 | 90 | 6 | 14pt Bold |
| 第1列-左-小字 | "單號:" + value | 5 | 14 | 45 | 4 | 8pt |
| 第1列-左-大字 | value | 5 | 19 | 45 | 6 | 11pt Bold |
| 第1列-右-小字 | "代碼:" + value | 55 | 14 | 40 | 4 | 8pt |
| 第1列-右-大字 | value | 55 | 19 | 40 | 6 | 11pt Bold |
| 第2列-左-小字 | "ERP料號:" + value | 5 | 32 | 45 | 4 | 8pt |
| 第2列-左-大字 | value | 5 | 37 | 45 | 6 | 11pt Bold |
| 第2列-右-小字 | "規格型號:" + value | 55 | 32 | 40 | 4 | 8pt |
| 第2列-右-大字 | value | 55 | 37 | 40 | 6 | 11pt Bold |
| 第3列-左-小字 | "數量:" + value | 5 | 50 | 45 | 4 | 8pt |
| 第3列-左-大字 | value | 5 | 55 | 45 | 6 | 11pt Bold |
| 第3列-右-小字 | "D/C (LOT NO. ):" + value | 55 | 50 | 40 | 4 | 8pt |
| 第3列-右-大字 | value | 55 | 55 | 40 | 6 | 11pt Bold |

## 5. 小字行實作決策

### 5.1 最終決策

**Decision**: 小字行採用「常數標籤」+「動態值」兩個欄位組合

**實作方式**:
1. 每個小字行拆為兩個 LabelField：
   - 標籤欄位 (IsConstant = true): 如 "單號:"
   - 值欄位 (IsConstant = false): 對應 DataSource
2. 標籤欄位寬度固定，值欄位緊接在後

**座標調整**:
- 標籤欄位：X = 5, Width = 根據標籤長度估算
- 值欄位：X = 5 + 標籤寬度, Width = 剩餘空間

### 5.2 替代方案（保留評估）

若上述方案過於複雜，可採用：
- 小字行直接使用 CombinePattern 格式：`"{label}{value}"`
- 需確認 LabelRenderer 是否支援 Text 類型的 CombinePattern

## 6. 外框繪製

**Decision**: 設定 `HasBorder = true`

**Rationale**:
- 現有 LabelTemplate 已支援 HasBorder 屬性
- PdfExporter 已實作外框繪製邏輯

## 7. 長文字處理

**Decision**: 繼續使用現有 AutoShrinkFont 機制

**Rationale**:
- LabelField.AutoShrinkFont 已支援
- LabelField.MinFontSize 已設為 6pt 預設值
- LabelRenderer.CalculateFontSize() 已實作縮小邏輯
- RenderCommand.RequiresWrap 已支援換行標記

## 8. 結論與建議

### 8.1 主要修改檔案

| 檔案 | 修改內容 |
|------|----------|
| BuiltInTemplates.cs | 完全重寫 CreateQW075551_2() |
| (無其他檔案) | 現有架構已足夠支援 |

### 8.2 欄位數量估算

- 標題：1 個
- 6 個資料欄位 × 2 行 = 12 個
- **總計：13 個 LabelField**

### 8.3 風險評估

| 風險 | 機率 | 影響 | 緩解措施 |
|------|------|------|----------|
| 小字行標籤與值對齊困難 | 中 | 低 | 使用絕對座標精確定位 |
| 長文字換行影響版面 | 低 | 中 | 現有 RequiresWrap 機制已支援 |
| 中文字寬估算誤差 | 低 | 低 | PdfExporter 實際渲染時會自動調整 |
