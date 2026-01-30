# Implementation Plan: ListView/DataGrid Item 選取狀態視覺規範

**Branch**: `004-listview-style` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-listview-style/spec.md`

## Summary

修正 ListView 與 DataGrid 在選取狀態時光條破碎、Hover 覆蓋 Selected 的問題。透過建立共用 ControlTemplate 並使用 MultiTrigger 處理狀態優先級，確保兩分頁視覺一致。

## Spec Clarifications Applied

| 日期 | 變更 | 影響範圍 |
|------|------|----------|
| 2026-01-30 | 字體大小 15pt → 16pt | FR-005, US4, SC-006, Phase 3 |

## Technical Context

**Language/Version**: C# 12 / .NET 8 LTS
**Primary Dependencies**: WPF (Windows Presentation Foundation)、CommunityToolkit.Mvvm
**Storage**: N/A（純 UI 變更）
**Testing**: 手動視覺驗收（WPF UI 測試需人工確認視覺效果）
**Target Platform**: Windows 10/11 Desktop
**Project Type**: Single WPF Application
**Performance Goals**: 狀態切換無可感知延遲（< 16ms 畫面更新）
**Constraints**: 光條完整無破碎；Selected 狀態不被 Hover 覆蓋
**Scale/Scope**: 2 個分頁（LabelPrintView、DataManageView），共 2 個控制項（ListView、DataGrid）

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 狀態 | 依據 |
|------|------|------|
| I. 程式碼品質 | ✅ PASS | 樣式定義於共用 ResourceDictionary，遵循單一職責 |
| II. 測試標準 | ✅ PASS | 視覺變更以手動驗收清單驗證，無核心業務邏輯變更 |
| III. 使用者體驗一致性 | ✅ PASS | FR-017/FR-018 要求兩分頁視覺一致，使用共用樣式 |
| IV. 效能要求 | ✅ PASS | 純 XAML 樣式變更，無 UI 執行緒阻塞操作 |
| A-1. 文件依據原則 | ✅ PASS | 所有設計依據 raw_delta_listview.md 與 spec.md |
| A-2. 禁止預設設計原則 | ✅ PASS | 僅實作當前需求，無預留擴充點 |
| A-3. 可執行可驗證原則 | ✅ PASS | 每項需求皆有對應驗收條件 |
| A-4. 需求追溯原則 | ✅ PASS | 所有變更可追溯至 FR-001~FR-018 |
| A-5. 格式遵循原則 | ✅ PASS | 依模板格式撰寫 |

## Project Structure

### Documentation (this feature)

```text
specs/004-listview-style/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (N/A - 純 UI 無資料模型)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── App.xaml                           # 引用共用樣式資源
├── Resources/
│   ├── ButtonStyles.xaml              # 既有按鈕樣式（參考）
│   └── ListViewStyles.xaml            # 新增：ListView/DataGrid 共用樣式
└── Views/
    ├── LabelPrintView.xaml            # 修改：套用 ListViewItemStyle
    └── DataManageView.xaml            # 修改：套用 DataGridRowStyle
