# ListView / DataGrid Item 視覺與互動行為調整規範

| 項目 | 內容 |
|------|------|
| 文件版本 | v1.0 |
| 建立日期 | 2026-01-30 |
| 狀態 | **待實作** |
| 相依文件 | raw_spec.md 8.4、8.5、8.6 |

---

## 1. 背景說明

### 1.1 問題描述

目前 ListView 與 DataGrid 在 Item 被選中（Selected）時，顯示的光條（Highlight Bar）存在以下瑕疵：

| 問題編號 | 問題描述 | 影響 |
|----------|----------|------|
| LV-001 | 光條顯示破碎、不連續 | 視覺完整性受損 |
| LV-002 | Selected 與 Hover 狀態衝突 | 操作體驗混亂 |
| LV-003 | 字體未隨狀態變化 | 焦點辨識度不足 |
| LV-004 | 使用簡單 Style.Triggers 無法正確處理狀態優先級 | 技術架構問題 |

### 1.2 現況分析

目前實作方式（`LabelPrintView.xaml:192-207`、`DataManageView.xaml:175-187`）：

```xml
<Style.Triggers>
    <Trigger Property="IsSelected" Value="True">
        <Setter Property="Background" Value="#0078D4"/>
        <Setter Property="Foreground" Value="White"/>
    </Trigger>
    <Trigger Property="IsMouseOver" Value="True">
        <Setter Property="Background" Value="#F0F0F0"/>
    </Trigger>
</Style.Triggers>
```

**技術缺陷：**

| 項目 | 說明 |
|------|------|
| Trigger 順序問題 | WPF Trigger 依宣告順序執行，後者可能覆蓋前者 |
| 無 ControlTemplate | 未完整定義 ItemContainer 模板，導致光條區域不完整 |
| 缺乏多重狀態處理 | 無法處理 Selected + Hover 同時存在的情況 |

### 1.3 調整目標

- **視覺一致性**：光條完整顯示，無破碎、裁切
- **操作可辨識度**：Normal / Hover / Selected 三狀態明確區分
- **文字可讀性**：字體大小、粗細、顏色隨狀態變化
- **技術正確性**：使用 MultiTrigger 或 ControlTemplate 確保狀態優先級

---

## 2. 設計原則

### 2.1 狀態分層優先級

```
優先級（由高到低）：
┌─────────────────────────────────────────┐
│  1. Selected（被選中）                  │  ← 最高優先
├─────────────────────────────────────────┤
│  2. Hover（滑鼠懸停，未選中）           │
├─────────────────────────────────────────┤
│  3. Normal（未選中、未 Hover）          │  ← 最低優先
└─────────────────────────────────────────┘
```

### 2.2 核心規則

| 規則編號 | 規則內容 |
|----------|----------|
| R-001 | Selected 狀態視覺優先級最高，不得被 Hover 覆蓋 |
| R-002 | 光條必須完整顯示，與 Item 可點擊區域一致 |
| R-003 | 狀態切換時視覺回饋即時（無延遲） |
| R-004 | 字體樣式隨狀態變化，增強焦點辨識 |

---

## 3. 視覺規範

### 3.1 色彩定義

| 色彩變數名稱 | 色碼 | 用途 |
|--------------|------|------|
| `ListViewHighlightSelected` | `#0078D4` | Selected 狀態光條（高亮藍） |
| `ListViewHighlightHover` | `#E5F3FF` | Hover 狀態光條（淡藍） |
| `ListViewTextSelected` | `#0066CC` | Selected 狀態字體顏色（鮮藍） |
| `ListViewTextNormal` | `#333333` | Normal 狀態字體顏色 |
| `ListViewForegroundSelected` | `#FFFFFF` | Selected 狀態前景色（白） |

### 3.2 字體設定

| 狀態 | 字體大小 | 字體粗細 | 字體顏色 |
|------|----------|----------|----------|
| Normal | 15pt | Normal | `#333333` |
| Hover（未選中） | 15pt | Normal | `#333333` |
| Selected | 15pt | **Bold** | `#0066CC`（鮮藍） |

> **變更說明**：字體大小由 14pt 調整為 15pt，提升可讀性。

### 3.3 光條規範

#### 3.3.1 Normal 狀態

| 項目 | 設定值 |
|------|--------|
| 光條顯示 | **不顯示** |
| 背景色 | Transparent |
| 字體粗細 | Normal |
| 字體顏色 | `#333333` |

#### 3.3.2 Hover 狀態（未選中）

| 項目 | 設定值 |
|------|--------|
| 光條顯示 | 顯示 |
| 光條顏色 | `#E5F3FF`（淡藍） |
| 光條範圍 | 完整填滿 Item 區域 |
| 字體粗細 | Normal |
| 字體顏色 | `#333333` |

#### 3.3.3 Selected 狀態

