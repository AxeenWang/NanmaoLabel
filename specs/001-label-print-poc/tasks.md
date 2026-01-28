# Tasks: 南茂標籤列印 POC

**Reviewer**: Third-party Auditor
**Date**: 2026-01-28 (v1.1)
**Input**: Design documents from `/specs/001-label-print-poc/`
**Authority**: `raw_spec.md` (第 13 章為最高優先規範)

**Tests**: 依據 raw_spec 7.2 與憲章 II，核心模組須有單元測試。

**Organization**: Tasks grouped by user story for independent implementation and testing.

---

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1~US6)
- Include exact file paths in descriptions

## Path Conventions [ref: raw_spec 7.2]

```text
NanmaoLabel/
├── NanmaoLabel.sln
├── NanmaoLabelPOC/                    # 主程式 (WPF)
│   ├── Views/
│   ├── ViewModels/
│   ├── Models/
│   ├── Services/
│   ├── Templates/
│   ├── Converters/
│   └── Resources/
├── NanmaoLabelPOC.Tests/              # 單元測試專案
│   └── Services/
└── samples/
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure [ref: raw_spec 7.2]
**Milestone**: M1 (專案骨架)

- [x] T001 Create solution file `NanmaoLabel.sln` or new style `NanmaoLabel.slnx` at repository root
- [x] T002 [P] Create WPF project `NanmaoLabelPOC/NanmaoLabelPOC.csproj` targeting .NET 8 LTS with WindowStyle="None" [ref: raw_spec 7, 8.3]
- [x] T003 [P] Create test project `NanmaoLabelPOC.Tests/NanmaoLabelPOC.Tests.csproj` with xUnit [ref: raw_spec 7.2, 憲章 II]
- [x] T004 [P] Add NuGet packages: CommunityToolkit.Mvvm (8.x), ExcelDataReader (3.x), ExcelDataReader.DataSet (3.x), ZXing.Net (0.16.x), QuestPDF (2024.x) [ref: raw_spec 7.1]
- [x] T005 [P] Create directory structure: Views/, ViewModels/, Models/, Services/, Templates/, Converters/, Resources/ [ref: raw_spec 7.2]
- [x] T006 Configure QuestPDF Community License in `NanmaoLabelPOC/App.xaml.cs` [ref: raw_spec 7.1]
- [x] T007 [P] Create `samples/MockData.xlsx` with test data matching Appendix A fields [ref: raw_spec 附錄 A]

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure MUST be complete before ANY user story [ref: raw_spec 2.5]
**Milestones**: M2 (資料層), M3 (條碼生成)

### Data Models [ref: raw_spec 附錄 B, 7.2]

- [x] T008 [P] Create `NanmaoLabelPOC/Models/DataRecord.cs` with Id (UUID) + 17 data fields (ogb03, ogb19, ogb092, ogb905, ogd12b, ogd12e, ima902, ogd15, ogd09, obe25, nvr_cust, nvr_cust_item_no, nvr_cust_pn, nvr_remark10, pono, erpmat, cscustpo), GetRawValue/GetDisplayValue methods for Raw/Display Value separation [ref: raw_spec 附錄 B.2, 13.13]
- [x] T009 [P] Create `NanmaoLabelPOC/Models/LabelField.cs` with FieldType enum (Text, Barcode, QRCode), properties: Name, FieldType, DataSource, IsConstant, CombinePattern, X, Y, Width, Height, FontSize, IsBold, Alignment [ref: raw_spec 4.1, 4.2]
- [x] T010 [P] Create `NanmaoLabelPOC/Models/LabelTemplate.cs` with Code, Name, WidthMm (100), HeightMm (60), List<LabelField> Fields [ref: raw_spec 5.1, 5.2]

### Unit Converter [ref: raw_spec 13.1]

- [x] T011 Create `NanmaoLabelPOC/Services/UnitConverter.cs` static class with:
  - MmToPt: `mm × 72 ÷ 25.4` (PDF output)
  - MmToPx300: `mm × 300 ÷ 25.4` (barcode generation)
  - MmToWpf: `mm × 96 ÷ 25.4` (preview)
  [ref: raw_spec 13.1 換算公式]

### Data Layer [ref: raw_spec 2.2, 3.1 F-01~F-02]

- [x] T012 Create interface `NanmaoLabelPOC/Services/IDataStore.cs` with Load(), Save(records), GetAll() [ref: raw_spec 7.2]
- [x] T013 Create `NanmaoLabelPOC/Services/DataStore.cs` implementing IDataStore:
  - Path: `.\data\data.json` [ref: raw_spec 2.3]
  - JSON structure: { version: "1.0", lastModified: ISO 8601, records: [] } [ref: raw_spec 附錄 B.1]
  - Auto-create directory if not exists [ref: raw_spec 2.3]
  - Update lastModified as ISO 8601 on save [ref: raw_spec 13.5]
  - Generate UUID for new records [ref: raw_spec 附錄 B.2]
  - Single Source of Truth [ref: raw_spec 13.25]

- [x] T014 Create interface `NanmaoLabelPOC/Services/IExcelImporter.cs` with Import(filePath) [ref: raw_spec 7.2]
- [x] T015 Create `NanmaoLabelPOC/Services/ExcelImporter.cs` implementing IExcelImporter:
  - Read all cells as String (preserve leading zeros) [ref: raw_spec 3.1 備註]
  - Case-insensitive field matching [ref: raw_spec 3.3, 13.11]
  - Field name validation: alphanumeric only (A-Z, a-z, 0-9), reject underscore/space/special chars [ref: raw_spec 13.11]
  - Trim all values [ref: raw_spec 3.3]
  - Date conversion to yyyy-MM-dd [ref: raw_spec 3.3, 13.2]
  - Semicolon warning for QR Code fields [ref: raw_spec 3.3, 13.4]
  - Quantity validation: digits only, reject thousand separators [ref: raw_spec 13.14]
  - Overwrite mode [ref: raw_spec 13.5]

### Barcode Generation [ref: raw_spec 6.1, 6.2]

- [x] T016 Create interface `NanmaoLabelPOC/Services/IBarcodeGenerator.cs` with GenerateCode128(content), GenerateQRCode(content) [ref: raw_spec 7.2]
- [x] T017 Create `NanmaoLabelPOC/Services/BarcodeGenerator.cs` implementing IBarcodeGenerator:
  - **Code 128** [ref: raw_spec 6.1]:
    - Height: 10mm (118px @300DPI)
    - Quiet Zone: Margin=10 (10× minimum unit width)
    - PureBarcode=true (text rendered separately)
  - **QR Code** [ref: raw_spec 6.2]:
    - Size: 20mm × 20mm (236×236px @300DPI)
    - ErrorCorrection: Level M (15%)
    - CharacterSet: UTF-8 (supports Chinese)
    - Quiet Zone: Margin=4 (4× unit width)
  - **CRITICAL** [ref: raw_spec 13.9]: No ellipsis, no truncation, no scaling on barcode content

### Label Templates [ref: raw_spec 5.1, 5.2]

- [ ] T018 Create `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` static class with:
  - **QW075551-1** (出貨標籤): 17 fields per raw_spec 5.1 coordinates
    - Title, Customer label, CSCUSTOMER, Date label, FINDPRTDC, Q'ty label, CSQTY, Product NO label, CSCUSTITEMNO, CSCUSTPN (Barcode), MO label, CSMO, Device label, OUTDEVICENO, Remark label, CSREMARK, QRCODE
    - QR Code pattern: `{pono};{ima902};{ogd09};{nvr_remark10}`
  - **QW075551-2**: 14 fields per raw_spec 5.2 coordinates
    - Title, Customer PO label, CSCUSTPO, CS Number label, CSNUMBER ("17008" constant), Q'ty label, CSQTY, ERP Part NO label, ERPPARTNO (Barcode), Customer Item label, CSCUSTITEMNO, Remark label, CSREMARK, QRCODE
    - QR Code pattern: `{cscustpo};{erpmat};{ogd09}`
  - All coordinates in mm [ref: raw_spec 13.1]
  - Font specs per raw_spec 5.3

### Unit Tests (Foundational) [ref: raw_spec 7.2, 憲章 II]

- [ ] T019 [P] Create `NanmaoLabelPOC.Tests/Services/DataStoreTests.cs`:
  - Test Load/Save JSON with version field [ref: raw_spec 附錄 B.1]
  - Test lastModified update (ISO 8601)
  - Test UUID generation for new records [ref: raw_spec 附錄 B.2]
  - Test auto-create directory

- [ ] T020 [P] Create `NanmaoLabelPOC.Tests/Services/ExcelImporterTests.cs`:
  - Test case-insensitive field matching
  - Test field name validation (reject underscore, space, special chars) [ref: raw_spec 13.11]
  - Test Trim behavior
  - Test date format conversion
  - Test quantity validation (reject thousand separators) [ref: raw_spec 13.14]
  - Test semicolon warning [ref: raw_spec 13.4]

- [ ] T021 [P] Create `NanmaoLabelPOC.Tests/Services/BarcodeGeneratorTests.cs`:
  - Test Code 128 generation with Quiet Zone [ref: raw_spec 6.1]
  - Test QR Code generation with Level M, UTF-8 [ref: raw_spec 6.2]
  - Test barcode content uses Raw Value only [ref: raw_spec 13.13]

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - 標籤列印與 PDF 輸出 (Priority: P1) 🎯 MVP

**Goal**: 產線操作員選取紀錄 → 即時預覽 → 輸出 PDF → 條碼可被穩定掃描 [ref: spec.md US1]
**Milestone**: M4 (標籤渲染), M5 (PDF 輸出)

**Independent Test**: 選取資料 → 預覽 → 輸出 PDF → 手機/掃描槍首次掃描即成功 [ref: raw_spec 13.24]

### Label Rendering [ref: raw_spec 2.5, 4.2]

- [ ] T022 [US1] Create interface `NanmaoLabelPOC/Services/ILabelRenderer.cs` with Render(template, record) returning render commands [ref: raw_spec 7.2]
- [ ] T023 [US1] Create `NanmaoLabelPOC/Services/LabelRenderer.cs` implementing ILabelRenderer:
  - Variable substitution from DataRecord [ref: raw_spec 4.2]
  - Constant value handling (e.g., "17008") [ref: raw_spec 4.2]
  - **Raw Value for Barcode/QRCode** [ref: raw_spec 13.13]:
    - ogd09: use raw value (no thousand separator)
    - All fields: Trim only, no formatting
  - **Display Value for Text** [ref: raw_spec 13.13]:
    - ogd09: add thousand separator
    - obe25: yyyy-MM-dd format
  - **Text overflow**: Ellipsis + truncate, no wrap, no shrink [ref: raw_spec 13.2]
  - **Barcode/QRCode**: NO ellipsis, NO truncation [ref: raw_spec 13.9]
  - QR Code combine pattern: preserve empty field position (e.g., `A;;C`) [ref: raw_spec 13.4, 13.15]
  - Shared render logic for Preview and PDF [ref: raw_spec 13.1]

### PDF Export [ref: raw_spec 3.1 F-08]

- [ ] T024 [US1] Create interface `NanmaoLabelPOC/Services/IPdfExporter.cs` with ExportSingle(template, record, outputPath) [ref: raw_spec 7.2]
- [ ] T025 [US1] Create `NanmaoLabelPOC/Services/PdfExporter.cs` implementing IPdfExporter:
  - Page size: 100mm × 60mm [ref: raw_spec 5.1]
  - Font embedding: Microsoft JhengHei (Regular + Bold) [ref: raw_spec 5.3, 13.2]
  - Use UnitConverter.MmToPt for coordinates [ref: raw_spec 13.1]
  - Barcode text: 8pt, centered below barcode [ref: raw_spec 6.1]
  - Default filename: `Label_{ogb19}_{yyyyMMdd_HHmmss}.pdf` [ref: raw_spec 13.6]
  - Output directory: `.\output\` (auto-create if not exists) [ref: raw_spec 2.3]
  - **300 DPI rendering** [ref: raw_spec 7, 13.17]

### ViewModel [ref: raw_spec 8.4]

- [ ] T026 [US1] Create `NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs`:
  - Properties: Records, SelectedRecord, SelectedTemplate, PreviewContent
  - Commands: LoadDataCommand, ExportPdfCommand
  - Use CommunityToolkit.Mvvm [ref: raw_spec 7.1]
  - Single-click: update preview only [ref: raw_spec 13.6]
  - Double-click: trigger PDF output once (with debounce 500ms) [ref: raw_spec 8.8, 13.6]

### View [ref: raw_spec 8.4]

- [ ] T027 [US1] Create `NanmaoLabelPOC/Views/LabelPrintView.xaml`:
  - Left: Preview area (Canvas, ratio 100:60, background #F5F5F5) [ref: raw_spec 8.4]
  - Right: ListView (item height 50px, fields: 📦 ogb19 │ nvr_cust │ ogd09) [ref: raw_spec 8.4]
  - Bottom-left: Template dropdown [ref: raw_spec 8.4]
  - Bottom-right: [輸出 PDF] button [ref: raw_spec 8.4]
  - MouseDoubleClick with e.Handled=true + debounce [ref: raw_spec 8.8]
  - Grid layout: 45*:55* columns [ref: raw_spec 8.4]

- [ ] T028 [US1] Implement status bar message `✅ PDF 已輸出：{完整檔案路徑}` [ref: raw_spec 8.8, 13.6]

### Unit Tests (US1) [ref: raw_spec 7.2, 憲章 II]

- [ ] T029 [P] [US1] Create `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`:
  - Test variable substitution [ref: raw_spec TC-02]
  - Test constant value handling [ref: raw_spec TC-03]
  - Test Raw Value vs Display Value separation [ref: raw_spec 13.13]
  - Test QR Code empty field preservation (A;;C format) [ref: raw_spec 13.15]
  - Test text ellipsis [ref: raw_spec 13.2]
  - Test barcode content has no ellipsis [ref: raw_spec 13.9]

**Checkpoint**: US1 完成 - 可驗證條碼掃描成功 [ref: raw_spec TC-08, TC-09, TC-11]

---

## Phase 4: User Story 2 - 標籤格式切換 (Priority: P1)

**Goal**: 操作員可切換 QW075551-1 / QW075551-2 格式 [ref: spec.md US2]
**Milestone**: M4

**Independent Test**: 切換下拉選單 → 預覽區即時更新 [ref: raw_spec TC-05]

- [ ] T030 [US2] Extend LabelPrintViewModel with AvailableTemplates collection (QW075551-1, QW075551-2) in `NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs`
- [ ] T031 [US2] Bind template dropdown to AvailableTemplates in `NanmaoLabelPOC/Views/LabelPrintView.xaml`
- [ ] T032 [US2] Implement immediate preview update on template change (PropertyChanged)

**Checkpoint**: US2 完成 - 兩種格式皆可正確顯示 [ref: raw_spec A-05]

---

## Phase 5: User Story 6 - 自動載入與程式啟動 (Priority: P1)

**Goal**: 程式啟動自動載入 data.json [ref: spec.md US6]
**Milestone**: M6

**Independent Test**: 啟動程式 → ListView 自動顯示資料 [ref: raw_spec TC-01]

- [ ] T033 [US6] Create `NanmaoLabelPOC/ViewModels/MainViewModel.cs`:
  - Auto-load data.json on startup [ref: raw_spec 13.5]
  - Tab navigation: 標籤列印, 資料管理 [ref: raw_spec 8.2]
  - Handle load failure gracefully [ref: raw_spec 8.9]

- [ ] T034 [US6] Create `NanmaoLabelPOC/Views/MainWindow.xaml`:
  - Custom title bar (height 40px, #1E3A5F) [ref: raw_spec 8.3]
  - Buttons: [全螢幕 F11] [─] [□] [✕] [ref: raw_spec 8.3]
  - TabControl for pages [ref: raw_spec 8.2]
  - Status bar (height 30px) [ref: raw_spec 8.2]
  - MinWidth: 1024, MinHeight: 576 [ref: raw_spec 8.3]
  - WindowStyle="None" [ref: raw_spec 8.3]

- [ ] T035 [US6] Implement empty state message "📂 尚無資料，請至「資料管理」分頁匯入 Excel" [ref: raw_spec 8.4 空白狀態]
- [ ] T036 [US6] Disable [輸出 PDF] button when no data [ref: raw_spec 8.10]
- [ ] T037 [US6] Update `App.xaml.cs` to initialize MainWindow and trigger data load

**Checkpoint**: US6 完成 - 程式啟動自動載入 [ref: raw_spec A-01]

---

## Phase 6: User Story 3 - 批次輸出 (Priority: P2)

**Goal**: 操作員可將所有資料輸出為多頁 PDF [ref: spec.md US3]
**Milestone**: M5

**Independent Test**: 點擊「批次輸出全部」→ 產出多頁 PDF [ref: raw_spec TC-12]

- [ ] T038 [US3] Extend IPdfExporter with ExportBatch(template, records, outputPath) in `NanmaoLabelPOC/Services/IPdfExporter.cs`
- [ ] T039 [US3] Implement batch export in `NanmaoLabelPOC/Services/PdfExporter.cs`:
  - One label per page [ref: raw_spec 3.3 批次輸出規格]
  - Filename: `Labels_Batch_{yyyyMMdd_HHmmss}.pdf` [ref: raw_spec 13.6]
- [ ] T040 [US3] Add BatchExportCommand to LabelPrintViewModel in `NanmaoLabelPOC/ViewModels/LabelPrintViewModel.cs`
- [ ] T041 [US3] Add [📑 批次輸出全部] button to LabelPrintView.xaml [ref: raw_spec 8.4]
- [ ] T042 [US3] Implement completion dialog with "開啟資料夾" / "確定" options [ref: raw_spec 3.3, 8.8]
- [ ] T043 [US3] Disable [批次輸出全部] button when record count = 0 [ref: raw_spec 8.10]

**Checkpoint**: US3 完成 - 批次輸出可驗證 [ref: raw_spec A-12]

---

## Phase 7: User Story 4 - 資料匯入 (Priority: P2)

**Goal**: 管理者從 Excel 匯入交易資料 [ref: spec.md US4]
**Milestone**: M2

**Independent Test**: 匯入 Excel → data.json 正確產生 [ref: raw_spec TC-13]

- [ ] T044 [US4] Create `NanmaoLabelPOC/ViewModels/DataManageViewModel.cs`:
  - Properties: Records, SelectedRecord, IsDirty
  - Commands: ImportCommand, SaveCommand
  - Data binding for field edit area [ref: raw_spec 8.5]

- [ ] T045 [US4] Create `NanmaoLabelPOC/Views/DataManageView.xaml`:
  - Toolbar: [📥 匯入] [➕ 新增] [🗑️ 刪除] [💾 儲存] (height 60px) [ref: raw_spec 8.5]
  - DataGrid (inline editing) [ref: raw_spec 8.5]
  - Field edit area (two columns) [ref: raw_spec 8.5]
  - Grid layout: row heights 60:60*:40* [ref: raw_spec 8.5]

- [ ] T046 [US4] Implement import with OpenFileDialog (filter: *.xlsx) [ref: raw_spec 8.10]
- [ ] T047 [US4] Implement overwrite confirmation dialog "匯入將覆蓋現有資料，是否繼續？" [ref: raw_spec 8.9]
- [ ] T048 [US4] Display status bar "✅ 匯入成功，共 N 筆資料" [ref: raw_spec 8.10]
- [ ] T049 [US4] Implement Excel format error handling [ref: raw_spec 8.9]:
  - "檔案格式不正確，請確認為 .xlsx 格式"
  - "缺少必要欄位：{欄位名稱}"
- [ ] T050 [US4] Implement semicolon warning for QR Code fields [ref: raw_spec 3.3, 13.4]
- [ ] T051 [US4] Implement thousand separator rejection for quantity [ref: raw_spec 13.14]

**Checkpoint**: US4 完成 - Excel 匯入可驗證 [ref: raw_spec A-13]

---

## Phase 8: User Story 5 - 資料管理 CRUD (Priority: P3)

**Goal**: 管理者手動新增/編輯/刪除/儲存資料 [ref: spec.md US5]
**Milestone**: M6

**Independent Test**: 新增資料 → 編輯 → 儲存 → 切換分頁 → 輸出標籤 [ref: raw_spec TC-18]

- [ ] T052 [US5] Add AddRecordCommand to DataManageViewModel (add blank row) [ref: raw_spec 8.8]
- [ ] T053 [US5] Add DeleteRecordCommand with confirmation dialog "確定要刪除選取的資料嗎？此操作無法復原。" [ref: raw_spec 8.9]
- [ ] T054 [US5] Implement IsDirty tracking (enable/disable [儲存] button) [ref: raw_spec 8.10]
- [ ] T055 [US5] Implement two-way binding between DataGrid and field edit area [ref: raw_spec 8.5]
- [ ] T056 [US5] Implement unsaved changes prompt on tab switch "資料尚未儲存，是否要儲存變更？" [ref: raw_spec 8.9]
- [ ] T057 [US5] Display status bar "⚠️ 已修改（未儲存）　共 N 筆資料" [ref: raw_spec 8.10]
- [ ] T058 [US5] Display status bar "✅ 儲存成功" after save [ref: raw_spec 8.10]
- [ ] T059 [US5] Implement quantity field validation (digits only, reject non-numeric) [ref: raw_spec 13.14]
- [ ] T060 [US5] Implement required field validation with error dialog [ref: raw_spec 3.3, 8.9]

**Checkpoint**: US5 完成 - CRUD 操作可驗證 [ref: raw_spec A-14~A-18]

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: UI polish and edge case handling
**Milestone**: M7 (驗收測試)

### UI Polish - Kiosk Style [ref: raw_spec 8.1]

- [ ] T061 [P] Implement Kiosk design principles in all views:
  - Large buttons (min 100×40, recommended 140×50) [ref: raw_spec 8.6]
  - Large fonts (base 14pt for ≥1920px width) [ref: raw_spec 8.6]
  - High contrast (dark text + light background) [ref: raw_spec 8.1]
  - Min click area 44×44px, button spacing 8px [ref: raw_spec 8.6]

- [ ] T062 [P] Implement 16:9 adaptive layout (min 1024×576) [ref: raw_spec 8.1, 8.6]

- [ ] T063 [P] Implement fullscreen mode in MainWindow:
  - F11: toggle fullscreen [ref: raw_spec 8.3]
  - ESC: exit fullscreen [ref: raw_spec 8.3]
  - Show [結束全螢幕 F11/ESC] button in fullscreen [ref: raw_spec 8.3]

- [ ] T064 [P] Implement title bar drag-to-move and double-click maximize [ref: raw_spec 8.3]

### Edge Cases [ref: raw_spec 8.9, 13.15]

- [ ] T065 [P] Auto-create `.\output\` directory if not exists [ref: raw_spec 2.3]
- [ ] T066 [P] Implement file overwrite confirmation "檔案已存在，是否覆蓋？" [ref: raw_spec 8.9]
- [ ] T067 [P] Implement barcode skip when content is empty [ref: raw_spec 13.15]
- [ ] T068 [P] Implement QR Code empty value placeholder (A;;C format) [ref: raw_spec 13.4, 13.15]
- [ ] T069 [P] Implement required field missing warning "資料缺失：{欄位名稱}，無法產生標籤" [ref: raw_spec 3.3]

### Pagination [ref: raw_spec 8.4]

- [ ] T070 [P] Implement ListView pagination (item height 50px, auto-calculate per-page count) [ref: raw_spec 8.4]
- [ ] T071 [P] Implement pagination controls ◀ ▶ with page indicator [ref: raw_spec 8.4]
- [ ] T072 [P] Enable/disable ◀ ▶ based on current page [ref: raw_spec 8.10]

### Error Handling [ref: raw_spec 8.9, 13.21]

- [ ] T073 [P] Implement all error messages in Traditional Chinese [ref: raw_spec 13.21]
- [ ] T074 [P] Implement error dialog titles: 錯誤, 警告, 確認, 提示 [ref: raw_spec 8.10]

### Performance [ref: 憲章 IV]

- [ ] T075 Verify startup time ≤ 3 seconds [ref: 憲章 IV]
- [ ] T076 Verify operation response ≤ 100ms [ref: 憲章 IV]

### Final Validation [ref: raw_spec 10, 13.7]

- [ ] T077 Run all unit tests (dotnet test)
- [ ] T078 Execute TC-01 to TC-18 test scenarios [ref: raw_spec 9]
- [ ] T079 Execute A-01 to A-18 acceptance criteria [ref: raw_spec 10]
- [ ] T080 Verify barcode/QR Code scanning stability (首次即成功) [ref: raw_spec 13.24]
- [ ] T081 Verify preview vs PDF tolerance ≤ ±0.5mm [ref: raw_spec 13.23]

---

## Dependencies & Execution Order

### Phase Dependencies

```text
Phase 1 (Setup) ─────────────────────────────────────────────────┐
                                                                  │
