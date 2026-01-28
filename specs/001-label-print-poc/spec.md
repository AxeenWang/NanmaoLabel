# Feature Specification: 南茂標籤列印 POC

**Feature Branch**: `001-label-print-poc`
**Created**: 2026-01-27
**Status**: Draft
**Input**: User description: "南茂標籤列印 POC - 讀取交易資料，依據預定義的標籤格式，產出正確且條碼可被掃描的標籤 PDF 檔案"

## Clarifications

### Session 2026-01-27

- Q: 審查報告指出 spec.md 遺漏 raw_spec 第 13 章關鍵實作約束規範 → A: 新增 Implementation Constraints 區塊，補充 28 條強制約束（IC-001 至 IC-028），涵蓋座標渲染、資料值分離、條碼約束、UI 互動、資料匯入、文字字型、驗收權威來源、訊息語言等規範
- Q: FR 未標註對應的 raw_spec 章節編號（憲章 A-4 需求追溯原則）→ A: 為 Assumptions 與 Edge Cases 補充 `[ref: raw_spec X.X]` 追溯標註
- Q: 遺漏雙擊防抖、欄位名稱非法字元等 Edge Cases → A: 補充 3 項遺漏的 Edge Cases

### Session 2026-01-28

- Q: SD-03 規範缺陷 - raw_spec 13.17「300 DPI」定義模糊（是渲染 DPI 還是列印 DPI？）→ A: raw_spec 13.17 已補充「條碼圖片生成規範」表格，明確定義：(1) 300 DPI 為條碼光柵化生成時的像素密度基準 (2) 換算公式：像素 = 實體尺寸(mm) × 300 ÷ 25.4 (3) Code 128 高度 10mm = 118px，QR Code 20mm = 236px

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 標籤列印與 PDF 輸出 (Priority: P1)

產線操作員需要從已匯入的交易資料中選取特定紀錄，即時預覽標籤內容，並輸出可列印的 PDF 檔案。標籤上的一維條碼（Code 128）與 QR Code 必須能被掃描設備穩定讀取。

**Why this priority**: 這是 POC 的核心功能，驗證「資料對應與條碼輸出正確性」是本專案的首要目標。

**Independent Test**: 可透過選取任一筆資料、預覽標籤、輸出 PDF、並使用手機或掃描槍掃描條碼來完整測試此功能。

**Acceptance Scenarios**:

1. **Given** 系統已載入 data.json 且有交易資料，**When** 操作員單擊 ListView 中的一筆資料，**Then** 左側預覽區即時顯示該筆資料的標籤內容
2. **Given** 操作員已選取一筆資料且預覽成功，**When** 點擊「輸出 PDF」按鈕，**Then** 系統產出 PDF 檔案至 output 目錄，狀態列顯示完整檔案路徑
3. **Given** 已產出的 PDF 檔案，**When** 使用手機或掃描槍掃描 PDF 上的 Code 128 條碼，**Then** 首次嘗試即成功讀取，內容與資料源完全一致
4. **Given** 已產出的 PDF 檔案，**When** 使用手機或掃描槍掃描 PDF 上的 QR Code，**Then** 首次嘗試即成功讀取，內容為各欄位值以分號串接的格式

---

### User Story 2 - 標籤格式切換 (Priority: P1)

操作員需要在兩種內建標籤格式（QW075551-1 出貨標籤、QW075551-2）之間切換，以因應不同客戶或產品的標籤需求。

**Why this priority**: 支援多種標籤格式是 POC 必須驗證的核心功能之一。

**Independent Test**: 可透過切換標籤格式下拉選單，觀察預覽區即時更新為不同版面配置來驗證。

**Acceptance Scenarios**:

1. **Given** 操作員已選取一筆資料，**When** 從下拉選單切換為 QW075551-1 格式，**Then** 預覽區顯示出貨標籤版面（含客戶名稱、日期、數量、產品型號、條碼、QR Code）
2. **Given** 操作員已選取一筆資料，**When** 從下拉選單切換為 QW075551-2 格式，**Then** 預覽區顯示另一版面（含 Customer PO、CS Number、ERP Part NO.、條碼、QR Code）
3. **Given** 切換格式後，**When** 輸出 PDF，**Then** PDF 版面與預覽畫面一致（誤差 ≤ ±0.5mm）

