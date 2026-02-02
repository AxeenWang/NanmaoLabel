# Feature Specification: QW075551-2 物料標籤渲染

**Feature Branch**: `007-delta-label-qw075551-2`
**Created**: 2026-02-02
**Status**: Draft
**Input**: User description: "讀取 raw_delta_label_QW075551-2.md 並將其結構化為標準規格"

## Clarifications

### Session 2026-02-02

- Q: 標籤尺寸應為多少？ → A: 100mm × 80mm（變更自原 Delta Spec 的 60mm 高度）
- Q: 字體縮小的最小下限為何？ → A: 最小 6pt，若仍溢出則允許換行

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 匯出 QW075551-2 物料標籤 PDF (Priority: P1)

使用者從 Excel 匯入物料資料後，選擇 QW075551-2 標籤格式，系統產生符合規格的物料標籤 PDF 檔案。

**Why this priority**: 這是本功能的核心價值，物料標籤的主要用途就是產生可列印的 PDF。

**Independent Test**: 可透過匯入測試 Excel 資料，選擇 QW075551-2 模板，輸出 PDF 並以目視檢查版面是否符合規格。

**Acceptance Scenarios**:

1. **Given** 已匯入含有 cscustpo、erpmat、nvr_cust_item_no、ogd09、nvr_remark10 欄位的 Excel 資料，**When** 使用者選擇 QW075551-2 模板並執行 PDF 匯出，**Then** 系統產生 100mm × 80mm 的標籤 PDF，包含所有六個欄位的雙行顯示格式。

2. **Given** 已選擇 QW075551-2 模板，**When** PDF 產生完成，**Then** 標籤標題顯示「物料標籤」（置中，14pt Bold），版面採用左右兩欄式佈局，無任何條碼或 QR Code。

3. **Given** 資料中 ogd09 欄位值為 6733，**When** 渲染 CSQTY 欄位，**Then** 顯示值加入千分位格式（6,733）。

---

### User Story 2 - 欄位雙行顯示格式 (Priority: P1)

每個欄位以雙行方式顯示：小字行顯示「標籤:實際值」，大字行僅顯示實際值（Bold 強調）。

**Why this priority**: 這是 QW075551-2 標籤的核心視覺設計要求，直接影響標籤的可讀性與規格符合度。

**Independent Test**: 以任意測試資料產生 PDF，檢查每個欄位是否呈現雙行格式。

**Acceptance Scenarios**:

1. **Given** 欄位 CSCUSTPO 值為 "P600-X172509050001"，**When** 渲染該欄位，**Then** 小字行顯示「單號:P600-X172509050001」（8pt），大字行顯示「P600-X172509050001」（11pt Bold）。

2. **Given** 六個欄位均有值，**When** 渲染標籤，**Then** 左欄顯示單號、ERP料號、數量；右欄顯示代碼、規格型號、D/C (LOT NO.)。

---

### User Story 3 - 長文字縮小字體處理 (Priority: P2)

當欄位值超過配置區域寬度時，系統自動縮小字體以完整顯示內容，不截斷文字。

**Why this priority**: 確保資料完整性，避免因版面限制導致關鍵資訊遺失。

**Independent Test**: 以超長測試字串產生 PDF，驗證文字完整顯示且未被截斷。

**Acceptance Scenarios**:

1. **Given** 欄位值長度超過配置寬度，**When** 渲染該欄位，**Then** 系統自動縮小字體使文字完整顯示於配置區域內。

2. **Given** 縮小字體後的文字，**When** 檢查 PDF 輸出，**Then** 所有字元可識別，無截斷或溢出現象。

---

### User Story 4 - 空值欄位處理 (Priority: P2)

當欄位值為空時，保留版面位置，顯示空字串，不跳過或收合版面。

**Why this priority**: 維持標籤版面一致性，避免因空值導致版面錯亂。

**Independent Test**: 以含空值的測試資料產生 PDF，驗證版面配置不變。

**Acceptance Scenarios**:

1. **Given** CSREMARK 欄位值為空，**When** 渲染標籤，**Then** D/C (LOT NO.) 區域保留原位，小字行顯示「D/C (LOT NO. ):」，大字行顯示空白。

2. **Given** 多個欄位為空，**When** 渲染標籤，**Then** 所有欄位位置維持不變，版面結構與有值時一致。

---

### Edge Cases

