# Data Model: 按鈕樣式規範（極簡 Kiosk 風格）

**Branch**: `003-button-style` | **Date**: 2026-01-30

## Overview

本功能為純 UI 樣式實作，無資料庫實體。以下定義 UI 層面的邏輯模型。

---

## Entities

### ColorToken（顏色資源）

**Purpose**: 定義全系統統一的顏色值，確保品牌一致性

**XAML Key Naming**: `{Category}{State}{Property}`

| Key | Type | Value | Description |
|-----|------|-------|-------------|
| BrandPrimary | SolidColorBrush | #1E3A5F | 品牌主色 |
| ButtonDefaultBackground | SolidColorBrush | #1E3A5F | Action 按鈕預設背景 |
| ButtonDefaultForeground | SolidColorBrush | #FFFFFF | Action 按鈕預設文字 |
| ButtonHoverBackground | SolidColorBrush | #2E4A6F | Action 按鈕 Hover 背景 |
| ButtonActiveBackground | SolidColorBrush | #0E2A4F | Action 按鈕 Active 背景 |
| ButtonFocusBorder | SolidColorBrush | #FFFFFF | Action 按鈕 Focus 外框 |
| ButtonDisabledBackground | SolidColorBrush | #E0E0E0 | Disabled 按鈕背景 |
| ButtonDisabledForeground | SolidColorBrush | #A0A0A0 | Disabled 按鈕文字 |
| SecondaryDefaultBackground | SolidColorBrush | #FFFFFF | Secondary 按鈕預設背景 |
| SecondaryDefaultForeground | SolidColorBrush | #333333 | Secondary 按鈕預設文字 |
| SecondaryHoverBackground | SolidColorBrush | #F0F0F0 | Secondary 按鈕 Hover 背景 |
| SecondaryBorder | SolidColorBrush | #E0E0E0 | Secondary 按鈕邊框 |
| FeedbackSuccess | SolidColorBrush | #107C10 | 操作成功回饋 |
| FeedbackError | SolidColorBrush | #D13438 | 操作失敗回饋 |

**Validation**: 所有顏色值必須為有效的 Hex 格式

---

### ButtonStyle（按鈕樣式）

**Purpose**: 定義可重用的按鈕樣式

| StyleKey | TargetType | Description |
|----------|------------|-------------|
| ActionButtonStyle | Button | 主要操作按鈕（深藍底白字） |
| SecondaryButtonStyle | Button | 次要按鈕（白底黑字帶邊框） |
| TabHeaderStyle | TabItem | 頁簽樣式 |

**ActionButtonStyle Triggers**:

| Trigger | Property | Value |
|---------|----------|-------|
| IsEnabled=False | Background | ButtonDisabledBackground |
| IsEnabled=False | Foreground | ButtonDisabledForeground |
| IsMouseOver=True | Background | ButtonHoverBackground |
| IsPressed=True | Background | ButtonActiveBackground |
| IsFocused=True | BorderBrush | ButtonFocusBorder |
| IsFocused=True | BorderThickness | 2 |

---

### ButtonState（按鈕狀態枚舉）

**Purpose**: 定義按鈕的互動狀態（概念模型，實際由 WPF Triggers 處理）

```csharp
public enum ButtonState
{
    Default = 0,    // 無互動
    Hover = 1,      // 滑鼠移入
    Active = 2,     // 滑鼠按下
    Focus = 3,      // 鍵盤聚焦
    Loading = 4,    // 操作進行中
    Disabled = 5    // 停用
}
```

**State Priority** (高優先序覆蓋低優先序):
```
Disabled > Loading > Active > Focus+Hover > Focus > Hover > Default
```

---

### PositionZone（位置分區）

**Purpose**: 定義工具列的邏輯分區（概念模型，實際由 XAML Grid/StackPanel 實現）

| Zone | Position | Buttons | Margin |
|------|----------|---------|--------|
| CreateZone | Left | 匯入、新增 | Right: 32px |
| DangerZone | Center | 刪除 | Left: 32px, Right: 32px |
| ConfirmZone | Right | 儲存 | Left: 32px |

**Layout Implementation**: 使用 `Grid` 三欄配置，中欄為危險區

---

## ViewModel Additions

### DataManageViewModel

新增屬性（對應 FR-016, FR-019）:

| Property | Type | Description | Triggers |
|----------|------|-------------|----------|
| IsDirty | bool | 是否有未儲存變更 | 儲存按鈕脈動光暈 |
| SelectedItem | DataRecord | 當前選取項目 | 刪除按鈕啟用狀態 |

新增方法:

| Method | Description | Implementation |
|--------|-------------|----------------|
| ShowDeleteConfirmation() | 顯示刪除確認對話框 | MessageBox.Show() |

### LabelPrintViewModel

新增屬性（對應 FR-017）:

| Property | Type | Description | Triggers |
|----------|------|-------------|----------|
| IsExporting | bool | 是否正在輸出 PDF | 按鈕 Loading 狀態 |

---

## Relationships

```
App.xaml
    └── MergedDictionaries
            └── Resources/ButtonStyles.xaml
                    ├── ColorTokens (SolidColorBrush)
                    ├── ActionButtonStyle
                    │       └── References ColorTokens
                    ├── SecondaryButtonStyle
                    │       └── References ColorTokens
                    └── TabHeaderStyle
                            └── References ColorTokens

DataManageView.xaml
    └── Uses ActionButtonStyle, SecondaryButtonStyle
            └── Binds to DataManageViewModel (IsDirty, CanDelete, etc.)

LabelPrintView.xaml
    └── Uses ActionButtonStyle, SecondaryButtonStyle
            └── Binds to LabelPrintViewModel (IsExporting)

MainWindow.xaml
    └── Uses TabHeaderStyle
```

---

## Validation Rules

1. **顏色值驗證**: 所有 Hex 值必須為 6 位（#RRGGBB）或 8 位（#AARRGGBB）
2. **間距驗證**: 危險區與相鄰區間距 ≥ 32px
3. **狀態驗證**: Disabled 狀態時忽略所有其他 Trigger
