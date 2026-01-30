# Tasks: ListView/DataGrid Item 選取狀態視覺規範

**Input**: Design documents from `/specs/004-listview-style/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, quickstart.md

**Tests**: 本功能為純視覺變更，測試採用手動視覺驗收，無自動化測試任務。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

本專案為 WPF 單一專案：

```text
NanmaoLabelPOC/
├── App.xaml
├── Resources/
│   └── ListViewStyles.xaml  # 新增
└── Views/
    ├── LabelPrintView.xaml  # 修改
    └── DataManageView.xaml  # 修改
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立共用樣式資源檔與色彩 Token

- [x] T001 建立 `NanmaoLabelPOC/Resources/ListViewStyles.xaml` 檔案
- [x] T002 [P] 定義色彩 Token: `ListViewHighlightSelected` (#0078D4) 於 `Resources/ListViewStyles.xaml`
- [x] T003 [P] 定義色彩 Token: `ListViewHighlightHover` (#E5F3FF) 於 `Resources/ListViewStyles.xaml`
- [x] T004 [P] 定義色彩 Token: `ListViewTextNormal` (#333333) 於 `Resources/ListViewStyles.xaml`
- [x] T005 [P] 定義色彩 Token: `ListViewForegroundSelected` (#FFFFFF) 於 `Resources/ListViewStyles.xaml`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立共用樣式 ControlTemplate，所有 User Story 皆依賴此階段

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 定義 `ListViewItemStyle` ControlTemplate 於 `Resources/ListViewStyles.xaml`，包含：
  - Border 作為光條背景層（SnapsToDevicePixels="True", UseLayoutRounding="True"）
  - ContentPresenter 用於顯示內容
- [x] T007 [P] 定義 `DataGridRowStyle` ControlTemplate 於 `Resources/ListViewStyles.xaml`，結構同 T006
- [x] T008 [P] 定義 `DataGridCellStyle` 於 `Resources/ListViewStyles.xaml`：設定 Background="Transparent" 避免覆蓋 Row 光條
- [x] T009 在 `NanmaoLabelPOC/App.xaml` 的 `Application.Resources.MergedDictionaries` 加入 `Resources/ListViewStyles.xaml`

**Checkpoint**: 共用樣式資源已就緒，可開始各 User Story 實作

---

## Phase 3: User Story 1 - 清楚辨識被選中的項目 (Priority: P1) 🎯 MVP

**Goal**: Selected 狀態顯示完整高亮藍色光條 (#0078D4)，文字粗體白色，不被 Hover 覆蓋

**Independent Test**: 點擊任一項目，觀察是否出現完整藍色光條且文字變為粗體白色

**Acceptance Criteria**: [FR-001~FR-003, FR-009~FR-014]

### Implementation for User Story 1

- [x] T010 [US1] 在 `ListViewItemStyle` 加入 Trigger: IsSelected=True → Background=#0078D4, Foreground=White（`Resources/ListViewStyles.xaml`）
- [x] T011 [US1] 在 `DataGridRowStyle` 加入 Trigger: IsSelected=True → Background=#0078D4（`Resources/ListViewStyles.xaml`）
- [x] T012 [US1] 修改 `Views/LabelPrintView.xaml`: 移除 ListView.ItemContainerStyle 內現有 Style.Triggers（line 192-207）
- [x] T013 [US1] 修改 `Views/LabelPrintView.xaml`: 套用 `{StaticResource ListViewItemStyle}` 至 ListView.ItemContainerStyle
- [x] T014 [US1] 在 `Views/LabelPrintView.xaml` DataTemplate 加入 DataTrigger: IsSelected=True → FontWeight=Bold, Foreground=White
- [x] T015 [US1] 修改 `Views/DataManageView.xaml`: 移除 DataGrid.RowStyle 內現有 Style.Triggers（line 175-187）
- [x] T016 [US1] 修改 `Views/DataManageView.xaml`: 套用 `{StaticResource DataGridRowStyle}` 至 DataGrid.RowStyle
- [x] T017 [US1] 修改 `Views/DataManageView.xaml`: 套用 `{StaticResource DataGridCellStyle}` 至 DataGrid.CellStyle

**Checkpoint**: Selected 狀態視覺完成，光條完整無破碎

---

## Phase 4: User Story 2 - 懸停時預覽可點擊區域 (Priority: P2)

**Goal**: Hover 狀態（未選中時）顯示淡藍色光條 (#E5F3FF)，不覆蓋 Selected 狀態

**Independent Test**: 滑鼠移至未選中項目，觀察是否出現淡藍色光條

**Acceptance Criteria**: [FR-006~FR-008]

### Implementation for User Story 2

- [ ] T018 [US2] 在 `ListViewItemStyle` 加入 MultiTrigger: IsMouseOver=True AND IsSelected=False → Background=#E5F3FF（`Resources/ListViewStyles.xaml`）
- [ ] T019 [US2] 在 `DataGridRowStyle` 加入 MultiTrigger: IsMouseOver=True AND IsSelected=False → Background=#E5F3FF（`Resources/ListViewStyles.xaml`）
- [ ] T020 [US2] 確認 Trigger 順序：MultiTrigger (Hover) 在前，Trigger (IsSelected) 在後，確保 Selected 優先級最高（`Resources/ListViewStyles.xaml`）

**Checkpoint**: Hover 狀態視覺完成，且 Selected 狀態不被 Hover 覆蓋

---

## Phase 5: User Story 3 - 跨分頁視覺一致性 (Priority: P2)

**Goal**: 標籤列印 (ListView) 與資料管理 (DataGrid) 兩分頁的選取視覺效果 100% 一致

**Independent Test**: 在兩個分頁分別選取項目，比對光條顏色、字體樣式是否一致

**Acceptance Criteria**: [FR-017, FR-018, SC-004]

### Implementation for User Story 3

- [ ] T021 [US3] 驗證 `Views/DataManageView.xaml` DataGrid 已設定 `SelectionUnit="FullRow"`
- [ ] T022 [US3] 確認 ListView 與 DataGrid 皆使用相同色彩 Token（ListViewHighlightSelected, ListViewHighlightHover）（`Resources/ListViewStyles.xaml`）
- [ ] T023 [US3] 視覺比對：標籤列印分頁 ListView 與資料管理分頁 DataGrid 的 Selected 光條顏色一致
- [ ] T024 [US3] 視覺比對：兩分頁的 Hover 光條顏色一致

**Checkpoint**: 兩分頁視覺 100% 一致

---

## Phase 6: User Story 4 - 字體可讀性提升 (Priority: P3)

**Goal**: 清單字體從 14pt 調整為 16pt，提升長時間閱讀舒適度

**Independent Test**: 視覺比對或開發者工具檢查字體大小是否為 16pt

**Acceptance Criteria**: [FR-005, SC-006]

### Implementation for User Story 4

- [ ] T025 [P] [US4] 修改 `Views/LabelPrintView.xaml` DataTemplate 內所有 TextBlock FontSize 從 14 改為 16（line 157, 159, 171, 185, 187）
- [ ] T026 [P] [US4] 修改 `Views/DataManageView.xaml` DataGrid FontSize 從 13 改為 16（若需調整）或確認欄位編輯區維持現有大小
- [ ] T027 [US4] 確認字體變大後版面無溢出，TextTrimming="CharacterEllipsis" 設定正確（`Views/LabelPrintView.xaml`）

**Checkpoint**: 字體可讀性提升完成

---

## Phase 7: Polish & Validation

**Purpose**: 最終驗收與跨功能檢查

- [ ] T028 編譯專案確認無 XAML 警告（`dotnet build`）
- [ ] T029 執行應用程式確認無 Binding 錯誤（檢查 Output 視窗）
- [ ] T030 [P] 執行視覺驗收清單 V-001~V-007（quickstart.md）
- [ ] T031 [P] 執行互動驗收清單 I-001~I-004（quickstart.md）
- [ ] T032 [P] 執行跨頁驗收清單 C-001~C-003（quickstart.md）
- [ ] T033 確認 ListView 功能正常：選取、雙擊、分頁
- [ ] T034 確認 DataGrid 功能正常：選取、編輯、排序

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (Phase 3) → US2 (Phase 4) 建議依序執行（Trigger 順序影響）
  - US3 (Phase 5) depends on US1+US2 complete
  - US4 (Phase 6) can run in parallel with US2/US3
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

```text
Phase 1 (Setup)
    ↓
Phase 2 (Foundational) ─── BLOCKS ALL STORIES
    ↓
Phase 3 (US1: Selected) ───→ Phase 4 (US2: Hover) ───→ Phase 5 (US3: 一致性)
    ↓                            ↓
    └────────────────────────────┴───→ Phase 6 (US4: 字體) [可並行]
                                            ↓
                                      Phase 7 (Polish)
```

### Parallel Opportunities

**Phase 1** (色彩 Token 定義):
- T002, T003, T004, T005 可並行執行

**Phase 2** (ControlTemplate 定義):
- T007, T008 可並行執行（不同 Style）

**Phase 6** (字體調整):
- T025, T026 可並行執行（不同檔案）

**Phase 7** (驗收):
- T030, T031, T032 可並行執行

---

## Parallel Example: Phase 1

```bash
# Launch all color token definitions together:
Task: "T002 [P] 定義色彩 Token: ListViewHighlightSelected (#0078D4)"
Task: "T003 [P] 定義色彩 Token: ListViewHighlightHover (#E5F3FF)"
Task: "T004 [P] 定義色彩 Token: ListViewTextNormal (#333333)"
Task: "T005 [P] 定義色彩 Token: ListViewForegroundSelected (#FFFFFF)"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T009)
3. Complete Phase 3: User Story 1 (T010-T017)
4. **STOP and VALIDATE**: Selected 狀態光條完整、不被 Hover 覆蓋
5. 可提前 Demo MVP

### Incremental Delivery

1. Setup + Foundational → 樣式資源就緒
2. Add US1 (Selected) → 核心功能完成 → **MVP!**
3. Add US2 (Hover) → 懸停回饋完成
4. Add US3 (一致性) → 跨分頁統一
5. Add US4 (字體) → 可讀性優化
6. Polish → 最終驗收

---

## Notes

- [P] tasks = different files, no dependencies
- [US#] label maps task to specific user story for traceability
- 本功能為純 XAML 視覺變更，無後端邏輯
- Trigger 順序重要：MultiTrigger (Hover) 必須在 Trigger (IsSelected) 之前
- 每完成一個 Phase 建議 commit
- 視覺驗收依據 quickstart.md 檢查清單

---

## Summary

| Phase | Tasks | User Story | 可並行 |
|-------|-------|------------|--------|
| 1. Setup | T001-T005 | - | T002-T005 |
| 2. Foundational | T006-T009 | - | T007-T008 |
| 3. US1 Selected | T010-T017 | P1 | - |
| 4. US2 Hover | T018-T020 | P2 | - |
| 5. US3 一致性 | T021-T024 | P2 | - |
| 6. US4 字體 | T025-T027 | P3 | T025-T026 |
| 7. Polish | T028-T034 | - | T030-T032 |

**Total Tasks**: 34
