# Implementation Plan: 按鈕樣式規範（極簡 Kiosk 風格）

**Branch**: `003-button-style` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-button-style/spec.md`

## Summary

實作極簡 Kiosk 風格的按鈕樣式系統，包含：
1. 定義統一的顏色 Token（Color Resource）於 App.xaml
2. 建立可重用的按鈕樣式（ActionButtonStyle、DisabledButtonStyle、SecondaryButtonStyle、TabHeaderStyle）
3. 套用位置分區佈局於資料管理工具列
4. 實作狀態回饋動畫（脈動光暈、Loading 狀態）
5. 確保刪除按鈕的二次確認保護機制

## Technical Context

**Language/Version**: C# / .NET 8 LTS
**Primary Dependencies**: WPF (內建)、CommunityToolkit.Mvvm (已安裝)
**Storage**: N/A（純 UI 樣式，無資料儲存）
**Testing**: xUnit + WPF Test Helper（視覺驗證以手動為主）
**Target Platform**: Windows 10/11（WPF Desktop）
**Project Type**: Single WPF Application
**Performance Goals**: 動畫 60fps、狀態切換 <16ms
**Constraints**: 低階硬體可降級為無動畫；遵循 raw_spec 8.6 尺寸定義
**Scale/Scope**: 2 個分頁、約 10 個按鈕元件

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 檢查項目 | 狀態 |
|------|----------|------|
| I. 程式碼品質 | 樣式定義於 ResourceDictionary，遵循單一職責 | ✅ Pass |
| II. 測試標準 | 核心邏輯（狀態優先序）需單元測試 | ✅ Pass |
| III. UX 一致性 | 使用共用 ResourceDictionary、繁體中文訊息 | ✅ Pass |
| IV. 效能要求 | 動畫可降級、無 UI 執行緒阻塞 | ✅ Pass |
| A-1. 文件依據 | 所有顏色/間距來自 raw_delta_button.md | ✅ Pass |
| A-2. 禁止預設設計 | 無預留擴充點，僅實作當前需求 | ✅ Pass |
| A-3. 可執行可驗證 | 每項需求有對應驗收條件 | ✅ Pass |
| A-4. 需求追溯 | 所有實作對應 FR-001 ~ FR-025 | ✅ Pass |
| A-5. 格式遵循 | 遵循 plan-template.md 格式 | ✅ Pass |

**Gate Result**: ✅ 全部通過，可進入 Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/003-button-style/
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal - no unknowns)
├── data-model.md        # Phase 1 output (UI entities)
├── quickstart.md        # Phase 1 output (implementation guide)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── App.xaml                           # 全域資源字典（新增 Color Tokens + Button Styles）
├── Resources/                         # 【新增】資源檔案目錄
│   └── ButtonStyles.xaml              # 【新增】按鈕樣式定義
├── Converters/
│   └── BoolToVisibilityConverter.cs   # 既有
├── Views/
│   ├── MainWindow.xaml                # 修改：套用 TabHeaderStyle
│   ├── DataManageView.xaml            # 修改：套用位置分區 + 按鈕樣式
│   └── LabelPrintView.xaml            # 修改：套用按鈕樣式
└── ViewModels/
    ├── DataManageViewModel.cs         # 修改：新增 IsDirty 屬性、刪除確認邏輯
    └── LabelPrintViewModel.cs         # 修改：新增 IsLoading 狀態

NanmaoLabelPOC.Tests/
└── ViewModels/
    └── ButtonStateTests.cs            # 【新增】狀態優先序測試
```

**Structure Decision**: 沿用既有 WPF MVVM 結構，新增 `Resources/` 目錄存放樣式定義，符合 Constitution III（使用共用 ResourceDictionary）。

## Complexity Tracking

> 無違規需要說明。設計符合憲章所有原則。

---

## Phase 0: Research

**Status**: ✅ 完成（無需額外研究）

本功能為純 UI 樣式實作，所有技術細節已在 raw_delta_button.md 明確定義：

