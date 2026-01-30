# Feature Specification: ListView/DataGrid Item 選取狀態視覺規範

**Feature Branch**: `004-listview-style`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "讀取 raw_delta_listview.md 並將其結構化為標準規格"

---

## Clarifications

### Session 2026-01-30

- Q: ListView item 字體大小應為多少？ → A: 16pt（原規劃 15pt，因長時間閱讀舒適度考量調整為 16pt）

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 清楚辨識被選中的項目 (Priority: P1)

使用者在 ListView 或 DataGrid 中點擊某筆資料後，需要能夠清楚地看出哪一筆是目前被選中的項目。選中項目應有明顯的高亮藍色光條，且文字加粗以提升可辨識度。

**Why this priority**: 選取狀態是清單操作的核心功能，使用者必須隨時知道正在操作哪一筆資料，否則可能造成誤操作。

**Independent Test**: 可透過點擊任一項目，觀察是否出現完整藍色光條 (#0078D4) 且文字變為粗體白色來獨立驗證。

**Acceptance Scenarios**:

1. **Given** 使用者在標籤列印分頁的 ListView 中，**When** 點擊任一項目，**Then** 該項目顯示完整高亮藍色光條 (#0078D4)，無破碎、無裁切，文字變為粗體白色
2. **Given** 已選中某項目，**When** 點擊另一項目，**Then** 原選中項目回復正常狀態（無光條、一般字體），新項目變為選中狀態
3. **Given** 已選中某項目，**When** 滑鼠移入再移出該項目，**Then** 視覺狀態維持 Selected 不變

---

### User Story 2 - 懸停時預覽可點擊區域 (Priority: P2)

使用者將滑鼠移動到未選中的項目上時，應有淡藍色光條提示該項目可被點擊，讓使用者清楚知道互動目標。

**Why this priority**: Hover 回饋是良好使用體驗的標準功能，但優先級次於核心的選取功能。

**Independent Test**: 可透過將滑鼠移至未選中項目，觀察是否出現淡藍色光條 (#E5F3FF) 來獨立驗證。

**Acceptance Scenarios**:

1. **Given** ListView 中有多個未選中項目，**When** 滑鼠移入某項目，**Then** 該項目顯示淡藍色光條 (#E5F3FF)
2. **Given** 滑鼠懸停在未選中項目上，**When** 滑鼠移出該項目，**Then** 光條消失，回復正常背景
3. **Given** 滑鼠懸停在未選中項目上，**When** 點擊該項目，**Then** 光條從淡藍變為高亮藍，文字加粗

---

### User Story 3 - 跨分頁視覺一致性 (Priority: P2)

使用者在「標籤列印」與「資料管理」兩個分頁間切換時，ListView 與 DataGrid 的選取視覺效果應一致，避免使用者因視覺差異而困惑。

**Why this priority**: 視覺一致性是良好使用體驗的重要元素，與 Hover 回饋同等重要。

**Independent Test**: 可透過在兩個分頁分別選取項目，比對光條顏色、字體樣式是否一致來獨立驗證。

**Acceptance Scenarios**:

1. **Given** 使用者在標籤列印分頁選取某項目，**When** 切換至資料管理分頁並選取某列，**Then** 兩者的選取光條顏色 (#0078D4)、字體粗細 (Bold)、字體顏色 (#FFFFFF) 完全一致
2. **Given** 使用者在資料管理分頁 DataGrid 懸停，**When** 觀察 Hover 效果，**Then** 與標籤列印分頁 ListView 的 Hover 效果一致（#E5F3FF 淡藍）

---

### User Story 4 - 字體可讀性提升 (Priority: P3)

系統應將清單字體從 14pt 調整為 16pt，提升整體可讀性，尤其對於需要長時間操作的使用者。

**Why this priority**: 字體大小是增強體驗的優化項目，不影響核心功能。

**Independent Test**: 可透過視覺比對或開發者工具檢查字體大小是否為 16pt 來獨立驗證。

**Acceptance Scenarios**:

1. **Given** 使用者查看 ListView 或 DataGrid 中的項目，**When** 觀察字體大小，**Then** 字體為 16pt（相較先前 14pt 增大）

---

### Edge Cases

- 當 ListView/DataGrid 項目數量為 0（空清單）時，不應顯示任何光條或選取狀態
- 當快速連續點擊不同項目時，視覺狀態應即時切換，不應出現閃爍或延遲
- 當視窗縮放導致項目高度改變時，光條應自動調整以完整覆蓋項目區域
- 當 DataGrid 使用整列選取模式時，光條應覆蓋整列所有欄位

---

## Requirements *(mandatory)*

### Functional Requirements

#### 狀態優先級

- **FR-001**: 系統 MUST 實作三層狀態：Normal（預設）、Hover（懸停）、Selected（選中）
- **FR-002**: Selected 狀態視覺 MUST 具有最高優先級，不得被 Hover 狀態覆蓋
- **FR-003**: 當項目同時處於 Selected 且滑鼠懸停時，系統 MUST 維持 Selected 視覺效果不變

#### 視覺規範 - Normal 狀態

- **FR-004**: Normal 狀態 MUST 顯示透明背景（無光條）
- **FR-005**: Normal 狀態字體 MUST 為 16pt、Normal 粗細、#333333 顏色

#### 視覺規範 - Hover 狀態

- **FR-006**: Hover 狀態（未選中）MUST 顯示淡藍色光條 (#E5F3FF)
- **FR-007**: Hover 狀態光條 MUST 完整填滿項目區域，與可點擊區域一致
- **FR-008**: Hover 狀態字體 MUST 維持 Normal 粗細與 #333333 顏色

#### 視覺規範 - Selected 狀態

- **FR-009**: Selected 狀態 MUST 顯示高亮藍色光條 (#0078D4)
- **FR-010**: Selected 狀態光條 MUST 完整填滿項目區域，無破碎、無裁切、無缺角
- **FR-011**: Selected 狀態字體 MUST 為 Bold 粗體、#FFFFFF 白色

#### 光條完整性

- **FR-012**: 光條區域 MUST 與項目可點擊區域完全一致
- **FR-013**: 光條 MUST NOT 有缺角、斷裂或破碎
- **FR-014**: 光條 MUST NOT 小於或超出項目可點擊區域

#### 互動回饋

- **FR-015**: 狀態切換視覺回饋 MUST 即時呈現，無可感知延遲
- **FR-016**: 連續點擊不同項目時，視覺狀態 MUST 穩定切換不閃爍

#### 適用範圍

- **FR-017**: 上述視覺規範 MUST 同時適用於標籤列印分頁的 ListView 與資料管理分頁的 DataGrid
- **FR-018**: ListView 與 DataGrid MUST 共用相同的色彩定義，確保視覺一致

### Key Entities

- **ListViewItem**: 標籤列印分頁中的列表項目，具有 Normal、Hover、Selected 三種視覺狀態
- **DataGridRow**: 資料管理分頁中的表格列，具有與 ListViewItem 相同的三種視覺狀態
- **HighlightBar（光條）**: 項目選取或懸停時顯示的背景色區域，必須完整覆蓋項目可點擊範圍

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 所有 Selected 狀態光條無破碎、無裁切（100% 完整度）
- **SC-002**: Hover 與 Selected 狀態在視覺上明顯可區分（使用者能正確辨識當前狀態）
- **SC-003**: 狀態切換回饋在使用者感知範圍內為即時（無可察覺延遲）
- **SC-004**: 標籤列印與資料管理兩分頁的選取視覺效果 100% 一致
- **SC-005**: 連續快速切換選取項目時，視覺狀態 100% 穩定無閃爍
- **SC-006**: 字體大小提升至 16pt，提升可讀性

---

## Assumptions

- 本功能為純視覺變更，不影響現有的選取邏輯、分頁功能、雙擊行為或資料匯出功能
- ViewModel、Model、Services 層無需變更
- 使用者已熟悉基本的清單操作（點擊選取、滑鼠懸停）
- 系統使用的顯示器支援標準 RGB 色彩

---

## Out of Scope

- 多選功能（Ctrl+Click 或 Shift+Click）的視覺效果
- 鍵盤導航（上下鍵切換項目）的視覺效果
- 觸控裝置的觸控回饋
- 深色模式支援
- 動畫效果（淡入淡出）

---

## Dependencies

- 依賴 raw_spec.md 第 8.4、8.5、8.6 節定義的標籤列印分頁、資料管理分頁與尺寸規範
- 參考 raw_delta_button.md 的按鈕 Focus 狀態處理方式