Phase 2 (Foundational) ◀─────────────────────────────────────────┘
    │
    ├──▶ Phase 3 (US1 - 標籤列印) 🎯 MVP
    │         │
    │         ├──▶ Phase 4 (US2 - 格式切換)
    │         │
    │         ├──▶ Phase 5 (US6 - 自動載入)
    │         │
    │         └──▶ Phase 6 (US3 - 批次輸出)
    │
    └──▶ Phase 7 (US4 - 資料匯入)
              │
              └──▶ Phase 8 (US5 - 資料管理)

Phase 9 (Polish) ◀───── All user stories complete
```

### User Story Dependencies

| User Story | Priority | Depends On | Milestone |
|------------|----------|------------|-----------|
| US1 (標籤列印) | P1 | Foundational | M4, M5 |
| US2 (格式切換) | P1 | US1 | M4 |
| US6 (自動載入) | P1 | US1 | M6 |
| US3 (批次輸出) | P2 | US1 | M5 |
| US4 (資料匯入) | P2 | Foundational | M2 |
| US5 (資料管理) | P3 | US4 | M6 |

### Parallel Opportunities

**Phase 1 (Setup)**:
```bash
# T002, T003, T004, T005, T007 can run in parallel
```

**Phase 2 (Foundational)**:
```bash
# Models: T008, T009, T010 in parallel
# Interfaces: T012, T014, T016 in parallel
# Tests: T019, T020, T021 in parallel
```

**Phase 9 (Polish)**:
```bash
# All [P] tasks: T061~T074 in parallel
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (標籤列印與 PDF 輸出)
4. **STOP and VALIDATE**:
   - TC-08: 一維條碼掃描成功 ✓
   - TC-09: QR Code 掃描成功 ✓
   - TC-11: PDF 條碼掃描成功 ✓