---

### User Story 3 - 批次輸出 (Priority: P2)

操作員需要將所有交易資料一次性輸出為多頁 PDF，以提升大量標籤列印的效率。

**Why this priority**: 批次處理提升作業效率，但單筆輸出功能已能滿足基本需求。

**Independent Test**: 可透過點擊「批次輸出全部」按鈕，檢查產出的 PDF 是否包含所有資料的標籤來驗證。

**Acceptance Scenarios**:

1. **Given** 系統有 10 筆交易資料，**When** 操作員點擊「批次輸出全部」按鈕，**Then** 系統產出一份 10 頁的 PDF，每頁一張標籤
2. **Given** 批次輸出完成，**When** 系統顯示完成對話框，**Then** 操作員可選擇「開啟資料夾」或「確定」

---

### User Story 4 - 資料匯入 (Priority: P2)

管理者需要從 Excel 檔案（Data.xlsx）匯入交易資料，系統將資料儲存為 JSON 格式供後續使用。

**Why this priority**: 資料匯入是系統的資料來源，但 POC 階段可透過預置的 data.json 進行測試。

**Independent Test**: 可透過匯入一份 Excel 檔案，檢查 data.json 是否正確產生來驗證。

**Acceptance Scenarios**:

1. **Given** 管理者在「資料管理」分頁，**When** 點擊「匯入」按鈕並選擇有效的 Excel 檔案，**Then** 系統解析 Excel 並覆蓋現有 data.json
2. **Given** Excel 檔案中有 5 筆資料，**When** 匯入完成，**Then** 狀態列顯示「匯入成功，共 5 筆資料」
3. **Given** Excel 欄位名稱大小寫不一致（如 ogb19、OGB19、Ogb19），**When** 匯入，**Then** 系統正確識別為同一欄位（不分大小寫）
4. **Given** Excel 欄位值包含前後空白，**When** 匯入，**Then** 系統自動 Trim 空白

---

### User Story 5 - 資料管理（新增/編輯/刪除/儲存） (Priority: P3)

管理者需要能夠手動新增、編輯、刪除交易資料，並儲存變更。

**Why this priority**: 提供現場展示 POC 真實性的能力，但非日常主要操作流程。

**Independent Test**: 可透過新增一筆資料、編輯欄位、儲存、切換至標籤列印分頁並輸出該筆標籤來完整測試。

**Acceptance Scenarios**:

1. **Given** 管理者在「資料管理」分頁，**When** 點擊「新增」按鈕，**Then** DataGrid 新增一筆空白資料列
2. **Given** 管理者選取一筆資料，**When** 在欄位編輯區修改數值，**Then** DataGrid 即時同步更新
3. **Given** 管理者已修改資料（未儲存），**When** 點擊「儲存」按鈕，**Then** 變更寫入 data.json，狀態列顯示「儲存成功」
4. **Given** 管理者已修改資料（未儲存），**When** 嘗試切換分頁，**Then** 系統提示「資料尚未儲存，是否要儲存變更？」

---

### User Story 6 - 自動載入與程式啟動 (Priority: P1)

程式啟動時自動載入 data.json，操作員無需手動操作即可開始列印標籤。

**Why this priority**: 這是日常使用的基本流程起點。

**Independent Test**: 可透過啟動程式並觀察 ListView 是否自動顯示資料來驗證。

**Acceptance Scenarios**:

1. **Given** 程式目錄下存在 data.json，**When** 啟動程式，**Then** ListView 自動載入並顯示所有資料
2. **Given** data.json 不存在或讀取失敗，**When** 啟動程式，**Then** 程式以空白狀態啟動，顯示「尚無資料，請至資料管理分頁匯入 Excel」

---

### Edge Cases

