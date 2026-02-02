# Feature Specification: QW075551-1 出貨標籤 Delta Spec 實作

**Feature Branch**: `006-delta-label-qw075551-1`
**Created**: 2026-02-02
**Status**: Draft
**Input**: 基於 `.dao/raw_delta_label_QW075551-1.md` Delta Spec 文件

## Clarifications

### Session 2026-02-02

- Q: 長文字縮小字體的最小字體下限為何？ → A: 設定最小 6pt 下限，低於此值則允許換行或截斷並加省略號

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 渲染符合新規格的 QW075551-1 標籤 (Priority: P1)

使用者匯入出貨資料後，系統應依據新版規格渲染 QW075551-1 出貨標籤，包含正確的欄位配置、版面尺寸及 QR Code。

**Why this priority**: 這是 Delta Spec 的核心功能，所有後續功能都依賴正確的標籤渲染結果。

**Independent Test**: 匯入測試資料後預覽標籤，視覺呈現應符合參考圖片，且所有欄位正確顯示。

**Acceptance Scenarios**:

1. **Given** 使用者已匯入包含完整欄位的出貨資料, **When** 選擇 QW075551-1 標籤格式進行預覽, **Then** 標籤尺寸為 100mm × 80mm，版面配置符合參考圖片
2. **Given** 標籤渲染中, **When** CSCUSTPN 欄位有值, **Then** 顯示為純文字（非條碼），位於 CSCUSTITEMNO 正下方
3. **Given** 標籤渲染中, **When** FINDPRTDC 日期欄位有值, **Then** 日期格式顯示為 `yyyy/MM/dd`（如 2025/11/14）

---

### User Story 2 - QR Code 正確編碼與呈現 (Priority: P1)

使用者預覽或輸出標籤時，QR Code 應依據新規格組合四個欄位（CSMO、OUTDEVICENO、CSQTY、CSREMARK），以分號分隔。

**Why this priority**: QR Code 為標籤的關鍵識別元素，編碼錯誤將導致下游系統無法正確讀取。

**Independent Test**: 掃描輸出的 QR Code，驗證內容格式為 `{製令單號};{裝置編號};{數量};{備註}`。

**Acceptance Scenarios**:

1. **Given** 資料包含 CSMO=511-251020041, OUTDEVICENO=FC7800280H-00000-00005, CSQTY=6733, CSREMARK=00U35NVVR, **When** 渲染 QR Code, **Then** 編碼內容為 `511-251020041;FC7800280H-00000-00005;6733;00U35NVVR`
2. **Given** 標籤渲染中, **When** QR Code 渲染完成, **Then** QR Code 尺寸為 20mm × 20mm，位於左下角 Remarks 區段
3. **Given** CSQTY 值為 6733, **When** 渲染標籤, **Then** 文字顯示值為 `6,733`（含千分位），但 QR Code 編碼中為 `6733`（原始值）

---

### User Story 3 - 版面配置符合新規格 (Priority: P2)

使用者預覽標籤時，版面配置應符合新規格定義，包含 Remarks 區段採用 QR Code 與文字並排配置。

**Why this priority**: 正確的版面配置確保標籤可讀性與專業外觀。

**Independent Test**: 預覽標籤並與參考圖片對比，版面結構應一致。

**Acceptance Scenarios**:

1. **Given** 標籤渲染中, **When** 渲染 Remarks 區段, **Then** QR Code 位於左側（X=5mm），文字（CSMO、OUTDEVICENO、CSREMARK）位於右側並排
2. **Given** 標籤渲染中, **When** 渲染完成, **Then** 標籤具有外框（單線矩形邊框），無分隔線
3. **Given** 標籤渲染中, **When** 渲染欄位標籤, **Then** "Product NO." 拼寫正確（非 "Peoduct NO."）

---

### User Story 4 - PDF 輸出符合新規格 (Priority: P2)

使用者輸出 PDF 時，頁面尺寸與內容應符合新版 100mm × 80mm 規格。

**Why this priority**: PDF 輸出為驗收的唯一依據，必須確保輸出結果正確。

**Independent Test**: 輸出 PDF 並量測頁面尺寸與各元素位置。

**Acceptance Scenarios**:

1. **Given** 使用者選擇輸出 PDF, **When** 輸出 QW075551-1 標籤, **Then** PDF 頁面尺寸為 100mm × 80mm
2. **Given** PDF 輸出完成, **When** 檢查標籤內容, **Then** 無任何 Code 128 條碼，僅有 QR Code
3. **Given** PDF 輸出完成, **When** 掃描 QR Code, **Then** 可正確讀取內容且格式正確

---

### User Story 5 - 長文字溢出處理 (Priority: P3)

當欄位文字過長時，系統應自動縮小字體以完整顯示，不可截斷文字。

**Why this priority**: 確保所有資料完整顯示，避免資訊遺失。

**Independent Test**: 輸入超長文字資料，驗證字體自動縮小且文字完整顯示。

**Acceptance Scenarios**:

1. **Given** Text 欄位內容超過版面寬度, **When** 渲染該欄位, **Then** 自動縮小字體以完整顯示，不截斷
2. **Given** 多個欄位都需要縮小字體, **When** 渲染標籤, **Then** 各欄位獨立處理，互不影響

---

### Edge Cases

- 當 Text 欄位為空值時，保留版面位置，顯示空字串，不跳過或收合
- 當 QR Code 組合欄位中有空值時，輸出空字串但保留分號位置（如 `;FC7800280H;;`）
- 當日期欄位格式異常時，應有適當的錯誤處理或顯示原始值

## Requirements *(mandatory)*

### Functional Requirements

#### 標籤尺寸與版面

- **FR-001**: 系統必須將 QW075551-1 標籤尺寸由 100mm × 60mm 變更為 **100mm × 80mm**
- **FR-002**: 系統必須依據參考圖片版面配置各欄位座標，取代原 raw_spec.md §5.1 座標表
- **FR-003**: 系統必須繪製標籤外框（單線矩形邊框），不繪製分隔線
- **FR-004**: 系統必須將 "Peoduct NO." typo 修正為 "Product NO."

#### 欄位處理

- **FR-005**: 系統必須將 CSCUSTPN 欄位由 Barcode (Code 128) 變更為 **Text**，位於 CSCUSTITEMNO 正下方
- **FR-006**: 系統必須將日期欄位 (FINDPRTDC) 顯示格式由 `yyyy-MM-dd` 變更為 **`yyyy/MM/dd`**
- **FR-007**: 系統必須移除所有 Code 128 條碼，僅保留 QR Code 作為唯一條碼類型
- **FR-008**: 系統必須對所有 Text 欄位套用長文字縮小字體處理，最小字體下限為 **6pt**；若縮至 6pt 仍無法容納，則允許換行或截斷並加省略號

#### QR Code 規格

- **FR-009**: 系統必須依據新規格組合 QR Code 內容：`CSMO;OUTDEVICENO;CSQTY;CSREMARK`（固定 4 欄位，以分號分隔）
- **FR-010**: 系統必須確保 QR Code 中的 CSQTY 使用原始值（無千分位），而 Text 顯示使用千分位格式
- **FR-011**: 系統必須維持 QR Code 尺寸為 20mm × 20mm，容錯等級 Level M (15%)
- **FR-012**: 系統必須將 QR Code 位置由右下角移至左下角（Remarks 區段）

#### Remarks 區段版面

- **FR-013**: 系統必須將 Remarks 區段配置為 QR Code 與文字並排（QR Code 左側，文字右側）
- **FR-014**: 系統必須依序顯示 CSMO、OUTDEVICENO、CSREMARK 三行文字於 QR Code 右側

#### 空值處理

- **FR-015**: 系統必須對空值 Text 欄位顯示空字串，保留版面位置
- **FR-016**: 系統必須對 QR Code 組合中的空值欄位輸出空字串，保留分號位置

#### PDF 輸出

- **FR-017**: 系統必須將 PDF 輸出頁面尺寸同步調整為 100mm × 80mm
- **FR-018**: 系統必須確保 PDF 輸出為唯一驗收依據

#### 欄位前綴標籤

- **FR-019**: 系統必須採用雙行顯示欄位前綴標籤（中英文分行），如 "Customer" 與 "客戶名稱" 分行

### CodeSoft 欄位對照表

