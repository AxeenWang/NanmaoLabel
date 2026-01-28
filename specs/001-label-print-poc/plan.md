# Implementation Plan: 南茂標籤列印 POC

**Branch**: `001-label-print-poc` | **Date**: 2026-01-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-label-print-poc/spec.md`
**Review Note**: 本計畫經第三方審查，所有內容嚴格對應 `raw_spec.md` 定義，不含自行新增之功能或結構。

## Summary

本 POC 驗證「讀取交易資料，依據預定義的標籤格式，產出正確且條碼可被掃描的標籤 PDF 檔案」之核心功能 [ref: raw_spec 1]。採用 C# WPF (.NET 8 LTS) 開發，MVVM 架構 [ref: raw_spec 7]，使用 QuestPDF 輸出 PDF、ZXing.Net 產生條碼 [ref: raw_spec 7.1]。標籤格式（QW075551-1、QW075551-2）直接內建於程式碼 [ref: raw_spec 1]，資料以 JSON 檔案儲存 [ref: raw_spec 2.2]。

## Technical Context

**Language/Version**: C# / .NET 8 LTS [ref: raw_spec 7]
**Primary Dependencies**: CommunityToolkit.Mvvm、ExcelDataReader、ZXing.Net、QuestPDF [ref: raw_spec 7.1]
**Storage**: JSON 檔案 (`.\data\data.json`) [ref: raw_spec 2.3]
**Testing**: MSTest / xUnit（NanmaoLabelPOC.Tests 專案）[ref: raw_spec 7.2, 憲章 II]
**Target Platform**: Windows 10/11 Desktop [ref: raw_spec 7]
**Project Type**: Single WPF Application [ref: raw_spec 7.2]
**Performance Goals**: 應用程式啟動至可操作狀態 ≤ 3 秒、操作回應 ≤ 100ms [ref: 憲章 IV]
**Constraints**: PDF 尺寸 100mm x 60mm [ref: raw_spec 5.1]、條碼/QR Code 首次掃描即成功 [ref: raw_spec 13.3]、預覽與 PDF 誤差容許極小差異 [ref: raw_spec 13.10]
**Scale/Scope**: POC 階段資料量預期為數十至數百筆 [ref: spec.md Assumptions]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 檢查項目 | 狀態 | 說明 |
|------|----------|------|------|
| I. 程式碼品質 | 遵循 .editorconfig、命名清晰、禁止魔術數字 | ✅ PASS | 將於實作階段落實 |
| II. 測試標準 | 核心業務邏輯須有單元測試 | ✅ PASS | NanmaoLabelPOC.Tests 專案已定義 [ref: raw_spec 7.2] |
| III. 使用者體驗 | 錯誤訊息繁體中文、載入超過 500ms 顯示進度 | ✅ PASS | IC-027 與憲章 III 一致 |
| IV. 效能要求 | 啟動 ≤ 3 秒、操作回應 ≤ 100ms | ✅ PASS | 資料量小，JSON 讀寫可達成 |
| A-1. 文件依據 | 僅依據 raw_spec.md 與 spec.md | ✅ PASS | 專案結構完全對應 raw_spec 7.2 |
| A-2. 禁止預設設計 | 不預留未定義之擴充點 | ✅ PASS | 無自行新增之功能或結構 |
| A-3. 可執行可驗證 | 每項需求有驗收條件 | ✅ PASS | SC-001~010 已定義 |
| A-4. 需求追溯 | 工作對應需求編號 | ✅ PASS | tasks.md 將標註 FR/IC 編號 |
| A-5. 格式遵循 | 遵循指定模板 | ✅ PASS | 使用 plan-template.md |

**Pre-Phase 0 Gate**: ✅ 通過，無違規需說明

**Post-Phase 1 Re-check (2026-01-27)**:

| 原則 | 檢查項目 | 狀態 | 驗證依據 |
|------|----------|------|----------|
| I. 程式碼品質 | 資料模型命名清晰 | ✅ PASS | data-model.md 欄位命名符合 raw_spec 定義 |
| II. 測試標準 | 測試項目依據 raw_spec 9 | ✅ PASS | NanmaoLabelPOC.Tests 涵蓋核心模組 |
| III. 使用者體驗 | 錯誤訊息繁體中文 | ✅ PASS | data-model.md 8. Validation Rules |
| A-1. 文件依據 | 所有設計對應 raw_spec | ✅ PASS | 專案結構、介面命名完全對應 raw_spec 7.2 |
| A-2. 禁止預設設計 | 無未定義擴充點 | ✅ PASS | 僅內建兩種標籤格式，無外部設定 |
| A-4. 需求追溯 | 設計對應需求 | ✅ PASS | 所有項目標註 ref: raw_spec 章節 |
| A-5. 格式遵循 | 使用指定模板 | ✅ PASS | 符合模板結構 |

**Post-Phase 1 Gate**: ✅ 通過

## Project Structure

### Documentation (this feature)

```text
specs/001-label-print-poc/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── data-schema.json # JSON Schema for data.json
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root) [ref: raw_spec 7.2]

