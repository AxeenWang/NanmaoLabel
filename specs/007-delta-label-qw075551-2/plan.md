# Implementation Plan: QW075551-2 物料標籤渲染

**Branch**: `007-delta-label-qw075551-2` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-delta-label-qw075551-2/spec.md`

## Summary

本功能對 QW075551-2 標籤模板進行全面重構，從原「出貨標籤」改為「物料標籤」。主要變更包括：
- 標籤尺寸由 100mm × 60mm 變更為 100mm × 80mm
- 版面由上下分區式變更為左右兩欄式佈局
- 標題由「出貨標籤 Shipping Label」變更為「物料標籤」
- ERPPARTNO 由 Barcode 改為 Text
- 移除 QR Code
- 欄位採用雙行顯示格式（小字行 + 大字行）
- 欄位前綴標籤由英文改為中文

## Technical Context

**Language/Version**: C# / .NET 8 LTS
**Primary Dependencies**: CommunityToolkit.Mvvm, QuestPDF, ZXing.Net (本功能不使用條碼)
**Storage**: JSON 檔案 (DataStore)
**Testing**: dotnet test (NanmaoLabelPOC.Tests)
**Target Platform**: Windows (WPF)
**Project Type**: Single project (WPF Desktop)
**Performance Goals**: 單張標籤 PDF 輸出 < 3 秒 [spec.md SC-001]
**Constraints**: 字體最小 6pt，溢出時允許換行 [spec.md Clarification]
**Scale/Scope**: 內建標籤模板修改，不影響 QW075551-1

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 條文 | 狀態 | 說明 |
|------|------|------|------|
| I. 程式碼品質 | 單一職責原則 | ✅ PASS | 修改集中於 BuiltInTemplates.cs |
| II. 測試標準 | 核心邏輯需單元測試 | ✅ PASS | 現有 LabelRenderer 測試架構可擴充 |
| III. 使用者體驗 | 繁體中文錯誤訊息 | ✅ PASS | 無新增錯誤訊息 |
| IV. 效能要求 | 標註操作 < 100ms | ✅ PASS | PDF 輸出已有效能監控 |
| A-1. 文件依據 | 依據指定文件內容 | ✅ PASS | 依據 spec.md 與 raw_delta_label_QW075551-2.md |
| A-2. 禁止預設設計 | 禁止未定義擴充點 | ✅ PASS | 僅修改既有模板定義 |
| A-3. 可執行可驗證 | 具體驗收條件 | ✅ PASS | spec.md 定義 8 項成功標準 |
| A-4. 需求追溯 | 標註需求編號 | ✅ PASS | 所有變更對應 FR-001 ~ FR-012 |
| A-5. 格式遵循 | 遵循模板格式 | ✅ PASS | 本文件遵循 plan-template.md |

## Project Structure

### Documentation (this feature)

```text
specs/007-delta-label-qw075551-2/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── Models/
│   ├── LabelTemplate.cs      # 標籤模板模型 (無修改)
│   └── LabelField.cs         # 欄位模型 (無修改)
├── Services/
│   ├── LabelRenderer.cs      # 標籤渲染服務 (可能需微調雙行渲染)
│   └── PdfExporter.cs        # PDF 輸出服務 (可能需支援雙行格式)
└── Templates/
    └── BuiltInTemplates.cs   # 內建標籤模板 (**主要修改**)

NanmaoLabelPOC.Tests/
└── Services/
    └── LabelRendererTests.cs  # 渲染測試 (新增 QW075551-2 測試案例)
```

**Structure Decision**: 本功能為既有模板修改，不新增檔案，主要修改 `BuiltInTemplates.cs` 中的 `CreateQW075551_2()` 方法。

## Complexity Tracking

> **無 Constitution Check 違規需要說明**

本功能為模板定義修改，複雜度低，不涉及架構變更。

## Constitution Check (Post-Design)

*Re-check after Phase 1 design completion*

| 原則 | 條文 | 狀態 | 說明 |
|------|------|------|------|
| I. 程式碼品質 | 單一職責原則 | ✅ PASS | 僅修改 BuiltInTemplates.CreateQW075551_2() |
| II. 測試標準 | 核心邏輯需單元測試 | ✅ PASS | 預計新增 QW075551-2 模板測試案例 |
| A-1. 文件依據 | 依據指定文件內容 | ✅ PASS | data-model.md 所有欄位皆追溯至 spec.md |
| A-4. 需求追溯 | 標註需求編號 | ✅ PASS | data-model.md 每個欄位標註 FR-xxx |

## Generated Artifacts

| 檔案 | 狀態 | 說明 |
|------|------|------|
| plan.md | ✅ Complete | 本文件 |
| research.md | ✅ Complete | Phase 0 研究結論 |
| data-model.md | ✅ Complete | 19 個 LabelField 定義 |
| quickstart.md | ✅ Complete | 開發快速指南 |
| tasks.md | ✅ Complete | 27 個任務，16 個可並行 |