| Item | CodeSoft 欄位    | 資料來源欄位      | 顯示類型 | 備註                     |
|------|------------------|-------------------|----------|--------------------------|
| 1    | CSCUSTOMER       | nvr_cust          | Text     | 客戶名稱                 |
| 2    | FINDPRTDC        | obe25             | Text     | 日期，格式 yyyy/MM/dd    |
| 3    | CSQTY            | ogd09             | Text     | 數量，顯示含千分位       |
| 4    | CSCUSTITEMNO     | nvr_cust_item_no  | Text     | 客戶料號                 |
| 5    | CSCUSTPN         | nvr_cust_pn       | Text     | 客戶 P/N（原為 Barcode） |
| 6    | CSMO             | pono              | Text     | 製令單號                 |
| 7    | OUTDEVICENO      | ima902            | Text     | 裝置編號                 |
| 8    | CSREMARK         | nvr_remark10      | Text     | 備註                     |
| 9    | QRCODE           | CSMO;OUTDEVICENO;CSQTY;CSREMARK | QRCode | 以分號串接 |

### Key Entities

- **LabelTemplate (QW075551-1)**: 標籤模板定義，包含尺寸（100mm × 80mm）、欄位配置、座標資訊
- **LabelField**: 欄位定義，包含 CodeSoft 欄位名稱、資料來源欄位、顯示類型、座標
- **QRCodeContent**: QR Code 內容組合，包含欄位順序、分隔符號、編碼規格
- **RemarksSection**: Remarks 區段配置，包含 QR Code 與文字並排的版面定義

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: PDF 輸出頁面尺寸驗證為 100mm × 80mm
- **SC-002**: 日期欄位 (FINDPRTDC) 顯示格式為 `yyyy/MM/dd`（斜線分隔）
- **SC-003**: CSCUSTPN 顯示為純文字，無條碼渲染
- **SC-004**: QR Code 尺寸量測為 20mm × 20mm
- **SC-005**: QR Code 掃描內容格式正確，依 `{CSMO};{OUTDEVICENO};{CSQTY};{CSREMARK}` 順序
- **SC-006**: CSQTY 文字顯示含千分位（如 6,733），QR Code 內為原始值（如 6733）
- **SC-007**: 標籤具有外框，無分隔線
- **SC-008**: "Product NO." 拼寫正確
- **SC-009**: 長文字使用縮小字體處理（最小 6pt），超長文字允許換行或截斷加省略號
- **SC-010**: 無任何 Code 128 條碼存在
- **SC-011**: 欄位前綴標籤為中英文分行顯示

## Scope Boundary

### 納入本次範圍

- QW075551-1 標籤的 CodeSoft 欄位對照更新
- 標籤尺寸由 60mm 增至 80mm
- CSCUSTPN 由 Barcode 變更為 Text
- 日期格式由 `yyyy-MM-dd` 變更為 `yyyy/MM/dd`
- QR Code 組合語法統一為 CodeSoft 欄位名
- 版面座標全面以參考圖片為準

### 不納入本次範圍

- QW075551-2 標籤修正（將於後續另行處理）
- 字型規格變更（維持原規格 raw_spec.md §5.3）
- 條碼通用規格變更（維持原規格 raw_spec.md §6，但移除 Code 128）
- 資料處理通用規範（千分位、Trim 等維持原規格）
- PDF 輸出與驗收規範（維持原規格第 13 章）

## Assumptions

- 開發者可依據參考圖片自行配置各欄位座標，驗收以 PDF 輸出視覺呈現為準
- 參考圖片為 `出貨標籤格式_QW075551-1.png` 與 `出貨標籤預覽_QW075551-1.png`
- 標籤渲染實作位於 `LabelRenderer` 服務
- 長文字縮小字體的具體實作細節由開發者決定
- 座標參考值可依實際渲染效果微調，不需完全吻合圖片像素

## Dependencies

- **raw_spec.md**: 沿用字型規格（§5.3）、條碼通用規格（§6）、資料處理規範、PDF 輸出規範（第 13 章）
- **現有 LabelRenderer**: 需要擴充支援新版面配置與長文字處理
- **現有 BuiltInTemplates**: 需要更新 QW075551-1 模板定義

## Reference Documents

- `.dao/raw_delta_label_QW075551-1.md` - Delta Spec 完整定義（本規格來源）
- `.dao/raw_spec.md` v2.5 - 基準規格（沿用部分規範）
- `出貨標籤格式_QW075551-1.png` - 版面配置參考圖
- `出貨標籤預覽_QW075551-1.png` - 預覽效果參考圖
