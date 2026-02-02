# Tasks: QW075551-2 物料標籤渲染

**Input**: Design documents from `/specs/007-delta-label-qw075551-2/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: 本功能未明確要求 TDD，但依據 Constitution II. 核心邏輯需單元測試，將包含必要測試任務。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

```text
NanmaoLabelPOC/
├── Templates/BuiltInTemplates.cs   # 主要修改
├── Services/LabelRenderer.cs       # 可能需微調
└── Models/LabelField.cs            # 無修改

NanmaoLabelPOC.Tests/
└── Services/LabelRendererTests.cs  # 新增測試
```

---

## Phase 1: Setup (無額外設定)

**Purpose**: 本功能為既有專案修改，無需額外 Setup

**Checkpoint**: 專案已可編譯，直接進入 Phase 2

---

## Phase 2: Foundational (標籤模板基礎結構)

**Purpose**: 重寫 QW075551-2 模板的基礎結構，MUST 完成後才能實作各 User Story

**⚠️ CRITICAL**: 此階段建立模板框架，後續 User Story 在此基礎上增加欄位

- [ ] T001 更新 LabelTemplate 基本屬性：Code="QW075551-2", Name="物料標籤", WidthMm=100, HeightMm=80, HasBorder=true in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-001, FR-002, FR-010]
- [ ] T002 移除現有 QW075551-2 所有欄位定義，清空 Fields 集合 in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-011]
- [ ] T003 新增標題欄位 "物料標籤" (14pt Bold, 置中) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-002]

**Checkpoint**: 模板框架完成（僅標題，無資料欄位），可執行但 PDF 只顯示標題

---

## Phase 3: User Story 1 - 匯出 QW075551-2 物料標籤 PDF (Priority: P1) 🎯 MVP

**Goal**: 使用者選擇 QW075551-2 模板可產生 100mm × 80mm 的標籤 PDF，包含所有六個欄位

**Independent Test**: 匯入測試 Excel，選擇 QW075551-2 模板，輸出 PDF 並檢查尺寸與標題

### Implementation for User Story 1

- [ ] T004 [P] [US1] 新增第一列左欄欄位：單號標籤 (8pt) + 單號值 (8pt) + 單號大字 (11pt Bold) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005]
- [ ] T005 [P] [US1] 新增第一列右欄欄位：代碼標籤 (8pt) + 代碼值 (8pt, 固定 "17008") + 代碼大字 (11pt Bold, 固定 "17008") in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005, FR-006]
- [ ] T006 [P] [US1] 新增第二列左欄欄位：ERP料號標籤 (8pt) + ERP料號值 (8pt) + ERP料號大字 (11pt Bold) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005, FR-011]
- [ ] T007 [P] [US1] 新增第二列右欄欄位：規格型號標籤 (8pt) + 規格型號值 (8pt) + 規格型號大字 (11pt Bold) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005]
- [ ] T008 [P] [US1] 新增第三列左欄欄位：數量標籤 (8pt) + 數量值 (8pt, UseDisplayValue=true) + 數量大字 (11pt Bold, UseDisplayValue=true) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005, FR-007]
- [ ] T009 [P] [US1] 新增第三列右欄欄位：D/C標籤 (8pt) + D/C值 (8pt) + D/C大字 (11pt Bold) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004, FR-005]
- [ ] T010 [US1] 調整所有座標：左欄 X=5mm 起始，右欄 X=55mm 起始，列間距約 18mm in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-003]
- [ ] T011 [US1] PDF 輸出驗證：手動測試產生 PDF，確認尺寸 100mm × 80mm、標題「物料標籤」、無條碼/QR Code [SC-002, SC-006, SC-007]

**Checkpoint**: User Story 1 完成，可產生完整的物料標籤 PDF，包含所有六個欄位的雙行顯示

---

## Phase 4: User Story 2 - 欄位雙行顯示格式 (Priority: P1)

**Goal**: 每個欄位正確呈現雙行格式：小字行「標籤:值」+ 大字行「純值 Bold」

**Independent Test**: 檢查 PDF 中每個欄位是否有兩行顯示

### Implementation for User Story 2

- [ ] T012 [US2] 驗證小字行欄位名稱前綴正確：單號:、代碼:、ERP料號:、規格型號:、數量:、D/C (LOT NO. ): in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-005]
- [ ] T013 [US2] 驗證小字行字型 8pt、大字行字型 11pt Bold 設定正確 in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004]
- [ ] T014 [US2] 調整小字行與大字行垂直間距 (Y 座標差 5mm) in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-004]
- [ ] T015 [US2] PDF 輸出驗證：確認六個欄位皆顯示雙行格式 [SC-003]

**Checkpoint**: User Story 2 完成，所有欄位正確顯示雙行格式

---

## Phase 5: User Story 3 - 長文字縮小字體處理 (Priority: P2)

**Goal**: 長文字欄位自動縮小字體，最小 6pt，仍溢出則換行

**Independent Test**: 以超長測試字串產生 PDF，驗證文字完整顯示

### Implementation for User Story 3

- [ ] T016 [P] [US3] 設定所有動態欄位 AutoShrinkFont=true in `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` [FR-008]
- [ ] T017 [P] [US3] 確認 MinFontSize 預設值為 6pt（LabelField 模型已支援）in `NanmaoLabelPOC/Models/LabelField.cs` [FR-008, Clarification]
- [ ] T018 [US3] 驗證現有 LabelRenderer.CalculateFontSize() 支援換行標記 RequiresWrap in `NanmaoLabelPOC/Services/LabelRenderer.cs` [FR-008]
- [ ] T019 [US3] PDF 輸出驗證：以 50+ 字元測試字串確認無截斷 [SC-005]

**Checkpoint**: User Story 3 完成，長文字正確處理

---

## Phase 6: User Story 4 - 空值欄位處理 (Priority: P2)

**Goal**: 空值欄位保留版面位置，顯示空字串

**Independent Test**: 以含空值的測試資料產生 PDF，驗證版面不變

### Implementation for User Story 4

- [ ] T020 [US4] 驗證空值欄位渲染行為：LabelRenderer 已支援空字串輸出（無需修改）in `NanmaoLabelPOC/Services/LabelRenderer.cs` [FR-009]
- [ ] T021 [US4] PDF 輸出驗證：以空值測試資料確認版面位置不變 [FR-009]

**Checkpoint**: User Story 4 完成，空值處理正確

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 測試、文件與最終驗證

- [ ] T022 [P] 新增單元測試：QW075551-2 模板欄位數量驗證 (19 個 LabelField) in `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`
- [ ] T023 [P] 新增單元測試：標籤尺寸驗證 (100mm × 80mm) in `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`
- [ ] T024 [P] 新增單元測試：無 Barcode/QRCode 欄位驗證 in `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`
- [ ] T025 [P] 新增單元測試：CSQTY 千分位格式驗證 in `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`
- [ ] T026 執行 quickstart.md 驗證流程，確認所有測試通過
- [ ] T027 更新 BuiltInTemplates.cs 方法註解，標註 Delta Spec 參考 [ref: raw_delta_label_QW075551-2.md]

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: N/A - 無額外設定需求
- **Phase 2 (Foundational)**: 必須先完成 - BLOCKS 所有 User Stories
- **Phase 3-6 (User Stories)**: 依序執行，但 P1 Stories 可合併處理
- **Phase 7 (Polish)**: 所有 User Stories 完成後執行

### User Story Dependencies

- **User Story 1 (P1)**: 依賴 Phase 2 完成，建立所有欄位
- **User Story 2 (P1)**: 依賴 US1 欄位已建立，調整雙行格式
- **User Story 3 (P2)**: 依賴 US1/US2 完成，設定 AutoShrinkFont
- **User Story 4 (P2)**: 依賴 US1 完成，驗證空值行為

### Within Each Phase

- Tasks marked [P] can run in parallel (different code sections)
- T004-T009 can all run in parallel (different欄位)
- T022-T025 can all run in parallel (different test methods)

### Parallel Opportunities

```bash
# Phase 3 欄位新增可並行：
T004 (單號) | T005 (代碼) | T006 (ERP料號) | T007 (規格型號) | T008 (數量) | T009 (D/C)

