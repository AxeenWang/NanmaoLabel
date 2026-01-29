# Implementation Plan: Excel 匯入警告分級

**Branch**: `002-import-warning` | **Date**: 2026-01-29 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-import-warning/spec.md`

## Summary

修正 Excel 匯入欄位名稱驗證規則（允許底線）並實作三級訊息分類（Error/Warning/Info），讓使用者快速識別問題嚴重程度。

## Technical Context

**Language/Version**: C# / .NET 8 LTS
**Primary Dependencies**: CommunityToolkit.Mvvm、ExcelDataReader
**Storage**: JSON 檔案（data.json）- 格式不變
**Testing**: xUnit（NanmaoLabelPOC.Tests）
**Target Platform**: Windows (WPF)
**Project Type**: Single WPF application
**Performance Goals**: 匯入操作 < 3 秒（現有標準）
**Constraints**: 向後相容現有 Excel 檔案、data.json 格式不變
**Scale/Scope**: 單機桌面應用，資料量 < 1000 筆

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 狀態 | 說明 |
|------|------|------|
| I. 程式碼品質 | ✅ PASS | 修改範圍明確，遵循既有命名規範 |
| II. 測試標準 | ✅ PASS | 需新增 MessageSeverity 相關測試 |
| III. 使用者體驗一致性 | ✅ PASS | 訊息使用繁體中文、Expander 為 WPF 標準控件 |
| IV. 效能要求 | ✅ PASS | 不影響匯入效能 |
| A-1. 文件依據原則 | ✅ PASS | 依據 raw_delta_import.md 與 spec.md |
| A-2. 禁止預設設計原則 | ✅ PASS | 僅實作規格定義的三級分類 |
| A-3. 可執行可驗證原則 | ✅ PASS | 每項變更可透過單元測試驗證 |
| A-4. 需求追溯原則 | ✅ PASS | 變更對應 FR-001~FR-010 |
| A-5. 格式遵循原則 | ✅ PASS | 使用標準 plan 模板 |

## Project Structure

### Documentation (this feature)

```text
specs/002-import-warning/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A for this feature)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── Models/
│   └── (無新增)
├── Services/
│   ├── IExcelImporter.cs        # 修改：ImportResult 新增 Messages、MessageSeverity
│   └── ExcelImporter.cs         # 修改：正規表達式、訊息分級邏輯
├── ViewModels/
│   └── DataManageViewModel.cs   # 修改：ImportResult 顯示邏輯
└── Views/
    └── DataManageView.xaml(.cs) # 修改：結果對話框、Info 收合/展開

NanmaoLabelPOC.Tests/
└── Services/
    └── ExcelImporterTests.cs    # 修改：新增 MessageSeverity 測試
```

**Structure Decision**: 沿用現有 WPF MVVM 架構，不新增專案或目錄。

## Complexity Tracking

> **無違規項目** - 設計符合憲章所有原則。