```

**Structure Decision**: 遵循既有架構，新增 `Resources/ListViewStyles.xaml` 作為共用樣式檔，與 `ButtonStyles.xaml` 一致。

## Complexity Tracking

> No violations. All requirements map to minimal necessary changes.

| Item | Status |
|------|--------|
| 新增檔案數量 | 1 (`ListViewStyles.xaml`) |
| 修改檔案數量 | 3 (`App.xaml`, `LabelPrintView.xaml`, `DataManageView.xaml`) |
| 新增類別/介面 | 0 |
| 新增抽象層 | 0 |

---

## Design Decisions

### D-001: 使用 ControlTemplate 取代 Style.Triggers

**決策**: 為 ListViewItem 與 DataGridRow 定義完整 ControlTemplate

**理由**:
- 現有 `Style.Triggers` 無法正確處理 Selected + Hover 複合狀態
- ControlTemplate 可使用 MultiTrigger 明確定義狀態優先級
- 光條使用 Border 完整填滿 ItemContainer，避免破碎

**來源**: raw_delta_listview.md §5.1

### D-002: 共用色彩 Token 於 ResourceDictionary

**決策**: 在 `ListViewStyles.xaml` 定義色彩常數，ListView 與 DataGrid 共用

**理由**:
- 憲章 III 要求 UI 元件使用共用 ResourceDictionary
- FR-018 要求兩控制項共用色彩定義
- 便於未來統一調整色彩

**來源**: spec.md FR-018、constitution.md III

### D-003: 字體粗細透過 DataTrigger 切換

**決策**: 在 DataTemplate 內使用 DataTrigger 綁定 ListViewItem.IsSelected

**理由**:
- FontWeight 屬性位於 DataTemplate 內的 TextBlock
- 需使用 RelativeSource AncestorType 綁定至 ItemContainer
- 與 raw_delta_button.md 的 DataTrigger 模式一致

**來源**: raw_delta_listview.md §5.1.2

---

## Implementation Phases

### Phase 1: 建立共用樣式檔

1. 建立 `Resources/ListViewStyles.xaml`
2. 定義色彩 Token（ListViewHighlightSelected, ListViewHighlightHover 等）
3. 定義 `ListViewItemStyle` ControlTemplate
4. 定義 `DataGridRowStyle` ControlTemplate
5. 定義 `DataGridCellStyle`（確保 Cell 不覆蓋 Row 背景）

### Phase 2: 整合至 App.xaml

1. 在 `App.xaml` 的 `Application.Resources.MergedDictionaries` 加入 `ListViewStyles.xaml`

### Phase 3: 修改 LabelPrintView

1. 移除現有 `ListView.ItemContainerStyle` 內的 `Style.Triggers`
2. 套用 `{StaticResource ListViewItemStyle}`
3. 更新 DataTemplate 內 TextBlock 字體大小至 16pt
4. 加入 DataTrigger 處理 Selected 狀態字體粗細

### Phase 4: 修改 DataManageView

1. 移除現有 `DataGrid.RowStyle` 內的 `Style.Triggers`
2. 套用 `{StaticResource DataGridRowStyle}`
3. 套用 `{StaticResource DataGridCellStyle}`
4. 確認 `SelectionUnit="FullRow"` 已設定

### Phase 5: 驗收測試

1. 執行視覺驗收清單（spec.md §7）
2. 確認兩分頁視覺一致
3. 確認狀態切換無延遲

---

## File Change Matrix

| 檔案 | 變更類型 | 關聯需求 |
|------|----------|----------|
| `Resources/ListViewStyles.xaml` | 新增 | FR-001~FR-018 |
| `App.xaml` | 修改 | FR-017, FR-018 |
| `Views/LabelPrintView.xaml` | 修改 | FR-001~FR-016, US1, US2, US4 |
| `Views/DataManageView.xaml` | 修改 | FR-001~FR-016, US1, US2, US3, US4 |

---

## Risk Assessment

| 風險 | 影響 | 緩解措施 |
|------|------|----------|
| DataGridCell 覆蓋 Row 背景 | 光條不完整 | 同時設定 DataGridCellStyle，移除 Cell 背景 |
| Trigger 順序錯誤導致 Hover 覆蓋 Selected | 視覺錯誤 | 使用 MultiTrigger 明確條件；Trigger 順序依優先級反向排列 |
| 字體變粗導致版面位移 | 視覺跳動 | 使用 CharacterEllipsis 控制 TextTrimming |

---

## Acceptance Criteria Mapping

| 驗收條件 | 實作項目 | 驗證方式 |
|----------|----------|----------|
| V-001: Selected 光條完整 | ControlTemplate Border | 視覺檢查無破碎 |
| V-002: Hover 光條完整 | MultiTrigger 淡藍背景 | 視覺檢查 |
| V-003: Normal 無光條 | Default Background Transparent | 視覺檢查 |
| V-004: Selected 字體粗體 | DataTrigger FontWeight Bold | 視覺檢查 |
| V-005: Selected 字體白色 | DataTrigger Foreground White | 視覺檢查 |
| V-006: 字體 16pt | TextBlock FontSize="16" | 視覺檢查 |
| I-001: Hover 不覆蓋 Selected | MultiTrigger 條件判斷 | 互動測試 |
| I-002: 點擊響應即時 | 純 XAML 無阻塞 | 互動測試 |
| C-003: 兩頁視覺一致 | 共用 ResourceDictionary | 對照測試 |