# Phase 7 測試可並行：
T022 (欄位數量) | T023 (尺寸) | T024 (無條碼) | T025 (千分位)
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 2: Foundational (標題 + 框架)
2. Complete Phase 3: User Story 1 (所有欄位)
3. Complete Phase 4: User Story 2 (雙行格式調整)
4. **STOP and VALIDATE**: 測試 PDF 輸出符合規格
5. 可先行發布 MVP

### Incremental Delivery

1. Phase 2 + Phase 3 → MVP (基本標籤輸出)
2. Add Phase 4 → 雙行格式完善
3. Add Phase 5 → 長文字處理
4. Add Phase 6 → 空值處理
5. Add Phase 7 → 測試與文件

---

## Summary

| 統計項目 | 數量 |
|----------|------|
| 總任務數 | 27 |
| Phase 2 (Foundational) | 3 |
| Phase 3 (US1) | 8 |
| Phase 4 (US2) | 4 |
| Phase 5 (US3) | 4 |
| Phase 6 (US4) | 2 |
| Phase 7 (Polish) | 6 |
| 可並行任務 [P] | 16 |

### MVP Scope

- **最小可行產品**: Phase 2 + Phase 3 (11 tasks)
- **完整功能**: Phase 2 ~ Phase 6 (21 tasks)
- **含測試與文件**: 全部 Phase (27 tasks)