| 項目 | 設定值 |
|------|--------|
| 光條顯示 | 顯示 |
| 光條顏色 | `#0078D4`（高亮藍） |
| 光條範圍 | 完整填滿 Item 區域，無裁切、無破碎 |
| 字體粗細 | **Bold** |
| 字體顏色 | `#FFFFFF`（白）或 `#0066CC`（鮮藍）* |

> *註：若光條為深色背景，字體使用白色；若需字體醒目但保持可讀，可改用鮮藍色配淺色光條。依實際視覺效果擇一。

#### 3.3.4 Selected + Hover 狀態（複合）

| 項目 | 設定值 |
|------|--------|
| 視覺表現 | **以 Selected 為準** |
| 光條顏色 | `#0078D4`（維持 Selected） |
| 字體粗細 | Bold |
| 字體顏色 | 維持 Selected 字體顏色 |

---

## 4. 互動行為規範

### 4.1 狀態轉換矩陣

| 當前狀態 | 觸發事件 | 轉換至 | 視覺變化 |
|----------|----------|--------|----------|
| Normal | 滑鼠移入 | Hover | 顯示淡藍光條 |
| Hover | 滑鼠移出 | Normal | 光條消失 |
| Hover | 點擊 | Selected | 光條變高亮藍、字體加粗 |
| Selected | 點擊其他項目 | Normal | 光條消失、字體還原 |
| Selected | 滑鼠移入 | Selected + Hover | **無變化**（維持 Selected） |
| Selected | 滑鼠移出 | Selected | **無變化**（維持 Selected） |

### 4.2 點擊區域規範

```
┌──────────────────────────────────────────────────┐
│  光條區域 ＝ Item 可點擊區域 ＝ Item 視覺區域    │
│                                                  │
│  禁止：                                          │
│  - 光條小於點擊區域（誤導使用者）                │
│  - 光條超出點擊區域（視覺誤導）                  │
│  - 光條有缺角、斷裂（破碎）                      │
└──────────────────────────────────────────────────┘
```

---

## 5. WPF 技術實作指引

### 5.1 建議方案：使用 ControlTemplate

為確保光條完整且狀態優先級正確，建議為 `ListViewItem` 定義完整的 `ControlTemplate`。

#### 5.1.1 ListViewItem ControlTemplate 結構

```xml
<Style x:Key="ListViewItemStyle" TargetType="ListViewItem">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ListViewItem">
                <!-- 光條背景層 -->
                <Border x:Name="Bd"
                        Background="Transparent"
                        BorderThickness="0"
                        Padding="{TemplateBinding Padding}"
                        SnapsToDevicePixels="True">
                    <ContentPresenter
                        HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                        VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                        SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}"/>
                </Border>

                <ControlTemplate.Triggers>
                    <!-- Hover 狀態（未選中時） -->
                    <MultiTrigger>
                        <MultiTrigger.Conditions>
                            <Condition Property="IsMouseOver" Value="True"/>
                            <Condition Property="IsSelected" Value="False"/>
                        </MultiTrigger.Conditions>
                        <Setter TargetName="Bd" Property="Background" Value="#E5F3FF"/>
                    </MultiTrigger>

                    <!-- Selected 狀態（最高優先） -->
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="Bd" Property="Background" Value="#0078D4"/>
                        <Setter Property="Foreground" Value="White"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### 5.1.2 字體粗細動態切換

由於 `FontWeight` 通常設定於 `DataTemplate` 內的 `TextBlock`，需使用以下方式之一：

**方案 A：使用 DataTrigger（推薦）**

```xml
<DataTemplate>
    <Grid>
        <TextBlock x:Name="MainText"
                   Text="{Binding Ogb19}"
                   FontSize="15"
                   FontWeight="Normal"/>
    </Grid>
    <DataTemplate.Triggers>
        <DataTrigger Binding="{Binding IsSelected,
                              RelativeSource={RelativeSource AncestorType=ListViewItem}}"
                     Value="True">
            <Setter TargetName="MainText" Property="FontWeight" Value="Bold"/>
            <Setter TargetName="MainText" Property="Foreground" Value="White"/>
        </DataTrigger>
    </DataTemplate.Triggers>
