# Tasks: 按鈕樣式規範（極簡 Kiosk 風格）

**Input**: Design documents from `/specs/003-button-style/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: 視覺驗證以手動為主，僅核心邏輯（狀態優先序）需單元測試。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **WPF Application**: `NanmaoLabelPOC/` at repository root
- **Tests**: `NanmaoLabelPOC.Tests/` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立樣式資源基礎架構

- [x] T001 建立 Resources 目錄結構於 NanmaoLabelPOC/Resources/
- [x] T002 [P] 建立 ButtonStyles.xaml 於 NanmaoLabelPOC/Resources/ButtonStyles.xaml（空檔案含 ResourceDictionary 根元素）
- [x] T003 修改 App.xaml 合併 ButtonStyles.xaml 於 NanmaoLabelPOC/App.xaml

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 定義顏色 Token，所有 User Story 共用

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 定義 Action 按鈕顏色 Token（ButtonDefaultBackground #1E3A5F, ButtonDefaultForeground #FFFFFF, ButtonHoverBackground #2E4A6F, ButtonActiveBackground #0E2A4F, ButtonFocusBorder #FFFFFF）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T005 [P] 定義 Disabled 按鈕顏色 Token（ButtonDisabledBackground #E0E0E0, ButtonDisabledForeground #A0A0A0）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T006 [P] 定義 Secondary 按鈕顏色 Token（SecondaryDefaultBackground #FFFFFF, SecondaryDefaultForeground #333333, SecondaryHoverBackground #F0F0F0, SecondaryBorder #E0E0E0）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T007 [P] 定義 Feedback 顏色 Token（FeedbackSuccess #107C10, FeedbackError #D13438）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T008 建立 ActionButtonStyle 基礎樣式（TargetType=Button, 深藍底白字, CornerRadius=4）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T009 建立 SecondaryButtonStyle 樣式（白底黑字帶邊框, MinWidth=40, MinHeight=40）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml

**Checkpoint**: Foundation ready - Color Tokens 與基礎樣式可用，user story implementation can now begin

---

## Phase 3: User Story 1 - 產線操作員快速操作按鈕 (Priority: P1) 🎯 MVP

**Goal**: 實作位置分區佈局，讓操作員透過固定位置形成肌肉記憶

**Independent Test**: 啟動應用程式，檢查資料管理工具列按鈕位置：匯入/新增在左側、刪除在中間（間距≥32px）、儲存在右側

**Ref**: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006

### Implementation for User Story 1

- [x] T010 [US1] 修改 DataManageView.xaml 工具列佈局為三欄 Grid（建立區 | 危險區 | 確認區）於 NanmaoLabelPOC/Views/DataManageView.xaml
- [x] T011 [US1] 套用 8px 間距於建立區內（匯入↔新增）於 NanmaoLabelPOC/Views/DataManageView.xaml
- [x] T012 [US1] 套用 32px 間距於危險區（刪除按鈕）與相鄰區域於 NanmaoLabelPOC/Views/DataManageView.xaml
- [x] T013 [US1] 修改 LabelPrintView.xaml 輸出按鈕佈局為並排置中於 NanmaoLabelPOC/Views/LabelPrintView.xaml
- [x] T014 [US1] 修改 LabelPrintView.xaml 分頁導航按鈕（◀ ▶）置中顯示於 NanmaoLabelPOC/Views/LabelPrintView.xaml
- [x] T015 [US1] 套用 ActionButtonStyle 至資料管理工具列按鈕（匯入、新增、刪除、儲存）於 NanmaoLabelPOC/Views/DataManageView.xaml
- [x] T016 [US1] 套用 ActionButtonStyle 至標籤列印按鈕（輸出 PDF、批次輸出全部）於 NanmaoLabelPOC/Views/LabelPrintView.xaml
- [x] T017 [US1] 套用 SecondaryButtonStyle 至分頁導航按鈕（◀ ▶）於 NanmaoLabelPOC/Views/LabelPrintView.xaml

**Checkpoint**: User Story 1 完成 - 按鈕位置分區佈局就緒，可獨立驗證

---

## Phase 4: User Story 2 - 操作員識別按鈕可用狀態 (Priority: P1)

**Goal**: 實作按鈕狀態視覺回饋（Hover、Active、Focus、Disabled）

**Independent Test**: 啟動應用程式，測試各按鈕的 Hover（背景變亮）、按下（背景變深）、Focus（白色外框）、Disabled（灰色）狀態

**Ref**: FR-007, FR-008, FR-009, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-021

### Implementation for User Story 2

- [x] T018 [US2] 為 ActionButtonStyle 新增 IsMouseOver Trigger（背景變 #2E4A6F）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T019 [US2] 為 ActionButtonStyle 新增 IsPressed Trigger（背景變 #0E2A4F）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T020 [US2] 為 ActionButtonStyle 新增 IsFocused Trigger（白色外框 2px）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T021 [US2] 為 ActionButtonStyle 新增 IsEnabled=False Trigger（灰底灰字、游標 Arrow）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T022 [US2] 調整 Trigger 順序確保狀態優先序正確（Disabled 最後定義 = 最高優先）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T023 [US2] 為 SecondaryButtonStyle 新增 IsMouseOver Trigger（背景變 #F0F0F0）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T024 [US2] 為 SecondaryButtonStyle 新增 IsEnabled=False Trigger（灰色系）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T025 [US2] 建立 TabHeaderStyle 頁簽樣式（選中=深藍底白字, 未選中=淡色）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T026 [US2] 為 TabHeaderStyle 新增 IsMouseOver Trigger（視覺回饋）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T027 [US2] 套用 TabHeaderStyle 至 MainWindow.xaml 頁簽於 NanmaoLabelPOC/Views/MainWindow.xaml

**Checkpoint**: User Story 2 完成 - 所有按鈕狀態視覺回饋正常，可獨立驗證 ✅

### Hotfix: 強化 Hover 視覺回饋

- [x] T018-HF1 [US2] 調整 ButtonHoverBackground 顏色從 #2E4A6F 至 #3D5A80（提高對比度）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T018-HF2 [US2] 為 ActionButtonStyle 新增 Hover 時 DropShadow 效果（ShadowDepth=2, BlurRadius=6）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T018-HF3 [US2] 為 ActionButtonStyle 新增 Hover 時 ScaleTransform (1.02x) 微幅放大效果於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T018-HF4 [US2] 為 SecondaryButtonStyle 新增 Hover 時 DropShadow 和 ScaleTransform 效果於 NanmaoLabelPOC/Resources/ButtonStyles.xaml

**Hotfix 完成** - Hover 視覺回饋已強化：顏色對比度提升 + 陰影效果 + 微幅放大 ✅

---

## Phase 5: User Story 3 - 操作員避免誤刪資料 (Priority: P2)

**Goal**: 實作刪除按鈕二次確認保護機制

**Independent Test**: 選取資料後點擊刪除按鈕，確認顯示確認對話框；點擊取消後資料不變

**Ref**: FR-019, FR-020

### Implementation for User Story 3

- [x] T028 [US3] 修改 DataManageViewModel.cs 新增 ShowDeleteConfirmation 方法（MessageBox 確認對話框）於 NanmaoLabelPOC/ViewModels/DataManageViewModel.cs
- [x] T029 [US3] 修改 DeleteCommand 執行邏輯，加入二次確認呼叫於 NanmaoLabelPOC/ViewModels/DataManageViewModel.cs
- [x] T030 [US3] 確認對話框訊息使用繁體中文（「確定要刪除選取的資料嗎？此操作無法復原。」）於 NanmaoLabelPOC/ViewModels/DataManageViewModel.cs

**Checkpoint**: User Story 3 完成 - 刪除保護機制正常，可獨立驗證 ✅

---

## Phase 6: User Story 4 - 操作員獲得操作回饋 (Priority: P2)

**Goal**: 實作儲存按鈕脈動光暈與 Loading 狀態

**Independent Test**: 修改資料後檢查儲存按鈕是否有脈動效果；執行輸出 PDF 時按鈕是否顯示 Loading 並禁止重複點擊

**Ref**: FR-016, FR-017, FR-018

### Implementation for User Story 4

- [x] T031 [US4] 修改 DataManageViewModel.cs 新增 IsDirty 屬性（追蹤未儲存變更）於 NanmaoLabelPOC/ViewModels/DataManageViewModel.cs
- [x] T032 [US4] 在資料編輯時設定 IsDirty=true，儲存後設定 IsDirty=false 於 NanmaoLabelPOC/ViewModels/DataManageViewModel.cs
- [x] T033 [US4] 建立 PulseActionButtonStyle 樣式（含 DropShadowEffect 脈動 Storyboard）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T034 [US4] 為 PulseActionButtonStyle 新增 DataTrigger（Binding IsDirty=True 時啟動脈動）於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [x] T035 [US4] 套用 PulseActionButtonStyle 至儲存按鈕於 NanmaoLabelPOC/Views/DataManageView.xaml
- [x] T036 [US4] 修改 LabelPrintViewModel.cs 新增 IsExporting 屬性於 NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs
- [x] T037 [US4] 在輸出 PDF 開始時設定 IsExporting=true，完成後設定 IsExporting=false 於 NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs
- [x] T038 [US4] 綁定輸出 PDF 按鈕 IsEnabled 至 !IsExporting 於 NanmaoLabelPOC/Views/LabelPrintView.xaml

**Checkpoint**: User Story 4 完成 - 操作回饋（脈動、Loading）正常，可獨立驗證 ✅

---

## Phase 7: User Story 5 - 操作員透過鍵盤操作 (Priority: P3)

**Goal**: 確保鍵盤 Focus 狀態有清晰的白色外框指示

**Independent Test**: 使用 Tab 鍵導航，確認各按鈕 Focus 時顯示白色外框

**Ref**: FR-014

### Implementation for User Story 5

- [ ] T039 [US5] 確認 ActionButtonStyle 的 IsFocused Trigger 正確顯示白色外框於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [ ] T040 [US5] 確認 SecondaryButtonStyle 的 IsFocused Trigger 正確顯示白色外框於 NanmaoLabelPOC/Resources/ButtonStyles.xaml
- [ ] T041 [US5] 確認 TabHeaderStyle 的 IsFocused Trigger 正確顯示焦點指示於 NanmaoLabelPOC/Resources/ButtonStyles.xaml

**Checkpoint**: User Story 5 完成 - 鍵盤 Focus 指示正常，可獨立驗證

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 測試、驗證、收尾工作

- [ ] T042 [P] 新增 ButtonStateTests.cs 測試狀態優先序邏輯於 NanmaoLabelPOC.Tests/ViewModels/ButtonStateTests.cs
- [ ] T043 執行完整測試套件確認無回歸於 NanmaoLabelPOC.Tests/
- [ ] T044 [P] 執行手動視覺驗證檢查清單（對照 spec.md Success Criteria）
- [ ] T045 確認所有顏色值與 raw_delta_button.md 一致（程式碼審查）
- [ ] T046 確認建置無警告（Constitution I 要求）
- [ ] T047 執行 quickstart.md 驗證步驟

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup)
    │
    ▼
Phase 2 (Foundational) ─── BLOCKS ALL USER STORIES
    │
    ├──────────────────────────────────────────────────────┐
    ▼                                                      ▼
Phase 3 (US1: 位置分區) ◀──────┐              Phase 4 (US2: 狀態回饋) ◀───┐
    │                          │                   │                      │
    │ (可並行)                  │                   │ (可並行)             │
    ▼                          │                   ▼                      │
Phase 5 (US3: 刪除保護)        │              Phase 6 (US4: 操作回饋)    │
    │                          │                   │                      │
    └──────────────────────────┼───────────────────┘                      │
                               │                                          │
                               ▼                                          │
                        Phase 7 (US5: 鍵盤 Focus) ◀───────────────────────┘
                               │
                               ▼
                        Phase 8 (Polish)
```