| 項目 | 來源 | 決策 |
|------|------|------|
| 顏色定義 | raw_delta_button.md §4 | 直接使用定義值 |
| 間距規範 | raw_delta_button.md §2 | 8px/32px 如文件定義 |
| 動畫規格 | raw_delta_button.md §5.3 | 1.5s 週期、ease-in-out |
| 狀態優先序 | raw_delta_button.md §8.1 | 7 層優先序如文件定義 |

**Output**: `research.md` 已生成（記錄來源對應）

---

## Phase 1: Design & Contracts

### Data Model

本功能無資料庫實體，但定義以下 UI 相關模型：

| Entity | 用途 | 屬性 |
|--------|------|------|
| ColorToken | 顏色資源定義 | Key, HexValue, Description |
| ButtonStyle | 按鈕樣式定義 | StyleKey, TargetType, BasedOn, Triggers |
| ButtonState | 按鈕狀態枚舉 | Default, Hover, Active, Focus, Disabled, Loading |

### Contracts

本功能無 API，但定義以下 ViewModel 介面契約：

**IButtonStateProvider** (DataManageViewModel, LabelPrintViewModel 實作)
```
Properties:
  - IsDirty: bool          # 是否有未儲存變更
  - IsLoading: bool        # 是否正在執行操作
  - CanDelete: bool        # 刪除按鈕是否啟用
  - CanSave: bool          # 儲存按鈕是否啟用

Commands:
  - DeleteCommand          # 含二次確認邏輯
  - SaveCommand            # 含 Loading 狀態管理
```

### Implementation Quickstart

**Step 1**: 建立 Resources/ButtonStyles.xaml
- 定義 Color Tokens（9 個顏色資源）
- 定義 ActionButtonStyle（含 Hover/Active/Focus Triggers）
- 定義 DisabledButtonStyle
- 定義 SecondaryButtonStyle（分頁導航按鈕）
- 定義 TabHeaderStyle（頁簽樣式）

**Step 2**: 修改 App.xaml
- 合併 ButtonStyles.xaml 至 Application.Resources

**Step 3**: 修改 DataManageView.xaml
- 重新排列工具列按鈕（建立區 | 危險區 | 確認區）
- 套用間距規範（8px / 32px）
- 套用按鈕樣式

**Step 4**: 修改 LabelPrintView.xaml
- 套用按鈕樣式
- 調整輸出按鈕佈局（並排置中）

**Step 5**: 修改 MainWindow.xaml
- 套用 TabHeaderStyle 至頁簽

**Step 6**: 實作動畫效果
- 儲存按鈕脈動光暈（Storyboard）
- Loading 狀態指示器

**Step 7**: 修改 ViewModel
- DataManageViewModel: 新增 IsDirty、刪除確認對話框
- LabelPrintViewModel: 新增 IsLoading 狀態管理

---

## Constitution Re-Check (Post-Design)

| 原則 | 檢查項目 | 狀態 |
|------|----------|------|
| I. 程式碼品質 | 樣式集中於 ResourceDictionary | ✅ Pass |
| II. 測試標準 | 狀態優先序有對應測試 | ✅ Pass |
| III. UX 一致性 | 共用樣式、刪除確認對話框 | ✅ Pass |
| IV. 效能要求 | 動畫使用 Storyboard（GPU 加速） | ✅ Pass |
| A-1. 文件依據 | 所有數值來自 raw_delta_button.md | ✅ Pass |
| A-4. 需求追溯 | 見下方對應表 | ✅ Pass |

### 需求追溯矩陣

| 需求 | 實作位置 |
|------|----------|
| FR-001~FR-006 (位置) | DataManageView.xaml, LabelPrintView.xaml |
| FR-007~FR-011 (樣式) | ButtonStyles.xaml |
| FR-012~FR-015 (狀態) | ButtonStyles.xaml Triggers |
| FR-016~FR-018 (動畫) | ButtonStyles.xaml Storyboards |
| FR-019~FR-020 (保護) | DataManageViewModel.cs |
| FR-021 (優先序) | ButtonStyles.xaml Trigger 順序 |
| FR-022~FR-025 (頁簽) | ButtonStyles.xaml TabHeaderStyle |

---

## Next Steps

1. 執行 `/speckit.tasks` 生成任務清單
2. 依任務順序實作
3. 驗收測試對照 spec.md Success Criteria
