# Feature Specification: ListView 選取後標籤預覽顯示

**Feature Branch**: `005-label-display`
**Created**: 2026-02-01
**Status**: Draft
**Type**: Bug Fix / 功能補齊
**Input**: 修復 ListView 選取項目後，標籤預覽區未顯示對應標籤內容的問題

## 問題摘要

當使用者在「標籤列印」分頁的 ListView 中點選任一項目時，左側標籤預覽區維持空白，未顯示任何標籤內容。根據規格 `raw_spec.md` 的定義，預覽區應即時顯示所選項目的標籤樣式，包含文字與條碼。

**根因**：Canvas 渲染層未實作 — `PreviewCanvas` 未訂閱 `PreviewCommands` 變更，未執行繪製邏輯。

## User Scenarios & Testing

### User Story 1 - 單擊 ListView 項目即時預覽標籤 (Priority: P1)

使用者在標籤列印分頁的 ListView 中點選任一筆資料項目，左側預覽區應立即顯示該筆資料對應的標籤內容，包含所有文字欄位、條碼（Code 128）與 QR Code。

**Why this priority**: 這是核心功能，直接影響使用者是否能在列印前確認標籤內容正確。沒有此功能，使用者無法預覽即將列印的標籤。

**Independent Test**: 匯入範例資料後，單擊 ListView 任一項目，觀察左側預覽區是否顯示該筆標籤內容。

**Acceptance Scenarios**:

1. **Given** ListView 已載入資料（至少 1 筆），**When** 使用者單擊任一項目，**Then** 左側預覽區於 100ms 內顯示該筆資料的完整標籤（含文字、條碼、QR Code）
2. **Given** ListView 已選取項目 A，**When** 使用者單擊項目 B，**Then** 預覽區立即更新為項目 B 的標籤內容
3. **Given** 標籤預覽已顯示，**When** 使用者按 Esc 或點擊空白處取消選取，**Then** 預覽區顯示空白狀態提示

---

### User Story 2 - 切換標籤格式即時更新預覽 (Priority: P2)

使用者切換標籤格式下拉選單（QW075551-1 ↔ QW075551-2）時，預覽區應以新格式重新渲染當前選取的項目。

**Why this priority**: 讓使用者能即時比較不同格式的標籤外觀，協助選擇最適合的格式。

**Independent Test**: 選取一筆 ListView 項目後，切換標籤格式下拉選單，觀察預覽區是否即時更新為新格式。

**Acceptance Scenarios**:

1. **Given** 已選取 ListView 項目且預覽區顯示標籤，**When** 使用者從 QW075551-1 切換至 QW075551-2，**Then** 預覽區立即以 QW075551-2 格式重新渲染該筆標籤
2. **Given** 已選取項目且格式為 QW075551-2，**When** 使用者切換回 QW075551-1，**Then** 預覽區立即以 QW075551-1 格式重新渲染

---

### User Story 3 - 空白狀態正確顯示提示 (Priority: P3)

當沒有選取任何項目、尚未載入資料、或發生渲染錯誤時，預覽區應顯示適當的提示訊息，而非空白或錯誤畫面。

**Why this priority**: 提升使用者體驗，避免使用者對空白畫面感到困惑。

**Independent Test**: 啟動程式且無資料時，觀察預覽區是否顯示引導提示。

**Acceptance Scenarios**:

1. **Given** 程式啟動且無 data.json 或 data.json 為空，**When** 使用者進入標籤列印分頁，**Then** 預覽區顯示「請先匯入資料或選取資料項目」
2. **Given** 已載入資料但無選取項目，**When** 使用者查看預覽區，**Then** 顯示「請先匯入資料或選取資料項目」
3. **Given** 選取項目但渲染過程發生例外，**When** 渲染失敗，**Then** 預覽區顯示「標籤渲染失敗」錯誤提示並記錄錯誤

---

### Edge Cases

- **快速連續切換**：使用者快速連續點擊多個 ListView 項目時，最終應顯示最後選取項目的標籤
- **重複點擊同一項目**：連續點擊同一項目時，可選擇不重新渲染（效能優化）
- **必要欄位缺失**：若選取項目缺少必要欄位（如品名、料號），應彈出警告提示
- **非必要欄位缺失**：若選取項目缺少非必要欄位，該欄位顯示空白，不影響其他欄位渲染
- **資料管理分頁儲存後**：若選取項目的資料在其他分頁被修改並儲存，預覽區應重新渲染