> ⚠️ **本結構嚴格遵循 raw_spec 7.2 定義，不得自行新增目錄或變更命名**

```text
NanmaoLabel/
├── NanmaoLabel.slnx
├── NanmaoLabelPOC/                    # 主程式 (WPF)
│   ├── Views/                         # XAML 畫面
│   │   ├── MainWindow.xaml            # 主視窗（分頁容器）
│   │   ├── LabelPrintView.xaml        # 標籤列印分頁
│   │   └── DataManageView.xaml        # 資料管理分頁
│   ├── ViewModels/                    # MVVM ViewModel
│   │   ├── MainViewModel.cs
│   │   ├── LabelPrintViewModel.cs
│   │   └── DataManageViewModel.cs
│   ├── Models/                        # 資料模型
│   │   ├── LabelTemplate.cs           # 標籤格式定義
│   │   ├── LabelField.cs              # 欄位定義
│   │   └── DataRecord.cs              # 資料紀錄
│   ├── Services/                      # 商業邏輯
│   │   ├── IExcelImporter.cs          # Excel 匯入介面
│   │   ├── ExcelImporter.cs           # Excel 讀取實作
│   │   ├── IDataStore.cs              # 資料儲存介面
│   │   ├── DataStore.cs               # JSON 讀寫實作
│   │   ├── ILabelRenderer.cs          # 標籤渲染介面
│   │   ├── LabelRenderer.cs           # 標籤渲染實作
│   │   ├── IBarcodeGenerator.cs       # 條碼生成介面
│   │   ├── BarcodeGenerator.cs        # 條碼生成實作
│   │   ├── IPdfExporter.cs            # PDF 輸出介面
│   │   └── PdfExporter.cs             # PDF 輸出實作
│   ├── Templates/                     # 內建標籤格式
│   │   └── BuiltInTemplates.cs        # QW075551-1、QW075551-2 定義
│   ├── Converters/                    # XAML 值轉換器
│   ├── Resources/                     # 資源檔
│   ├── App.xaml
│   └── App.xaml.cs
├── NanmaoLabelPOC.Tests/              # 單元測試專案
│   ├── Services/
│   │   ├── ExcelImporterTests.cs      # Excel 匯入測試
│   │   ├── DataStoreTests.cs          # JSON 讀寫測試
│   │   ├── BarcodeGeneratorTests.cs   # 條碼生成測試
│   │   └── LabelRendererTests.cs      # 標籤渲染測試
│   └── NanmaoLabelPOC.Tests.csproj
└── samples/                           # 範例檔案
    └── MockData.xlsx                  # 模擬資料範例
```

> **備註**：需求規格書存放於 `.dao/raw_spec.md`（專案配置目錄），不納入原始碼專案結構。

### 執行時目錄結構 [ref: raw_spec 2.3]

> 以下目錄於程式執行時建立，不屬於原始碼專案結構

```text
NanmaoLabelPOC/                        # 程式執行目錄
├── NanmaoLabelPOC.exe
├── data/
│   └── data.json                      # 交易資料快取
├── output/                            # PDF 輸出預設目錄
│   ├── Label_G25A111577_20250126_143052.pdf
│   └── Labels_Batch_20250126_150000.pdf
└── samples/
    └── MockData.xlsx
```

**Structure Decision**: 採用 raw_spec 7.2 定義之專案架構。測試專案 `NanmaoLabelPOC.Tests/` 與主專案同層級，符合 .NET 慣例與憲章 II 測試要求。

## Complexity Tracking

*無需追蹤 - Constitution Check 無違規*

---

## Implementation Phases

### Phase 0: Research & Technical Decisions

**產出物**: [research.md](research.md)

| 研究主題 | 目的 | 依據 |
|----------|------|------|
| QuestPDF 字型嵌入 | 確認微軟正黑體嵌入方式 | raw_spec 5.3, IC-022 |
| ZXing.Net Code 128 參數 | Quiet Zone 保留、條碼下方文字配置 | raw_spec 6.1, IC-008 |
| ZXing.Net QR Code 參數 | Level M 容錯、UTF-8 編碼、Quiet Zone | raw_spec 6.2, IC-009~010 |
| WPF 雙擊事件處理 | 避免雙擊地獄，確保 IC-013 規範實現 | raw_spec 8.8, IC-013 |
| mm → pt 渲染一致性 | 預覽與 PDF 共用邏輯 | IC-001~004 |

