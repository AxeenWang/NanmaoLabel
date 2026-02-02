# Tasks: QW075551-1 出貨標籤 Delta Spec 實作

**Input**: Design documents from `/specs/006-delta-label-qw075551-1/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md, quickstart.md

**Tests**: 本功能規格中有定義測試案例（Constitution II 測試標準），故包含測試任務。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **主程式**: `NanmaoLabelPOC/`
- **測試專案**: `NanmaoLabelPOC.Tests/`

---

## Phase 1: Setup (模型擴充)

**Purpose**: 擴充 LabelField 模型以支援長文字處理功能

- [x] T001 [P] 擴充 LabelField 模型，新增 MinFontSize 屬性 (預設 6) 於 NanmaoLabelPOC/Models/LabelField.cs [FR-008]
- [x] T002 [P] 擴充 LabelField 模型，新增 AutoShrinkFont 屬性 (預設 false) 於 NanmaoLabelPOC/Models/LabelField.cs [FR-008]
- [x] T003 [P] 擴充 RenderCommand 類別，新增 ActualFontSize 屬性於 NanmaoLabelPOC/Services/ILabelRenderer.cs [FR-008]
- [x] T004 [P] 擴充 RenderCommand 類別，新增 RequiresWrap 屬性於 NanmaoLabelPOC/Services/ILabelRenderer.cs [FR-008]

---

## Phase 2: Foundational (基礎渲染邏輯)

**Purpose**: 實作日期格式轉換與長文字縮小邏輯，這些是所有 User Story 的前置條件

**⚠️ CRITICAL**: User Story 實作需依賴此階段的渲染邏輯變更

- [ ] T005 實作日期格式轉換邏輯 (yyyy-MM-dd → yyyy/MM/dd) 於 LabelRenderer.ResolveContent() 方法 NanmaoLabelPOC/Services/LabelRenderer.cs [FR-006]
- [ ] T006 實作長文字縮小字體計算邏輯 (最小 6pt) 於 LabelRenderer.RenderField() 方法 NanmaoLabelPOC/Services/LabelRenderer.cs [FR-008]
- [ ] T007 實作標籤外框渲染邏輯 (單線矩形邊框，0.5pt) 於 PdfExporter.CreateDocument() 方法 NanmaoLabelPOC/Services/PdfExporter.cs [FR-003]
- [ ] T008 實作長文字縮小渲染邏輯 (根據 ActualFontSize) 於 PdfExporter.RenderText() 方法 NanmaoLabelPOC/Services/PdfExporter.cs [FR-008]

**Checkpoint**: 渲染基礎邏輯就緒，可開始 User Story 實作

---

## Phase 3: User Story 1 - 渲染符合新規格的 QW075551-1 標籤 (Priority: P1) 🎯 MVP

**Goal**: 更新 QW075551-1 模板定義，包含正確的尺寸、欄位配置與座標

**Independent Test**: 匯入測試資料後預覽標籤，視覺呈現應符合參考圖片

### Tests for User Story 1

- [ ] T009 [P] [US1] 新增測試 Render_QW075551_1_DateFormat_ReturnsSlashFormat 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-006]
- [ ] T010 [P] [US1] 新增測試 Render_QW075551_1_CSCUSTPN_ReturnsText 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-005, FR-007]

### Implementation for User Story 1

- [ ] T011 [US1] 更新 QW075551-1 模板 HeightMm 由 60 改為 80 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-001]
- [ ] T012 [US1] 更新 CSCUSTPN 欄位 FieldType 由 Barcode 改為 Text 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-005, FR-007]
- [ ] T013 [US1] 更新 CSCUSTPN 欄位座標 (Y=41, Height=5) 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-002]
- [ ] T014 [US1] 更新 CustomerLabel 為雙行顯示 ("Customer\n客戶名稱", Height=8) 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-019]
- [ ] T015 [US1] 更新 ProductNoLabel 為雙行顯示 ("Product NO.\n產品型號", Height=8) 並修正 typo 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-004, FR-019]
- [ ] T016 [US1] 為所有變數 Text 欄位設定 AutoShrinkFont = true 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-008]

**Checkpoint**: User Story 1 完成 - 基本標籤渲染正確（尺寸、欄位類型、欄位標籤）

---

## Phase 4: User Story 2 - QR Code 正確編碼與呈現 (Priority: P1)

**Goal**: 更新 QR Code 位置至左下角，確保編碼內容與千分位處理正確

**Independent Test**: 掃描 QR Code 驗證內容格式正確

### Tests for User Story 2

- [ ] T017 [P] [US2] 新增測試 Render_QW075551_1_QRCode_Position 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-012]
- [ ] T018 [P] [US2] 新增測試 Render_QW075551_1_QRCode_Content 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-009, FR-010]

### Implementation for User Story 2

- [ ] T019 [US2] 更新 QRCODE 欄位座標 (X=5, Y=55) 移至左下角於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-012]
- [ ] T020 [US2] 確認 QRCODE CombinePattern 使用 Raw Value (無千分位) 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-009, FR-010]

**Checkpoint**: User Story 2 完成 - QR Code 位置與編碼內容正確

---

## Phase 5: User Story 3 - 版面配置符合新規格 (Priority: P2)

**Goal**: 實作 Remarks 區段版面配置（QR Code 與文字並排）

**Independent Test**: 預覽標籤並與參考圖片對比

### Implementation for User Story 3

- [ ] T021 [US3] 新增 RemarksLabel 欄位 ("Remarks", X=5, Y=50) 於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-013]
- [ ] T022 [US3] 移除 MoLabel, DeviceLabel, RemarkLabel 獨立標籤於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-013]
- [ ] T023 [US3] 更新 CSMO 座標 (X=28, Y=55, Width=67) 至 QR Code 右側於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-014]
- [ ] T024 [US3] 更新 OUTDEVICENO 座標 (X=28, Y=60, Width=67) 至 QR Code 右側於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-014]
- [ ] T025 [US3] 更新 CSREMARK 座標 (X=28, Y=65, Width=67) 至 QR Code 右側於 NanmaoLabelPOC/Templates/BuiltInTemplates.cs [FR-014]

**Checkpoint**: User Story 3 完成 - Remarks 區段版面正確

---

## Phase 6: User Story 4 - PDF 輸出符合新規格 (Priority: P2)

**Goal**: 確保 PDF 輸出頁面尺寸與內容符合新規格

**Independent Test**: 輸出 PDF 並量測頁面尺寸

### Tests for User Story 4

- [ ] T026 [P] [US4] 新增測試 Export_QW075551_1_PageSize_100x80mm 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-001, FR-017]
- [ ] T027 [P] [US4] 新增測試 Export_QW075551_1_HasBorder_NoSeparator 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-003]

### Implementation for User Story 4

- [ ] T028 [US4] 確認 PdfExporter 使用 template.HeightMm 動態設定頁面尺寸於 NanmaoLabelPOC/Services/PdfExporter.cs [FR-017]
- [ ] T029 [US4] 驗證 PDF 輸出無 Code 128 條碼（僅 QR Code）於 NanmaoLabelPOC/Services/PdfExporter.cs [FR-007]

**Checkpoint**: User Story 4 完成 - PDF 輸出符合規格

---

## Phase 7: User Story 5 - 長文字溢出處理 (Priority: P3)

**Goal**: 實作長文字自動縮小字體處理

**Independent Test**: 輸入超長文字資料，驗證字體自動縮小

### Tests for User Story 5

- [ ] T030 [P] [US5] 新增測試 Render_LongText_ShrinkFont_MinSize6pt 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-008]
- [ ] T031 [P] [US5] 新增測試 Render_LongText_ExceedsMinFont_Truncate 於 NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs [FR-008]

### Implementation for User Story 5

- [ ] T032 [US5] 驗證長文字縮小邏輯處理超長文字時正確截斷加省略號於 NanmaoLabelPOC/Services/LabelRenderer.cs [FR-008]
- [ ] T033 [US5] 驗證多個欄位各自獨立縮小處理於 NanmaoLabelPOC/Services/PdfExporter.cs [FR-008]

**Checkpoint**: User Story 5 完成 - 長文字處理正確

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 最終驗證與整合

- [ ] T034 執行 quickstart.md 驗證流程
- [ ] T035 執行所有測試確認通過 (powershell.exe -Command "cd '$(wslpath -w .)'; dotnet test NanmaoLabelPOC.Tests")
- [ ] T036 視覺驗收：輸出 PDF 並與參考圖片對比
- [ ] T037 QR Code 驗收：掃描驗證編碼內容正確

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 無依賴 - 可立即開始
- **Foundational (Phase 2)**: 依賴 Setup 完成 - **阻擋所有 User Story**
- **User Stories (Phase 3-7)**: 依賴 Foundational 完成
  - US1 與 US2 同為 P1，建議 US1 先完成（模板基礎）
  - US3、US4 為 P2，依賴 US1 模板定義
  - US5 為 P3，可獨立開發
- **Polish (Phase 8)**: 依賴所有 User Story 完成

### User Story Dependencies

- **User Story 1 (P1)**: 模板基礎 - 無其他 Story 依賴
- **User Story 2 (P1)**: QR Code - 可與 US1 平行，建議 US1 先完成
- **User Story 3 (P2)**: 版面配置 - 建議在 US1、US2 後
- **User Story 4 (P2)**: PDF 輸出 - 依賴 US1 模板尺寸
- **User Story 5 (P3)**: 長文字處理 - 獨立於其他 Story

### Within Each User Story

- Tests 先寫，確認 FAIL
- 模板定義先於渲染邏輯
- 完成後執行測試確認 PASS

### Parallel Opportunities

- T001-T004 (Setup): 可平行（不同檔案）
- T009-T010, T017-T018, T026-T027, T030-T031 (Tests): 各組可平行
- US3 的 T021-T025 全在同一檔案，需序列執行

---

## Parallel Example: Phase 1 Setup

```bash
# Launch all model extensions together:
Task T001: "擴充 LabelField 模型，新增 MinFontSize 屬性"
Task T002: "擴充 LabelField 模型，新增 AutoShrinkFont 屬性"
Task T003: "擴充 RenderCommand 類別，新增 ActualFontSize 屬性"
Task T004: "擴充 RenderCommand 類別，新增 RequiresWrap 屬性"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. 完成 Phase 1: Setup (模型擴充)
2. 完成 Phase 2: Foundational (渲染邏輯)
3. 完成 Phase 3: User Story 1 (標籤基礎)
4. 完成 Phase 4: User Story 2 (QR Code)
5. **STOP and VALIDATE**: 測試標籤預覽與 QR Code
6. 可部署/展示 MVP

### Incremental Delivery

1. Setup + Foundational → 渲染基礎就緒
2. Add US1 + US2 → 測試 → **MVP 可用**
3. Add US3 → 測試 → 版面完整
4. Add US4 → 測試 → PDF 輸出驗證
5. Add US5 → 測試 → 長文字處理完整

---

## Summary

| 項目 | 數量 |
|------|------|
| 總任務數 | 37 |
| Setup (Phase 1) | 4 |
| Foundational (Phase 2) | 4 |
| User Story 1 (Phase 3) | 8 |
| User Story 2 (Phase 4) | 4 |
| User Story 3 (Phase 5) | 5 |
| User Story 4 (Phase 6) | 4 |
| User Story 5 (Phase 7) | 4 |
| Polish (Phase 8) | 4 |

---

## Notes

- [P] tasks = 不同檔案，無依賴
- [Story] label 對應 spec.md 中的 User Story
- 驗收以 PDF 輸出為唯一依據
- 座標可依實際渲染效果微調
- 參考圖片：`出貨標籤格式_QW075551-1.png`、`出貨標籤預覽_QW075551-1.png`