- 當 Excel 欄位值包含分號時，系統應顯示警告（影響 QR Code 解析）[ref: raw_spec 13.4]
- 當必要欄位缺失時，系統應彈出警告視窗並說明無法產生標籤 [ref: raw_spec 3.3]
- 當 PDF 輸出路徑不存在時，系統應自動建立 output 目錄 [ref: raw_spec 2.3]
- 當 PDF 檔案已存在時，系統應詢問是否覆蓋 [ref: raw_spec 8.9]
- 當數量欄位包含非數字字元時，系統應阻擋輸入或顯示驗證錯誤 [ref: raw_spec 13.14]
- 當條碼內容為空值時，不產生條碼圖形 [ref: raw_spec 13.15]
- 當 QR Code 組合欄位中有空值時，保留欄位位置（如 `A;;C`）[ref: raw_spec 13.4, 13.15]
- 當 Excel 欄位名稱含底線、空白、特殊符號時，視為欄位缺失並顯示錯誤 [ref: raw_spec 13.11]
- 當雙擊事件被誤判為兩次單擊時，系統應透過防抖機制確保僅觸發一次輸出 [ref: raw_spec 13.6]
- 當匯入的數量欄位含千分位符號時，視為資料格式錯誤 [ref: raw_spec 13.14]

## Requirements *(mandatory)*

### Functional Requirements

**標籤列印功能**

- **FR-001**: 系統 MUST 在程式啟動時自動載入 data.json 並顯示於 ListView
- **FR-002**: 系統 MUST 支援單擊 ListView 項目即時預覽標籤
- **FR-003**: 系統 MUST 支援雙擊 ListView 項目直接輸出該筆標籤 PDF
- **FR-004**: 系統 MUST 支援切換內建的兩種標籤格式（QW075551-1、QW075551-2）
- **FR-005**: 系統 MUST 將變數對應至資料源欄位取值（如 nvr_cust → 客戶名稱）
- **FR-006**: 系統 MUST 識別並輸出常數值（如 QW075551-2 的 CSNUMBER 固定為 "17008"）
- **FR-007**: 系統 MUST 支援生成 Code 128 一維條碼，高度 10mm，保留 Quiet Zone
- **FR-008**: 系統 MUST 支援生成 QR Code，尺寸 20mm x 20mm，容錯等級 Level M
- **FR-009**: 系統 MUST 將標籤輸出為 PDF 檔案，尺寸 100mm x 60mm
- **FR-010**: 系統 MUST 支援批次輸出所有資料為多頁 PDF（每頁一張標籤）

**資料管理功能**

- **FR-011**: 系統 MUST 支援從 Excel 檔案匯入交易資料（覆蓋模式）
- **FR-012**: 系統 MUST 將匯入的資料儲存為 data.json
- **FR-013**: 系統 MUST 支援手動新增、編輯、刪除資料
- **FR-014**: 系統 MUST 支援儲存資料變更至 data.json
- **FR-015**: 系統 MUST 在切換分頁時檢查未儲存變更並提示使用者

**資料處理規範**

- **FR-016**: 系統 MUST 讀取 Excel 時將所有內容視為字串處理（避免前導零消失）
- **FR-017**: 系統 MUST 欄位名稱比對不分大小寫
- **FR-018**: 系統 MUST 對所有欄位值 Trim 前後空白
- **FR-019**: 系統 MUST 將日期統一轉換為 `yyyy-MM-dd` 格式
- **FR-020**: 系統 MUST 在顯示時為數量加入千分位，但條碼/QR Code 編碼使用原始數值
- **FR-021**: 系統 MUST 長文字超出寬度時裁切並加上省略號（不縮字、不換行）

**條碼與 QR Code**

- **FR-022**: 系統 MUST 條碼下方顯示編碼內容（8pt 字型，置中）
- **FR-023**: 系統 MUST QR Code 使用 UTF-8 編碼（支援中文）
- **FR-024**: 系統 MUST QR Code 組合欄位使用分號串接（如 `{pono};{ima902};{ogd09}`）
- **FR-025**: 系統 MUST 條碼/QR Code 保留 Quiet Zone，不得裁切

**UI 與互動**

- **FR-026**: 系統 MUST 採用 Kiosk 風格設計（大按鈕、大字體、高對比）
- **FR-027**: 系統 MUST 支援 16:9 橫式自適應佈局（最小解析度 1024 x 576）
- **FR-028**: 系統 MUST 提供全螢幕模式（F11 切換，ESC 離開）
- **FR-029**: 系統 MUST 無資料時顯示空白狀態提示並停用輸出按鈕

