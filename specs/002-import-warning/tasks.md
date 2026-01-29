# Tasks: Excel 匯入警告分級

**Input**: Design documents from `/specs/002-import-warning/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: 依據憲章 II「測試標準」，核心業務邏輯需有對應單元測試。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **主程式**: `NanmaoLabelPOC/`
- **測試專案**: `NanmaoLabelPOC.Tests/`

---

## Phase 1: Setup (基礎設施)

**Purpose**: 新增 MessageSeverity 列舉與 ImportMessage 類別

- [X] T001 [P] 新增 MessageSeverity 列舉至 NanmaoLabelPOC/Services/IExcelImporter.cs（FR-003）
- [X] T002 [P] 新增 ImportMessage 類別至 NanmaoLabelPOC/Services/IExcelImporter.cs（FR-003, FR-009）
- [X] T003 擴充 ImportResult 類別：新增 Messages 屬性與計算屬性（ErrorCount、WarningCount、InfoCount、HasErrors、HasWarnings、HasInfos）至 NanmaoLabelPOC/Services/IExcelImporter.cs（FR-007, FR-008）
- [X] T004 標記 ImportResult.Warnings 為 [Obsolete] 並實作向後相容邏輯至 NanmaoLabelPOC/Services/IExcelImporter.cs

**Checkpoint**: 資料模型擴充完成，可開始實作 User Story

---

## Phase 2: User Story 1 - 匯入含底線欄位的 Excel 檔案 (Priority: P1) 🎯 MVP

**Goal**: 修正欄位名稱驗證規則，允許底線字元，解決 `nvr_cust` 等欄位被誤判為非法的問題

**Independent Test**: 匯入包含 `nvr_cust` 欄位的 Excel 檔案，確認無「非法字元」警告

### Tests for User Story 1

- [X] T005 [P] [US1] 新增測試 FieldNameValidation_WithUnderscore_ShouldBeValid 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-001）
- [X] T006 [P] [US1] 新增測試 FieldNameValidation_WithSpecialChars_ShouldBeInvalid 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-002）

### Implementation for User Story 1

- [X] T007 [US1] 修正 FieldNamePattern() 正規表達式從 `^[A-Za-z0-9]+$` 改為 `^[A-Za-z0-9_]+$` 至 NanmaoLabelPOC/Services/ExcelImporter.cs:34（FR-001）
- [X] T008 [US1] 更新 BuildColumnMapping() 中的警告訊息，含底線欄位不再產生警告至 NanmaoLabelPOC/Services/ExcelImporter.cs:140-170

**Checkpoint**: 底線欄位可正常匯入，US1 可獨立測試驗證

---

## Phase 3: User Story 2 - 依等級顯示匯入訊息 (Priority: P1)

**Goal**: 將所有匯入訊息分級為 Error/Warning/Info，Error 中斷匯入、Warning 允許繼續、Info 預設隱藏

**Independent Test**: 匯入包含各類問題的 Excel 檔案，確認訊息正確分級

### Tests for User Story 2

- [X] T009 [P] [US2] 新增測試 Import_FileNotExists_ShouldReturnErrorSeverity 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-004）
- [X] T010 [P] [US2] 新增測試 Import_MissingRequiredField_ShouldReturnWarningSeverity 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-005）
- [X] T011 [P] [US2] 新增測試 Import_ExtraField_ShouldReturnInfoSeverity 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-006）

### Implementation for User Story 2

- [X] T012 [US2] 重構 Import() 方法：檔案不存在時新增 Error 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:60-66（FR-004, FR-009）
- [X] T013 [US2] 重構 Import() 方法：檔案格式錯誤時新增 Error 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:68-74（FR-004, FR-009）
- [X] T014 [US2] 重構 BuildColumnMapping()：額外欄位被忽略時新增 Info 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:140-170（FR-006, FR-009）
- [X] T015 [US2] 重構 BuildColumnMapping()：非法欄位名稱時新增 Info 等級 ImportMessage（非底線字元）至 NanmaoLabelPOC/Services/ExcelImporter.cs:140-170（FR-006, FR-009）
- [X] T016 [US2] 重構 Import() 迴圈：空白列略過時新增 Info 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:105-118（FR-006, FR-009）
- [X] T017 [US2] 重構 GetQuantityFieldValue()：千分位格式時新增 Warning 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:245-269（FR-005, FR-009, FR-010）
- [X] T018 [US2] 重構 CheckSemicolonWarnings()：含分號時新增 Warning 等級 ImportMessage 至 NanmaoLabelPOC/Services/ExcelImporter.cs:345-364（FR-005, FR-009）

**Checkpoint**: 所有訊息皆有正確分級，US2 可獨立測試驗證

---

## Phase 4: User Story 3 - 檢視匯入結果摘要 (Priority: P2)

**Goal**: 匯入結果顯示清晰摘要，包含匯入筆數、各等級訊息數量，Info 訊息預設收合

**Independent Test**: 匯入檔案後檢視結果對話框，確認摘要格式與 Expander 行為

### Implementation for User Story 3

- [X] T019 [P] [US3] 建立 ImportResultDialog.xaml 自訂對話框至 NanmaoLabelPOC/Views/ImportResultDialog.xaml（FR-006, FR-007, FR-008）
- [X] T020 [P] [US3] 建立 ImportResultDialog.xaml.cs code-behind 至 NanmaoLabelPOC/Views/ImportResultDialog.xaml.cs
- [X] T021 [US3] 實作 ImportResultDialog 內容：匯入筆數、Error 區段、Warning 區段至 NanmaoLabelPOC/Views/ImportResultDialog.xaml（FR-007）
- [X] T022 [US3] 實作 ImportResultDialog Info 區段：使用 Expander 預設收合至 NanmaoLabelPOC/Views/ImportResultDialog.xaml（FR-006, FR-008）
- [X] T023 [US3] 實作大量訊息摘要：Info 超過 10 條時顯示「共 N 條資訊」至 NanmaoLabelPOC/Views/ImportResultDialog.xaml.cs
- [X] T024 [US3] 修改 DataManageView.xaml.cs ImportButton_Click()：改用 ImportResultDialog 顯示結果至 NanmaoLabelPOC/Views/DataManageView.xaml.cs:155-229

**Checkpoint**: 匯入結果以結構化對話框顯示，Info 可展開，US3 可獨立測試驗證

---

## Phase 5: User Story 4 - 含分號欄位值警告 (Priority: P3)

**Goal**: 欄位值含分號時顯示 Warning 提醒可能影響 QR Code（此功能已在 US2 中實作，本階段為整合驗證）

**Independent Test**: 匯入含分號欄位值的 Excel，確認顯示正確 Warning

### Tests for User Story 4

- [X] T025 [P] [US4] 新增測試 Import_SemicolonInValue_ShouldReturnWarningSeverity 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs（FR-005, FR-009）

### Implementation for User Story 4

- [X] T026 [US4] 驗證 CheckSemicolonWarnings() 訊息格式符合規格：「欄位 'pono' 值 'ABC;123' 包含分號，可能影響 QR Code」至 NanmaoLabelPOC/Services/ExcelImporter.cs:345-364

**Checkpoint**: 分號警告功能完整，US4 可獨立測試驗證

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 清理、文件更新、回歸測試

- [ ] T027 [P] 更新 ExcelImporterTests.cs 中所有使用 Warnings 的測試改用 Messages 至 NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs
- [ ] T028 [P] 移除 ExcelImporter.cs 中的舊式 result.Warnings.Add() 呼叫（改用 AddMessage helper）至 NanmaoLabelPOC/Services/ExcelImporter.cs
- [ ] T029 執行完整測試套件確認回歸：`dotnet test NanmaoLabelPOC.Tests`
- [ ] T030 執行 quickstart.md 驗證流程確認功能正常

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: 無依賴，可立即開始
- **Phase 2 (US1)**: 依賴 Phase 1 完成
- **Phase 3 (US2)**: 依賴 Phase 1 完成（與 US1 可並行）
- **Phase 4 (US3)**: 依賴 Phase 1、Phase 3 完成（需使用 Messages 屬性）
- **Phase 5 (US4)**: 依賴 Phase 3 完成（CheckSemicolonWarnings 需改用 Messages）
- **Phase 6 (Polish)**: 依賴所有 User Story 完成

### User Story Dependencies

- **US1 (P1)**: 獨立可測試 - 僅修正正規表達式
- **US2 (P1)**: 獨立可測試 - 建立訊息分級基礎設施
- **US3 (P2)**: 依賴 US2（需使用 Messages 屬性顯示分級訊息）
- **US4 (P3)**: 依賴 US2（整合驗證分號警告分級）

### Parallel Opportunities

Setup Phase:
```
T001 (MessageSeverity) ─┬─ T003 (ImportResult 擴充)
T002 (ImportMessage)   ─┘
```

US1 Tests:
```
T005 (底線合法測試) ─┬─ T007 (正規表達式修正)
T006 (特殊字元測試) ─┘
```

US2 Tests:
```
T009 (Error 測試)   ─┬─ T012~T018 (訊息分級實作)
T010 (Warning 測試) ─┤
T011 (Info 測試)    ─┘
```

US3 Dialog:
```
T019 (XAML)    ─┬─ T021~T024 (內容實作)
T020 (Code-behind) ─┘
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 1: Setup（資料模型）
2. Complete Phase 2: US1（底線欄位修正）
3. Complete Phase 3: US2（訊息分級）
4. **STOP and VALIDATE**: 測試 US1、US2 獨立功能
5. 可先行部署，解決核心問題

### Incremental Delivery

1. Setup + US1 → 底線欄位可匯入 ✅
2. + US2 → 訊息有分級 ✅
3. + US3 → 結果對話框美化 ✅
4. + US4 → 分號警告整合驗證 ✅

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- FR-xxx 標註對應 spec.md Functional Requirements
- 所有訊息使用繁體中文
- 向後相容性：保留 Warnings 屬性（標記 Obsolete）
- 驗收以 PDF 輸出為最終標準（本功能不影響 PDF）