## Requirements

### Functional Requirements

- **FR-001**: 系統必須在使用者單擊 ListView 項目後 100ms 內，於預覽區顯示對應標籤
- **FR-002**: 系統必須監聽 ViewModel 的 `PreviewCommands` 屬性變更，並觸發 Canvas 渲染
- **FR-003**: 系統必須依據 `RenderCommand` 類型繪製對應元素：
  - `TextCommand` → TextBlock
  - `BarcodeCommand` → Image (Code 128)
  - `QRCodeCommand` → Image (QR Code)
- **FR-004**: 系統必須在切換標籤格式時重新渲染當前選取項目的標籤
- **FR-005**: 系統必須在無選取項目時顯示空白狀態提示文字
- **FR-006**: 系統必須在渲染失敗時顯示錯誤提示並記錄錯誤
- **FR-007**: 預覽區座標轉換須遵循規格：
  - 標籤實際尺寸：100mm × 60mm
  - 預覽 Canvas 尺寸：400px × 240px（4:1 縮放比）
  - 座標轉換：`px = mm × 4`
  - 字型縮放：`previewFontSize = specFontSize × 4 / 2.83465`
- **FR-008**: 系統必須確保預覽區版面與 PDF 輸出一致（誤差 ≤ ±0.5mm）

### Key Entities

- **RenderCommand**: 渲染指令基底，包含座標 (X, Y) 與尺寸 (Width, Height)
- **TextCommand**: 文字渲染指令，包含文字內容、字型、字號、對齊方式
- **BarcodeCommand**: 條碼渲染指令，包含條碼值、條碼類型 (Code 128)
- **QRCodeCommand**: QR Code 渲染指令，包含 QR Code 內容
- **LabelTemplate**: 標籤格式定義（QW075551-1、QW075551-2）
- **RecordViewModel**: ListView 項目的資料模型

## Success Criteria

### Measurable Outcomes

- **SC-001**: 使用者單擊 ListView 項目後，預覽區於 100ms 內顯示完整標籤內容
- **SC-002**: 標籤預覽與 PDF 輸出的版面誤差不超過 ±0.5mm
- **SC-003**: 100% 的標籤元素（文字、條碼、QR Code）正確顯示於預覽區
- **SC-004**: 切換標籤格式後，預覽區於 100ms 內以新格式重新渲染
- **SC-005**: 所有邊界情況（無資料、無選取、渲染失敗）皆顯示適當提示訊息

## Assumptions

- `PreviewCommands` 屬性已正確實作，會在 `SelectedRecord` 或 `SelectedTemplate` 變更時觸發 PropertyChanged 通知
- `ILabelRenderer.Render()` 方法已正確實作，能產生有效的 `RenderCommand` 集合
- 條碼與 QR Code 的圖片已能透過 `IBarcodeGenerator` 正確產生
- 標籤預覽區的 Canvas 控制項已存在於 XAML 中
- **PDF 輸出功能已完整實作**（經 2026-02-01 程式碼審查確認）：
  - `PdfExporter` 正確使用 `ILabelRenderer.Render()` 取得渲染指令
  - 支援單筆與批次輸出
  - Text、Barcode、QRCode 渲染皆已實作

## Out of Scope

- **PDF 輸出功能**（已完整實作，無需修改）
- 渲染快取機制（POC 階段每次選取皆重新渲染）
- 縮放預覽（預覽區固定為 400px × 240px）
- 預覽區拖曳或平移功能
- 條碼/QR Code 的掃描驗證功能

## Dependencies

- `ILabelRenderer` 服務
- `IBarcodeGenerator` 服務
- `BuiltInTemplates`（QW075551-1、QW075551-2）
- CommunityToolkit.Mvvm（PropertyChanged 通知機制）

## Clarification Log

| 日期 | 問題 | 結論 |
|------|------|------|
| 2026-02-01 | 此修正是否涵蓋 PDF 標籤輸出？ | **否**。經程式碼審查，`PdfExporter` 已完整實作（單筆/批次輸出、Text/Barcode/QRCode 渲染）。問題僅在 WPF Canvas 預覽層未訂閱 `PreviewCommands` 變更。PDF 輸出功能列為 Out of Scope。 |
