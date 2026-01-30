# Quickstart: ListView/DataGrid Item 選取狀態視覺規範

**Branch**: `004-listview-style` | **Date**: 2026-01-30

---

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022/2026 或 VS Code with C# extension
- Windows 10/11（WPF 需 Windows 環境）

---

## Quick Build & Run

```powershell
# 從 WSL 穿透執行（專案開發環境）
powershell.exe -Command "cd '$(wslpath -w .)'; cd NanmaoLabelPOC; dotnet build"

# 執行應用程式
powershell.exe -Command "cd '$(wslpath -w .)'; cd NanmaoLabelPOC; dotnet run"
```

---

## 檔案變更摘要

### 新增檔案

| 檔案 | 用途 |
|------|------|
| `NanmaoLabelPOC/Resources/ListViewStyles.xaml` | ListView/DataGrid 共用樣式定義 |

### 修改檔案

| 檔案 | 變更內容 |
|------|----------|
| `NanmaoLabelPOC/App.xaml` | 引用 `ListViewStyles.xaml` 資源字典 |
| `NanmaoLabelPOC/Views/LabelPrintView.xaml` | 套用 `ListViewItemStyle`、更新字體大小、加入 DataTrigger |
| `NanmaoLabelPOC/Views/DataManageView.xaml` | 套用 `DataGridRowStyle`、`DataGridCellStyle` |

---

## 驗收測試步驟

### 1. 視覺驗收

執行應用程式後，依序檢查：

**標籤列印分頁 (LabelPrintView)**

1. [ ] 點擊 ListView 任一項目 → 高亮藍色光條 (#0078D4) 完整顯示，無破碎
2. [ ] 選中項目文字變為粗體白色
3. [ ] 滑鼠移入未選中項目 → 淡藍色光條 (#E5F3FF)
4. [ ] 滑鼠在選中項目上懸停 → 維持 Selected 樣式不變
5. [ ] 字體大小為 16pt

**資料管理分頁 (DataManageView)**

1. [ ] 點擊 DataGrid 任一列 → 高亮藍色光條完整顯示
2. [ ] 選中列文字變為粗體白色
3. [ ] 滑鼠移入未選中列 → 淡藍色光條
4. [ ] 光條覆蓋整列所有欄位，無斷裂

### 2. 跨分頁一致性驗收

1. [ ] 兩分頁的 Selected 光條顏色一致 (#0078D4)
2. [ ] 兩分頁的 Hover 光條顏色一致 (#E5F3FF)
3. [ ] 兩分頁的字體樣式一致（16pt, Normal→Bold 切換）

### 3. 互動驗收

1. [ ] 連續快速點擊不同項目 → 無閃爍、無延遲
2. [ ] 空清單狀態 → 無光條顯示

---

## 色彩 Token 參考

| Token | 色碼 | 用途 |
|-------|------|------|
| `ListViewHighlightSelected` | `#0078D4` | Selected 光條背景 |
| `ListViewHighlightHover` | `#E5F3FF` | Hover 光條背景 |
| `ListViewTextNormal` | `#333333` | Normal 狀態文字 |
| `ListViewForegroundSelected` | `#FFFFFF` | Selected 狀態文字 |

---

## 常見問題

### Q: 光條顯示破碎？

檢查 ControlTemplate 中的 Border 是否設定：
- `SnapsToDevicePixels="True"`
- `UseLayoutRounding="True"`

### Q: Hover 覆蓋了 Selected 樣式？

確認 Trigger 順序：
1. MultiTrigger (IsMouseOver=True AND IsSelected=False) 在前
2. Trigger (IsSelected=True) 在後（最高優先）

### Q: DataGrid 光條不完整？

確認同時套用了：
- `DataGridRowStyle`
- `DataGridCellStyle`（Cell Background 必須透明）