### Phase 1: Design & Contracts

**產出物**: [data-model.md](data-model.md)、[contracts/](contracts/)、[quickstart.md](quickstart.md)

| 產出物 | 內容 |
|--------|------|
| data-model.md | DataRecord 欄位定義、LabelTemplate/LabelField 結構、Raw Value vs Display Value 處理 |
| contracts/data-schema.json | data.json 的 JSON Schema |
| quickstart.md | 開發環境設定、專案建置、執行指南 |

### Phase 2: Task Generation

**產出物**: [tasks.md](tasks.md)（由 `/speckit.tasks` 指令產生）

---

## Milestones [ref: raw_spec 3, 9, 10]

| 里程碑 | 內容 | 驗收檢查點 | 對應 raw_spec |
|--------|------|------------|---------------|
| M1: 專案骨架 | 建立 Solution、專案結構、NuGet 套件 | 可編譯、可執行空白視窗 | 7.2 |
| M2: 資料層 | DataStore (JSON CRUD)、DataLoader (Excel) | TC-13: 匯入 Excel 產生 data.json | 3.1 F-01~F-02, 3.2 |
| M3: 條碼生成 | BarcodeGenerator (Code 128 + QR Code) | TC-08, TC-09: 條碼可被掃描 | 6.1, 6.2 |
| M4: 標籤渲染 | LabelRenderer、LabelTemplates | TC-02~TC-05: 變數對應、常數值、格式切換 | 4, 5.1, 5.2 |
| M5: PDF 輸出 | PdfExporter、單筆/批次輸出 | TC-10~TC-12: PDF 輸出、條碼可掃描 | 3.1 F-08~F-09 |
| M6: UI 整合 | 標籤列印分頁、資料管理分頁、狀態列 | TC-01, TC-06, TC-07, TC-14~TC-18 | 8 |
| M7: 驗收測試 | 完整測試情境、條碼掃描驗證 | A-01~A-18 全數通過 | 10 |

---

## Acceptance Criteria Mapping [ref: raw_spec 10]

### 標籤列印驗收 [ref: raw_spec 10.1]

| 編號 | 驗收項目 | 通過條件 | 里程碑 |
|------|----------|----------|--------|
| A-01 | JSON 自動載入 | 程式啟動時自動載入並顯示資料 | M2 |
| A-02 | 變數對應 | 資料源欄位值正確顯示於標籤 | M4 |
| A-03 | 常數值處理 | 固定值正確顯示（如 "17008"） | M4 |
| A-04 | 標籤預覽 | 畫面顯示內容與格式定義一致 | M4 |
| A-05 | 格式切換 | 兩種標籤格式皆能正確顯示 | M4 |
| A-06 | 單擊預覽 | 單擊 ListView 項目即時預覽標籤 | M6 |
| A-07 | 雙擊輸出 | 雙擊 ListView 項目直接輸出 PDF | M6 |
| A-08 | 一維條碼 | Code 128 條碼可被掃描，內容正確 | M3, M5 |
| A-09 | QR Code | QR Code 可被掃描，內容正確 | M3, M5 |
| A-10 | PDF 輸出 | PDF 版面與預覽一致 | M5 |
| A-11 | PDF 條碼 | PDF 上的條碼可被掃描 | M5 |
| A-12 | 批次輸出 | 多筆資料正確輸出至單一 PDF | M5 |

### 資料管理驗收 [ref: raw_spec 10.2]

| 編號 | 驗收項目 | 通過條件 | 里程碑 |
|------|----------|----------|--------|
| A-13 | 資料源匯入 | Excel 正確解析並儲存為 data.json | M2 |
| A-14 | 新增資料 | 可新增空白資料列並編輯 | M6 |
| A-15 | 編輯資料 | DataGrid 與欄位編輯區雙向同步 | M6 |
| A-16 | 刪除資料 | 可刪除選取的資料列 | M6 |
| A-17 | 儲存資料 | 變更正確儲存至 data.json | M2 |
| A-18 | 新增後列印 | 新增的資料可正確產生標籤與 PDF | M7 |

---

## POC 預期產出物 [ref: raw_spec 11]

| 項目 | 說明 | 驗收方式 |
|------|------|----------|
| 執行檔 | `NanmaoLabelPOC.exe`（Portable / 免安裝） | 可執行 |
| 資料源範例 | `MockData.xlsx`（模擬交易資料） | 可匯入 |
| JSON 範例 | `data.json`（程式產生的本地快取） | 格式正確 |
| PDF 樣本 | 程式產出的 PDF 標籤範例 | 條碼可掃描 |
| 原始碼 | 完整 C# 專案原始碼 | 可編譯 |

