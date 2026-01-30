# Research: ListView/DataGrid Item 選取狀態視覺規範

**Branch**: `004-listview-style` | **Date**: 2026-01-30

---

## R-001: WPF ListViewItem 狀態優先級處理

**問題**: 如何確保 Selected 狀態視覺優先於 Hover 狀態？

**決策**: 使用 ControlTemplate + MultiTrigger

**理由**:
1. `Style.Triggers` 依宣告順序執行，後宣告者可能覆蓋前者
2. `MultiTrigger` 可設定多重條件（`IsSelected=False AND IsMouseOver=True`）
3. 單一 `Trigger Property="IsSelected"` 放在最後，確保最高優先

**替代方案**:
- VisualStateManager：WPF 支援度較差，主要用於 UWP/WinUI
- 純 Style.Triggers：無法處理複合狀態優先級

**來源**: raw_delta_listview.md §5.1

---

## R-002: DataGrid Cell 與 Row 背景衝突

**問題**: DataGridCell 的 Background 是否會覆蓋 DataGridRow？

**決策**: 同時設定 DataGridCellStyle，移除 Cell 預設背景

**理由**:
1. DataGrid 預設 Cell 有自己的背景色
2. 若僅設定 RowStyle，Cell 背景會遮蓋 Row 光條
3. 設定 `DataGridCell { Background: Transparent }` 可讓 Row 光條穿透

**替代方案**:
- 僅設定 RowStyle：光條會被 Cell 遮蓋，出現破碎
- 使用 CellStyle 設定背景色：違反 FR-018 共用色彩原則

**來源**: raw_delta_listview.md §5.3

---

## R-003: 字體粗細動態切換方式

**問題**: 如何讓 DataTemplate 內的 TextBlock 隨 Selected 狀態切換 FontWeight？

**決策**: 使用 DataTrigger + RelativeSource 綁定

**理由**:
1. FontWeight 屬性位於 DataTemplate 內的 TextBlock
2. TextBlock 無法直接存取 ItemContainer 的 IsSelected 屬性
3. 透過 `RelativeSource AncestorType=ListViewItem` 可向上查找

**程式碼模式**:
```xml
<DataTemplate.Triggers>
    <DataTrigger Binding="{Binding IsSelected,
                 RelativeSource={RelativeSource AncestorType=ListViewItem}}"
                 Value="True">
        <Setter TargetName="MainText" Property="FontWeight" Value="Bold"/>
        <Setter TargetName="MainText" Property="Foreground" Value="White"/>
    </DataTrigger>
</DataTemplate.Triggers>
```

**替代方案**:
- IValueConverter：過度複雜，需額外類別
- Attached Property：過度設計

**來源**: raw_delta_listview.md §5.1.2

---

## R-004: 光條完整性技術要點

**問題**: 如何確保光條無破碎、無裁切？

**決策**: 在 ControlTemplate 中使用完整 Border 作為光條容器

**理由**:
1. 設定 `SnapsToDevicePixels="True"` 避免子像素渲染導致模糊
2. 設定 `UseLayoutRounding="True"` 確保邊界對齊像素格線
3. Border 無圓角 (`CornerRadius="0"`) 避免裁切感

**來源**: raw_delta_listview.md §5.2

---

## R-005: 色彩規範對照

**問題**: 確認各狀態的色彩定義

**決策**: 依 raw_delta_listview.md §3.1 定義

| Token 名稱 | 色碼 | 用途 |
|-----------|------|------|
| `ListViewHighlightSelected` | `#0078D4` | Selected 光條 |
| `ListViewHighlightHover` | `#E5F3FF` | Hover 光條 |
| `ListViewTextNormal` | `#333333` | Normal 字體 |
| `ListViewForegroundSelected` | `#FFFFFF` | Selected 字體 |

**注意**: 這些色碼與 ButtonStyles.xaml 中的 `#0078D4` (Focus border) 一致，維持視覺統一。

**來源**: raw_delta_listview.md §3.1

---

## R-006: 字體大小調整影響

**問題**: 字體從 14pt 調整為 16pt 是否影響版面？

**決策**: 維持現有 ListView Item 高度 50px，可容納 16pt 字體

**理由**:
1. 現有 ItemTemplate 設定 `Height="50"` `MinHeight="50"`
2. 16pt 字體行高約 20px，50px 容器綽綽有餘
3. 使用 `TextTrimming="CharacterEllipsis"` 防止溢出

**來源**: raw_delta_listview.md §3.2、LabelPrintView.xaml:144

---

## Summary

| 研究項目 | 決策 | 風險等級 |
|----------|------|----------|
| 狀態優先級 | ControlTemplate + MultiTrigger | 低 |
| Cell/Row 背景 | DataGridCellStyle 透明背景 | 低 |
| 字體粗細切換 | DataTrigger + RelativeSource | 低 |
| 光條完整性 | SnapsToDevicePixels + UseLayoutRounding | 低 |
| 色彩規範 | 依 raw_delta 定義 Token | 無 |
| 字體大小 | 16pt，容器足夠 | 低 |

**結論**: 所有技術問題已解決，無 NEEDS CLARIFICATION 項目，可進入 Phase 1 設計。
