# Feature Specification: Excel 匯入警告分級

**Feature Branch**: `002-import-warning`
**Created**: 2026-01-29
**Status**: Draft
**Input**: 將 raw_delta_import.md 結構化為標準 spec

## User Scenarios & Testing

### User Story 1 - 匯入含底線欄位的 Excel 檔案 (Priority: P1)

使用者匯入包含標準欄位（如 `nvr_cust`、`nvr_cust_item_no` 等含底線的欄位名稱）的 Excel 檔案時，系統應正確識別並匯入，不再顯示「非法字元」警告。

**Why this priority**: 這是核心問題修正，目前系統將規格書定義的合法欄位誤判為非法，導致資料無法正確匯入。

**Independent Test**: 可透過匯入包含 `nvr_cust` 欄位的 Excel 檔案進行測試，確認匯入成功且無「非法字元」警告。

**Acceptance Scenarios**:

1. **Given** 一個包含 `nvr_cust`、`nvr_cust_item_no`、`nvr_cust_pn`、`nvr_remark10` 欄位的 Excel 檔案，**When** 使用者匯入該檔案，**Then** 系統成功匯入所有欄位，不顯示「非法字元」警告。

2. **Given** 欄位名稱包含英數字及底線，**When** 系統驗證欄位名稱，**Then** 該欄位被判定為合法。

3. **Given** 欄位名稱包含其他特殊字元（如 `-`、`@`、空格），**When** 系統驗證欄位名稱，**Then** 該欄位被判定為非法並顯示警告。

---

### User Story 2 - 依等級顯示匯入訊息 (Priority: P1)

使用者匯入 Excel 後，系統依據問題嚴重程度分級顯示訊息（Error/Warning/Info），讓使用者快速判斷是否需要處理。

**Why this priority**: 目前所有訊息統一顯示為「警告」，使用者無法區分嚴重問題與資訊提示，影響操作效率。

**Independent Test**: 可透過匯入包含各類問題的 Excel 檔案進行測試，確認訊息正確分級顯示。

**Acceptance Scenarios**:

1. **Given** 匯入一個不存在的檔案，**When** 匯入操作完成，**Then** 系統顯示 Error 等級訊息「找不到指定的檔案」並中斷匯入。

2. **Given** 匯入的 Excel 缺少必要欄位，**When** 匯入操作完成，**Then** 系統顯示 Warning 等級訊息「缺少必要欄位：[欄位名稱]」。

3. **Given** 匯入的 Excel 包含額外欄位（不在對應清單中），**When** 匯入操作完成，**Then** 系統顯示 Info 等級訊息「欄位 '[名稱]' 不在對應清單中，已忽略」。

---

### User Story 3 - 檢視匯入結果摘要 (Priority: P2)

使用者匯入完成後，系統顯示清晰的結果摘要，包含匯入筆數及各等級訊息數量，Info 類訊息預設收合。

**Why this priority**: 提供結構化的結果摘要，提升使用體驗，但核心功能（分級）需先完成。

**Independent Test**: 可透過匯入檔案後檢視結果對話框進行測試，確認摘要格式符合預期。

**Acceptance Scenarios**:

1. **Given** 成功匯入 10 筆資料且無任何警告，**When** 匯入操作完成，**Then** 系統顯示「匯入成功，共 10 筆資料」。

2. **Given** 成功匯入 10 筆資料但有 1 個 Warning 和 2 個 Info，**When** 匯入操作完成，**Then** 系統顯示摘要：匯入成功筆數、Warning 訊息列表、Info 訊息（預設收合，可展開）。

3. **Given** 匯入結果包含 Info 訊息，**When** 結果對話框顯示，**Then** Info 區段預設為收合狀態，使用者可點擊展開查看詳情。

---

### User Story 4 - 含分號欄位值警告 (Priority: P3)

當匯入的資料欄位值包含分號時，系統顯示 Warning 提醒使用者可能影響 QR Code 產出。

**Why this priority**: 這是輔助性的資料品質提示，對核心匯入流程影響較小。

**Independent Test**: 可透過匯入包含分號欄位值的 Excel 進行測試。

**Acceptance Scenarios**:

1. **Given** Excel 中 `pono` 欄位值為 `ABC;123`，**When** 匯入操作完成，**Then** 系統顯示 Warning「欄位 'pono' 值 'ABC;123' 包含分號，可能影響 QR Code」。

---

### Edge Cases

- **空白列處理**：Excel 中的空白列應被略過，並記錄為 Info 等級訊息。
- **數量欄位千分位**：若數量欄位值包含千分位格式（如 `1,234`），系統應自動移除並顯示 Warning。
- **檔案格式錯誤**：若檔案不是有效的 .xlsx 格式，系統應顯示 Error 並中斷匯入。
- **大量訊息**：當 Info 訊息超過 10 條時，應顯示「共 N 條資訊」摘要而非全部列出。

## Requirements

### Functional Requirements

- **FR-001**: 系統 MUST 接受欄位名稱包含英數字（A-Z、a-z、0-9）及底線（_）。
- **FR-002**: 系統 MUST 拒絕欄位名稱包含其他特殊字元（如 `-`、`@`、空格等）。
- **FR-003**: 系統 MUST 將匯入訊息分為三個等級：Error、Warning、Info。
- **FR-004**: Error 等級訊息 MUST 中斷匯入流程並顯示錯誤對話框。
- **FR-005**: Warning 等級訊息 MUST 允許匯入繼續，並在結果摘要中顯示。
- **FR-006**: Info 等級訊息 MUST 預設隱藏，使用者可選擇展開查看。
- **FR-007**: 匯入結果摘要 MUST 顯示總匯入筆數。
- **FR-008**: 匯入結果摘要 MUST 分別顯示各等級訊息數量。
- **FR-009**: 系統 MUST 依據下列規則分類訊息：
  - Error：檔案不存在、檔案格式錯誤
  - Warning：必要欄位缺失、欄位值含分號、數量欄位含千分位
  - Info：額外欄位被忽略、空白列被略過
- **FR-010**: 系統 MUST 自動移除數量欄位中的千分位格式。

### Key Entities

- **ImportMessage**: 代表匯入過程中產生的單一訊息，包含等級（Severity）、訊息內容（Message）、關聯欄位（FieldName，可選）。
- **ImportResult**: 代表匯入操作的整體結果，包含成功筆數（SuccessCount）、訊息列表（Messages）、是否成功（IsSuccess）。
- **MessageSeverity**: 訊息等級列舉，包含 Error、Warning、Info 三個值。

## Success Criteria

### Measurable Outcomes

- **SC-001**: 包含 `nvr_cust` 等含底線欄位的 Excel 匯入後，100% 不顯示「非法字元」警告。
- **SC-002**: 使用者可在匯入結果摘要中 3 秒內判斷是否需要處理（透過明確的等級區分）。
- **SC-003**: Error 等級問題 100% 中斷匯入流程。
- **SC-004**: Info 等級訊息預設隱藏，使用者可一次點擊展開。
- **SC-005**: 現有可成功匯入的 Excel 檔案仍 100% 可正常匯入（向後相容）。

## Assumptions

- 欄位名稱規則修正後，正規表達式為 `^[A-Za-z0-9_]+$`。
- Info 訊息收合/展開機制使用 WPF 標準 Expander 控件。
- 現有 `data.json` 格式維持不變，不影響後續標籤列印流程。
- 此功能不包含匯入來源格式調整、效能優化、列印流程修改。

## Out of Scope

- 匯入資料來源格式調整
- 效能優化
- 列印流程修改
- 欄位名稱自動修正（如 `nvr_cust` → `nvrcust`）