- 所有欄位同時為空值時，標籤仍應正常渲染，顯示標題與所有欄位標籤。
- 數量欄位 (CSQTY) 為 0 時，應顯示「0」而非空白。
- 極長字串（如 50+ 字元）導致字體縮小至 6pt 仍溢出時，允許換行顯示。
- 資料包含特殊字元（如引號、斜線）時，應正確顯示。

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: 系統 MUST 支援 QW075551-2 物料標籤模板，標籤尺寸為 100mm × 80mm。
- **FR-002**: 系統 MUST 渲染標籤標題「物料標籤」，置中顯示，字型 14pt Bold。
- **FR-003**: 系統 MUST 採用左右兩欄式佈局，左欄起始 X=5mm，右欄起始 X=55mm。
- **FR-004**: 系統 MUST 對每個欄位渲染雙行顯示：小字行（8pt，標籤:值格式）與大字行（11pt Bold，純值）。
- **FR-005**: 系統 MUST 使用以下欄位前綴標籤：單號:、代碼:、ERP料號:、規格型號:、數量:、D/C (LOT NO. ):
- **FR-006**: 系統 MUST 將 CSNUMBER 欄位固定顯示為 "17008"。
- **FR-007**: 系統 MUST 將 CSQTY 欄位值格式化為千分位顯示（如 6733 → 6,733）。
- **FR-008**: 系統 MUST 在長文字溢出時縮小字體（最小 6pt），若縮至 6pt 仍溢出則允許換行，禁止截斷文字。
- **FR-009**: 系統 MUST 在欄位空值時保留版面位置，顯示空字串。
- **FR-010**: 系統 MUST 繪製標籤外框（單線矩形邊框），不繪製分隔線。
- **FR-011**: 系統 MUST NOT 渲染任何 Barcode 或 QR Code（所有欄位均為純文字）。
- **FR-012**: 系統 MUST 對所有 Text 欄位值進行 Trim 處理（移除前後空白）。

### Key Entities

- **LabelData**: 標籤資料模型，包含 cscustpo、erpmat、nvr_cust_item_no、ogd09、nvr_remark10 欄位。
- **QW075551-2 Template**: 物料標籤模板定義，包含版面配置、欄位座標、字型規格等。

### CodeSoft 欄位對照表

| Item | CodeSoft 欄位    | 資料來源欄位      | 顯示前綴標籤         | 顯示類型 |
|------|------------------|-------------------|----------------------|----------|
| 1    | CSCUSTPO         | cscustpo          | 單號:                | Text     |
| 2    | CSNUMBER         | "17008"（固定值） | 代碼:                | Text     |
| 3    | ERPPARTNO        | erpmat            | ERP料號:             | Text     |
| 4    | CSCUSTITEMNO     | nvr_cust_item_no  | 規格型號:            | Text     |
| 5    | CSQTY            | ogd09             | 數量:                | Text     |
| 6    | CSREMARK         | nvr_remark10      | D/C (LOT NO. ):      | Text     |

### 版面配置參考

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              物料標籤                                            │
│                                                                                 │
│ 單號:P600-X172509050001                    代碼:17008                            │
│ P600-X172509050001                         17008                                 │
│                                                                                 │
│ ERP料號:4D010018                           規格型號:XPA72EA0I-008                │
│ 4D010018                                   XPA72EA0I-008                         │
│                                                                                 │
│ 數量:6,733                                 D/C (LOT NO. ):00U35NVVR              │
│ 6,733                                      00U35NVVR                             │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 使用者選擇 QW075551-2 模板後，可在 3 秒內產生單張標籤 PDF。
- **SC-002**: 產生的 PDF 標籤尺寸精確為 100mm × 80mm。
- **SC-003**: 所有六個欄位均正確顯示雙行格式（小字行 + 大字行）。
- **SC-004**: 數量欄位正確顯示千分位格式。
- **SC-005**: 長文字欄位（30+ 字元）完整顯示，無截斷。
- **SC-006**: 標籤無 Barcode 與 QR Code 元素。
- **SC-007**: 標籤標題顯示「物料標籤」，非「出貨標籤 Shipping Label」。
- **SC-008**: 標籤具有外框邊界，無內部分隔線。

## Assumptions

- 字型使用微軟正黑體，與現有 QW075551-1 標籤一致。
- PDF 輸出為唯一驗收依據，預覽畫面僅供參考。
- 座標配置可依實際渲染效果微調，以視覺呈現符合設計圖為準。
- 線條粗細建議 0.5pt ~ 1pt，由實作決定。

## Dependencies

- 依賴現有 LabelRenderer 服務進行渲染。
- 依賴現有 Excel 匯入功能提供資料來源。
- 依賴 QuestPDF 進行 PDF 輸出。

## Out of Scope

- 字型規格變更（維持 raw_spec.md §5.3 定義）。
- 資料處理規範變更（數量千分位顯示、Trim 空白等維持原規格）。
- PDF 輸出與驗收規範變更（維持第 13 章定義）。
- QW075551-1 出貨標籤的任何變更。