---

## Risk Mitigation

| 風險 | 緩解措施 | 對應規範 |
|------|----------|----------|
| 條碼掃描失敗 | Phase 0 先驗證 ZXing.Net 輸出品質，提早測試 PDF 列印掃描 | raw_spec 13.3 |
| 字型顯示異常 | 確認 QuestPDF 字型嵌入，使用 Windows 內建微軟正黑體 | raw_spec 5.3 |
| 預覽與 PDF 不一致 | 共用 mm → pt 換算邏輯，統一座標系統 | raw_spec 13.1 |

---

## Dependency Summary [ref: raw_spec 7.1]

| 套件 | 版本建議 | 用途 | 授權 |
|------|----------|------|------|
| CommunityToolkit.Mvvm | 8.x | MVVM 框架 | MIT |
| ExcelDataReader | 3.x | Excel 讀取 | MIT |
| ExcelDataReader.DataSet | 3.x | DataSet 擴充 | MIT |
| ZXing.Net | 0.16.x | 條碼生成 | Apache 2.0 |
| QuestPDF | 2024.x | PDF 輸出 | Community License* |

> *QuestPDF Community License：年營收 < 100 萬美金免費，正式上線再評估授權 [ref: raw_spec 7.1]

---

## Out of Scope [ref: raw_spec 12]

以下項目明確排除於 POC 範圍外，不得於本實作計畫中加入：

| 項目 | 說明 |
|------|------|
| 搜尋/篩選功能 | 資料量大時快速搜尋目標資料 |
| 設定檔匯入 | 將內建標籤格式改為外部設定檔驅動 |
| 欄位彈性設定 | UI 介面讓使用者自行配對欄位對應 |
| 標籤格式編輯器 | 視覺化拖拉調整標籤版面 |
| 複雜邏輯處理 | 支援 If-Else、運算式等進階邏輯 |
| 資料庫連接 | 改為從 Oracle/SAP HANA 取得資料 |
| 實際列印 | 連接標籤印表機（Zebra、TSC 等） |

---

## Verification Checklist (Post-Phase 1)

| 驗證項目 | 對應規範 | 狀態 |
|----------|----------|------|
| 專案結構完全對應 raw_spec 7.2 | A-1, A-2 | ✅ |
| data-model.md 定義 Raw Value / Display Value 處理 | IC-005~006 | ✅ |
| data-schema.json 定義 lastModified 欄位（ISO 8601） | IC-017 | ✅ |
| quickstart.md 包含字型嵌入說明 | IC-021~022 | ✅ |
| 座標系統統一為 mm，換算公式記錄 | IC-001~003 | ✅ |
| LabelTemplates 定義符合 raw_spec 5.1、5.2 座標規格 | raw_spec 5.1, 5.2 | ✅ |
| 無自行新增之功能、目錄或擴充點 | A-1, A-2 | ✅ |

---

## Reviewer Notes

### 審查修正記錄 (2026-01-27)

| 項目 | 原內容 | 修正為 | 依據 |
|------|--------|--------|------|
| 專案根目錄 | `src/NanmaoLabelPOC/` | `NanmaoLabelPOC/` | raw_spec 7.2 |
| Services 結構 | 有 `Interfaces/` 子目錄 | 無子目錄，介面與實作同層 | raw_spec 7.2 |
| data/output 目錄 | 列於專案架構 | 移至「執行時目錄結構」 | raw_spec 2.3 vs 7.2 |

### 需求變更記錄 (2026-01-27 - speckit.verify)

| 項目 | 變更前 | 變更後 | 說明 |
|------|--------|--------|------|
| Excel 匯入介面 | `IDataLoader.cs` / `DataLoader.cs` | `IExcelImporter.cs` / `ExcelImporter.cs` | 命名更明確反映功能 |
| 標籤模板檔案 | `LabelTemplates.cs` | `BuiltInTemplates.cs` | 強調內建性質 |
| 文件目錄 | `docs/需求規格書.md` | `.dao/raw_spec.md` | 移至專案配置目錄 |
| 測試專案 | 無 | `NanmaoLabelPOC.Tests/` | 新增，符合憲章 II 測試要求 |

### 審查結論

本實作計畫已依據 `raw_spec.md` 完成修正，所有專案結構、介面命名、目錄配置皆嚴格對應原始需求規格書定義。測試專案結構已納入 raw_spec 7.2，符合憲章 II 測試標準要求。
