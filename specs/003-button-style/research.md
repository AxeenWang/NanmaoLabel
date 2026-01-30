# Research: 按鈕樣式規範（極簡 Kiosk 風格）

**Branch**: `003-button-style` | **Date**: 2026-01-30

## Status

✅ 完成（無需額外研究）

## Summary

本功能為純 UI 樣式實作，所有技術規格已在原始文件中明確定義，無需外部研究。

## Source Mapping

### 顏色定義

**Decision**: 直接使用 raw_delta_button.md §4 定義的顏色值

**Source**: `.dao/raw_delta_button.md` §4.2-4.3

| Token | Hex | 用途 |
|-------|-----|------|
| BrandPrimary | #1E3A5F | 品牌主色 |
| ButtonDefaultBackground | #1E3A5F | Action 按鈕預設背景 |
| ButtonDefaultForeground | #FFFFFF | Action 按鈕預設文字 |
| ButtonHoverBackground | #2E4A6F | Action 按鈕 Hover 背景 |
| ButtonActiveBackground | #0E2A4F | Action 按鈕 Active 背景 |
| ButtonFocusBorder | #FFFFFF | Action 按鈕 Focus 外框 |
| ButtonDisabledBackground | #E0E0E0 | Disabled 按鈕背景 |
| ButtonDisabledForeground | #A0A0A0 | Disabled 按鈕文字 |
| FeedbackSuccess | #107C10 | 操作成功回饋 |
| FeedbackError | #D13438 | 操作失敗回饋 |

**Alternatives Considered**: 無（文件已明確定義）

---

### 間距規範

**Decision**: 使用 raw_delta_button.md §2 定義的間距值

**Source**: `.dao/raw_delta_button.md` §2.2

| 區域 | 間距 | 說明 |
|------|------|------|
| 建立區內（匯入↔新增） | 8px | 標準間距 |
| 建立區↔危險區 | ≥32px | 明確分隔 |
| 危險區↔確認區 | ≥32px | 明確分隔 |

**Alternatives Considered**: 無（文件已明確定義）

---

### 動畫規格

**Decision**: 使用 raw_delta_button.md §5.3 定義的脈動效果

**Source**: `.dao/raw_delta_button.md` §5.3

| 屬性 | 值 |
|------|-----|
| 光暈顏色 | #FFFFFF（透明度 0.3-0.6 循環） |
| 光暈範圍 | 按鈕外圍 4px |
| 動畫週期 | 1.5 秒 |
| 動畫曲線 | ease-in-out |

**WPF Implementation**: 使用 `Storyboard` + `DoubleAnimation` 實現透明度循環

**Alternatives Considered**:
- CSS Animation → 不適用（非 Web）
- DispatcherTimer → 不建議（效能較差，非 GPU 加速）

---

### 狀態優先序

**Decision**: 使用 raw_delta_button.md §8.1 定義的 7 層優先序

**Source**: `.dao/raw_delta_button.md` §8.1

| 優先序 | 狀態 |
|--------|------|
| 1（最高） | Disabled |
| 2 | Loading |
| 3 | Active |
| 4 | Focus + Hover |
| 5 | Focus |
| 6 | Hover |
| 7（最低） | Default |

**WPF Implementation**: 在 Style.Triggers 中按反向順序定義（最低優先序在前，最高優先序在後），WPF 會以後定義者覆蓋前者

**Alternatives Considered**: 無（WPF Trigger 機制即支援此需求）

---

### 分頁導航按鈕樣式

**Decision**: 使用 raw_delta_button.md §7.1 定義的 Secondary 風格

**Source**: `.dao/raw_delta_button.md` §7.1

| 狀態 | Background | Foreground | Border |
|------|------------|------------|--------|
| Default | #FFFFFF | #333333 | #E0E0E0 1px |
| Hover | #F0F0F0 | #333333 | #D0D0D0 1px |
| Active | #E0E0E0 | #333333 | #C0C0C0 1px |
| Disabled | #F5F5F5 | #C0C0C0 | #E0E0E0 1px |

**Alternatives Considered**: 無（文件已明確定義）

---

## Conclusion

所有技術決策皆直接來自 `.dao/raw_delta_button.md`，符合 Constitution A-1（文件依據原則）。無需外部研究或額外決策。