### Key Entities

- **DataRecord**: 交易資料紀錄，包含單據編號（ogb19）、客戶名稱（nvr_cust）、客戶料號（nvr_cust_item_no）、客戶 P/N（nvr_cust_pn）、數量（ogd09）、日期（obe25）、製令單號（pono）、裝置編號（ima902）、備註（nvr_remark10）、ERP 料號（erpmat）、客戶採購單號（cscustpo）等 17 個欄位
- **LabelTemplate**: 標籤格式定義，包含標籤代碼、名稱、尺寸（寬度/高度 mm）、欄位清單
- **LabelField**: 標籤欄位定義，包含欄位名稱、資料來源欄位、類型（Text/Barcode/QRCode）、座標（X/Y mm）、尺寸（寬度/高度 mm）、字型大小、對齊方式、是否常數值、組合模式

## Success Criteria *(mandatory)*

### Measurable Outcomes

**核心驗收標準（三項同時成立方為通過）**

- **SC-001**: 所有條碼與 QR Code 可被手機或掃描槍首次嘗試即穩定掃描（無需特定角度、距離或多次嘗試）
- **SC-002**: 掃描結果與 data.json 中的資料完全一致
- **SC-003**: 預覽畫面與 PDF 輸出版面誤差 ≤ ±0.5mm

**功能完整性**

- **SC-004**: 使用者可在 2 分鐘內完成「匯入 Excel → 選取資料 → 輸出 PDF」的完整流程
- **SC-005**: 使用者可在 30 秒內完成「選取資料 → 輸出 PDF」的日常操作流程
- **SC-006**: 兩種標籤格式（QW075551-1、QW075551-2）皆能正確顯示與輸出

**資料一致性**

- **SC-007**: data.json 為系統唯一資料來源，所有畫面顯示與 PDF 輸出皆源自此檔案
- **SC-008**: 資料儲存後，重新啟動程式載入的資料與儲存前一致

**使用者體驗**

- **SC-009**: 90% 的操作可透過單擊或雙擊完成，無需複雜操作步驟
- **SC-010**: 錯誤訊息以繁體中文顯示，操作員可立即理解問題

## Implementation Constraints *(mandatory - 優先於所有其他章節)*

> 本節規範優先於所有其他章節。若與其他段落描述衝突，以本節為準。[ref: raw_spec 13.12]

### 座標與渲染一致性 [ref: raw_spec 13.1]

- **IC-001**: 座標單位 MUST 以 mm 為唯一真相（規格表格中的 X/Y/Width/Height 皆為 mm）
- **IC-002**: 預覽與 PDF 輸出 MUST 共用同一套渲染定位邏輯
- **IC-003**: 換算公式：mm → pt = `mm × 72 ÷ 25.4`；mm → px@300DPI = `mm × 300 ÷ 25.4`
- **IC-004**: 預覽與 PDF 版面誤差 MUST ≤ ±0.5mm [ref: raw_spec 13.23]

### 資料值分離原則 [ref: raw_spec 13.13]

- **IC-005**: 系統 MUST 區分 Raw Value（原始值）與 Display Value（顯示值）
  - Raw Value：僅 Trim，用於 Barcode/QRCode 編碼，**禁止任何格式化**
  - Display Value：可加千分位、日期格式化，用於畫面顯示與標籤文字
- **IC-006**: Barcode/QRCode 編碼內容 MUST 使用 Raw Value，禁止使用 Display Value

### 條碼與 QR Code 約束 [ref: raw_spec 13.3, 13.9, 13.16]

- **IC-007**: Barcode/QRCode 類型欄位 MUST NOT 套用任何裁切處理（禁止 Ellipsis、禁止縮字、禁止裁切）
- **IC-008**: Code 128 Quiet Zone MUST 保留左右各 10 倍最小單元寬度 [ref: raw_spec 6.1]
- **IC-009**: QR Code Quiet Zone MUST 保留四周各 4 倍單元寬度 [ref: raw_spec 6.2]
- **IC-010**: QR Code 容錯等級 MUST 為 Level M（15%）[ref: raw_spec 6.2]
- **IC-011**: 條碼尺寸 MUST NOT 因版面配置進行過度縮放或壓縮，影響掃描成功率即視為規格不符