### User Story Dependencies

| User Story | 可開始條件 | 與其他 Story 關係 |
|------------|-----------|-------------------|
| US1 (P1) | Phase 2 完成 | 獨立，無依賴 |
| US2 (P1) | Phase 2 完成 | 獨立，無依賴 |
| US3 (P2) | Phase 2 完成 | 獨立，無依賴 |
| US4 (P2) | Phase 2 完成 | 獨立，無依賴 |
| US5 (P3) | Phase 2 完成 | 依賴 US2（需先有 Focus Trigger） |

### Parallel Opportunities

**Phase 2 內可並行**:
- T004 (Action 顏色) + T005 (Disabled 顏色) + T006 (Secondary 顏色) + T007 (Feedback 顏色)

**User Stories 可並行** (Phase 2 完成後):
- US1 + US2 + US3 + US4 可同時開始（不同檔案）

**Phase 8 內可並行**:
- T042 (單元測試) + T044 (視覺驗證)

---

## Parallel Example: Phase 2 Foundational

```bash
# Launch all color token tasks together:
Task: "T004 定義 Action 按鈕顏色 Token"
Task: "T005 定義 Disabled 按鈕顏色 Token"
Task: "T006 定義 Secondary 按鈕顏色 Token"
Task: "T007 定義 Feedback 顏色 Token"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T009)
3. Complete Phase 3: User Story 1 - 位置分區 (T010-T017)
4. Complete Phase 4: User Story 2 - 狀態回饋 (T018-T027)
5. **STOP and VALIDATE**: 按鈕樣式與位置已可用
6. 可 Demo 給 Stakeholder 確認視覺效果

### Incremental Delivery

1. Setup + Foundational → 樣式基礎就緒
2. Add US1 (位置) → 工具列佈局完成 → Demo
3. Add US2 (狀態) → Hover/Disabled 效果完成 → Demo
4. Add US3 (刪除保護) → 安全機制完成 → Demo
5. Add US4 (操作回饋) → 脈動/Loading 效果完成 → Demo
6. Add US5 (Focus) → 鍵盤操作完成 → Demo
7. Polish → 測試與驗證 → 合併主線

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- 視覺驗證以手動為主，參照 spec.md Success Criteria
- 所有顏色值來自 raw_delta_button.md，禁止自行調整
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