5. Deploy/demo if ready

### Incremental Delivery

| Increment | Phases | Validation |
|-----------|--------|------------|
| MVP | 1 + 2 + 3 (US1) | A-01~A-11 |
| P1 Complete | + 4 (US2) + 5 (US6) | A-05, A-01 |
| P2 Complete | + 6 (US3) + 7 (US4) | A-12, A-13 |
| P3 Complete | + 8 (US5) | A-14~A-18 |
| Production | + 9 (Polish) | TC-01~TC-18, A-01~A-18 |

### Suggested MVP Scope

**MVP = Phase 1 + Phase 2 + Phase 3 (US1)**

驗收條件 [ref: raw_spec 10, 13.Ω]:
- [ ] 條碼與 QR Code 首次掃描即成功 [ref: raw_spec 13.24]
- [ ] 掃描結果與 data.json 資料一致 [ref: raw_spec 13.25]
- [ ] 預覽與 PDF 版面誤差 ≤ ±0.5mm [ref: raw_spec 13.23]

---

## Reviewer Notes

### 審查依據

本 tasks.md 依據以下文件審查產出：

| 文件 | 章節 | 用途 |
|------|------|------|
| raw_spec.md | 全文 | 需求規格權威來源 |
| raw_spec.md | 第 13 章 | 最高優先實作約束 |
| raw_spec.md | 第 9 章 | 測試情境 (TC-01~TC-18) |
| raw_spec.md | 第 10 章 | 驗收標準 (A-01~A-18) |
| spec.md | User Stories | 功能需求 |
| plan.md | Milestones | 里程碑對應 |
| 憲章 | II, IV | 測試標準、效能要求 |