### UI 互動約束 [ref: raw_spec 13.6]

- **IC-012**: 單擊 ListView MUST 只做預覽更新
- **IC-013**: 雙擊 ListView MUST 只觸發一次輸出（允許雙擊前先觸發單擊），禁止雙擊被判成兩次單擊導致不輸出或輸出兩次
- **IC-014**: PDF 預設檔名格式：
  - 單筆：`Label_{單據編號}_{yyyyMMdd_HHmmss}.pdf`
  - 批次：`Labels_Batch_{yyyyMMdd_HHmmss}.pdf`
- **IC-015**: PDF 輸出成功後狀態列 MUST 顯示 `✅ PDF 已輸出：{完整檔案路徑}`

### 資料匯入與儲存約束 [ref: raw_spec 13.5, 13.11]

- **IC-016**: Excel 欄位名稱 MUST 僅允許英數字（A-Z, a-z, 0-9），含底線、空白、特殊符號者視為欄位缺失
- **IC-017**: 儲存 data.json 時 MUST 更新 lastModified 為當次儲存時間（ISO 8601 格式）
- **IC-018**: data.json MUST 為系統唯一資料真相來源（Single Source of Truth），禁止記憶體或 UI 元件維護獨立資料狀態 [ref: raw_spec 13.25]

### 數量欄位約束 [ref: raw_spec 13.14]

- **IC-019**: 數量欄位輸入端 MUST 僅允許純數字（0-9），禁止千分位、空白、其他符號
- **IC-020**: 匯入含千分位的數值 MUST 視為資料格式錯誤

### 文字與字型約束 [ref: raw_spec 13.2]

- **IC-021**: 字型 MUST 為微軟正黑體（Bold / Regular）
- **IC-022**: PDF 輸出時 MUST 嵌入字型（Embed）
- **IC-023**: 日期格式 MUST 固定為 `yyyy-MM-dd`，不得自行更改
- **IC-024**: 文字溢出處理 MUST 統一為「先裁切（Ellipsis）、不換行、不縮字」，僅適用於 Text 類型欄位

### 驗收權威來源 [ref: raw_spec 13.10]

- **IC-025**: PDF 輸出結果 MUST 為唯一驗收權威來源，螢幕預覽僅作為操作輔助
- **IC-026**: 掃描不穩定（需多次嘗試、需特定角度/距離、不同設備成功率不一致）MUST 等同掃描失敗，驗收不通過 [ref: raw_spec 13.24]

### 訊息語言約束 [ref: raw_spec 13.21]

- **IC-027**: 所有錯誤、警告與提示訊息 MUST 以繁體中文為主要顯示語言
- **IC-028**: 英文僅允許用於欄位代碼、錯誤代碼、系統識別用途

## Assumptions

1. **執行環境**: 系統僅需支援 Windows 10/11，不需跨平台 [ref: raw_spec 7]
2. **標籤格式**: POC 階段僅支援兩種內建格式，不需動態載入外部設定檔 [ref: raw_spec 1]
3. **資料量**: POC 階段資料量預期為數十至數百筆 [ref: raw_spec 無明確定義，依 POC 性質推斷]
4. **印表機連接**: POC 階段僅輸出 PDF，不需直接連接標籤印表機 [ref: raw_spec 12]
5. **字型**: 使用者環境已安裝微軟正黑體，PDF 輸出時嵌入字型 [ref: raw_spec 5.3, 13.2]
6. **網路**: 系統為單機使用，不需網路連線功能 [ref: raw_spec 無明確定義]
7. **權限**: 程式有權限讀寫執行目錄下的 data 與 output 資料夾 [ref: raw_spec 2.3]

## Out of Scope (POC 階段不處理)

1. 搜尋/篩選功能
2. 外部設定檔匯入標籤格式
3. 欄位彈性設定 UI
4. 標籤格式視覺化編輯器
5. If-Else 等複雜邏輯處理
6. 資料庫連接（Oracle/SAP HANA）
7. 實際印表機連接（Zebra、TSC 等）