</DataTemplate>
```

**方案 B：繼承 Foreground 並使用 Converter**

若需更複雜的邏輯，可建立 `IsSelectedToFontWeightConverter`。

### 5.2 關鍵實作要點

| 要點 | 說明 |
|------|------|
| SnapsToDevicePixels | 設為 `True`，避免子像素渲染導致模糊 |
| UseLayoutRounding | 設為 `True`，確保邊界對齊像素格線 |
| Border 無圓角 | 光條使用 `CornerRadius="0"`，避免裁切感 |
| Padding 一致 | `ListViewItem` 的 `Padding` 需與內容對齊 |

### 5.3 DataGrid 適用性

DataGrid 的 `DataGridRow` 樣式邏輯相同，但需注意：

- `DataGridCell` 的背景可能覆蓋 `DataGridRow`
- 建議同時設定 `DataGridRow` 和 `DataGridCell` 的樣式
- 使用 `SelectionUnit="FullRow"` 確保整列選取

---

## 6. 影響範圍

### 6.1 受影響檔案

| 檔案路徑 | 修改範圍 |
|----------|----------|
| `Views/LabelPrintView.xaml` | ListView.ItemContainerStyle（第 192-208 行） |
| `Views/DataManageView.xaml` | DataGrid.RowStyle（第 175-187 行） |
| `Resources/ListViewStyles.xaml`（新增） | 共用樣式定義 |
| `App.xaml` | 引用共用樣式資源 |

### 6.2 不影響範圍

| 項目 | 說明 |
|------|------|
| ViewModel | 純視覺變更，不影響邏輯 |
| Model | 無變更 |
| Services | 無變更 |
| 功能行為 | 選取、分頁、匯出等功能不變 |

---

## 7. 驗收檢查清單

### 7.1 視覺驗收

| 編號 | 檢查項目 | 驗收標準 | 通過 |
|------|----------|----------|------|
| V-001 | Selected 光條完整 | 無破碎、無裁切、無缺角 | [ ] |
| V-002 | Hover 光條完整 | 淡藍色，與 Selected 明顯區分 | [ ] |
| V-003 | Normal 無光條 | 背景透明 | [ ] |
| V-004 | Selected 字體粗體 | FontWeight = Bold | [ ] |
| V-005 | Selected 字體顏色 | 鮮藍或白色（依實作） | [ ] |
| V-006 | 字體大小提升 | 15pt（相較舊版 14pt） | [ ] |
| V-007 | 光條高度一致 | 與 Item 視覺區塊等高 | [ ] |

### 7.2 互動驗收

| 編號 | 檢查項目 | 驗收標準 | 通過 |
|------|----------|----------|------|
| I-001 | Hover 不覆蓋 Selected | Selected Item 上懸停無視覺變化 | [ ] |
| I-002 | 點擊響應即時 | 無可感知延遲 | [ ] |
| I-003 | 多次切換穩定 | 連續點擊不同項目，視覺穩定 | [ ] |
| I-004 | 滑鼠移出 Hover 消失 | 非 Selected 項目移出後光條消失 | [ ] |

### 7.3 技術驗收

| 編號 | 檢查項目 | 驗收標準 | 通過 |
|------|----------|----------|------|
| T-001 | 無 XAML 警告 | 編譯無 XAML 相關警告 | [ ] |
| T-002 | 無 Binding 錯誤 | Output 視窗無 Binding 錯誤 | [ ] |
| T-003 | ListView 功能正常 | 選取、雙擊、分頁功能不變 | [ ] |
| T-004 | DataGrid 功能正常 | 選取、編輯、排序功能不變 | [ ] |

### 7.4 跨頁驗收

| 編號 | 檢查項目 | 驗收標準 | 通過 |
|------|----------|----------|------|
| C-001 | 標籤列印分頁 ListView | 視覺符合規範 | [ ] |
| C-002 | 資料管理分頁 DataGrid | 視覺符合規範 | [ ] |
| C-003 | 兩頁視覺一致 | 光條顏色、字體樣式統一 | [ ] |

---

## 8. 附錄

### 8.1 色彩對照表（視覺化）

```
┌─────────────────────────────────────────────────────────────┐
│                       狀態色彩對照                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Normal:    [ 透明 ]  文字 #333333                          │
│             ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░                  │
│                                                             │
│  Hover:     [ #E5F3FF ]  文字 #333333                       │
│             ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒ (淡藍)          │
│                                                             │
│  Selected:  [ #0078D4 ]  文字 #FFFFFF (Bold)                │
│             ████████████████████████████████ (高亮藍)       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 相關參考

| 參考文件 | 章節 |
|----------|------|
| raw_spec.md | 8.4 標籤列印分頁、8.5 資料管理分頁、8.6 尺寸規範 |
| raw_delta_button.md | 按鈕樣式規範（參考 Focus 狀態處理） |
| Microsoft Fluent Design | ListView Item States |

---

## 9. 版本歷程

| 版本 | 日期 | 修改內容 | 作者 |
|------|------|----------|------|
| v1.0 | 2026-01-30 | 初版，定義 ListView/DataGrid 視覺與互動規範 | Claude |

---

## 10. 備註

- 本規範為「視覺與互動行為定義」，實作細節由工程端依 WPF 框架調整
- 若需調整色彩值，請同步更新本文件與程式碼中的色彩常數
- DataGrid 與 ListView 應共用色彩定義，確保視覺一致性