### 關鍵約束追溯 [ref: raw_spec 13]

| 約束 | 任務 | 說明 |
|------|------|------|
| 13.1 座標 mm | T011 | UnitConverter 統一換算 |
| 13.6 雙擊防抖 | T026, T027 | 500ms debounce |
| 13.9 條碼禁裁切 | T017, T023 | PureBarcode + no ellipsis |
| 13.11 欄位名稱 | T015, T020 | alphanumeric only |
| 13.13 Raw/Display | T008, T023, T029 | 明確分離 |
| 13.14 數量欄位 | T015, T051, T059 | digits only |
| 13.15 QR 空值 | T023, T068 | A;;C format |
| 13.21 繁體中文 | T073 | 所有訊息 |
| 13.23 誤差 ±0.5mm | T081 | 驗收檢查 |
| 13.24 首次掃描 | T080 | 掃描穩定性 |
| 13.25 Single Source | T013 | data.json |

### 任務統計

| Phase | 任務數 | [P] 標記 | [Story] 標記 |
|-------|--------|----------|--------------|
| 1 Setup | 7 | 5 | 0 |
| 2 Foundational | 14 | 9 | 0 |
| 3 US1 | 8 | 1 | 7 |
| 4 US2 | 3 | 0 | 3 |
| 5 US6 | 5 | 0 | 5 |
| 6 US3 | 6 | 0 | 6 |
| 7 US4 | 8 | 0 | 8 |
| 8 US5 | 9 | 0 | 9 |
| 9 Polish | 21 | 14 | 0 |
| **Total** | **81** | **29** | **38** |

---

## Revision History

| 版本 | 日期 | 修訂內容 |
|------|------|----------|
| v1.0 | 2026-01-27 | 初版產出 |
| v1.1 | 2026-01-28 | 依第三方審查報告修正：(1) T022~T026 補上 [US1] 標記 (2) T008 補上 Id (UUID) 欄位 [ref: raw_spec 附錄 B.2] (3) T013 補上 version 欄位與 UUID 生成 [ref: raw_spec 附錄 B.1] (4) T019 補上 version/UUID 測試項目 |

---

## Notes

- [P] = different files, no dependencies
- [Story] = maps to user story for traceability
- All file paths match raw_spec 7.2 structure
- All error messages in Traditional Chinese [ref: raw_spec 13.21]
- PDF output is the ONLY acceptance authority [ref: raw_spec 13.10]
- Barcode scanning must succeed on first attempt [ref: raw_spec 13.24]
