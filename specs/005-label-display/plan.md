# Implementation Plan: ListView 選取後標籤預覽顯示

**Branch**: `005-label-display` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-label-display/spec.md`
**Type**: Bug Fix / 功能補齊

## Summary

修復 ListView 選取項目後，標籤預覽區未顯示對應標籤內容的問題。根因為 WPF Canvas 渲染層未實作 — `LabelPrintView.xaml.cs` 未訂閱 `ViewModel.PreviewCommands` 屬性變更，也未執行將 `RenderCommand` 集合繪製至 `PreviewCanvas` 的邏輯。

**技術方案**：在 `LabelPrintView.xaml.cs` 中實作 `PropertyChanged` 訂閱機制，當 `PreviewCommands` 變更時，遍歷指令集合並依類型（Text、Barcode、QRCode）繪製對應的 WPF 元素至 Canvas。

## Technical Context

**Language/Version**: C# 12 / .NET 8 LTS
**Primary Dependencies**: WPF、CommunityToolkit.Mvvm 8.x、ZXing.Net（條碼生成）
**Storage**: N/A（本功能不涉及資料儲存）
**Testing**: xUnit + FluentAssertions（`NanmaoLabelPOC.Tests`）
**Target Platform**: Windows 10/11 (x64)
**Project Type**: WPF Desktop Application (Single Project)
**Performance Goals**: 渲染回應時間 ≤ 100ms [ref: 憲章 IV、FR-001]
**Constraints**: 預覽與 PDF 版面誤差 ≤ ±0.5mm [ref: FR-008]
**Scale/Scope**: 單一 View Code-behind 修改，約 100-150 行新增程式碼

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Check ✅

| 原則 | 檢查項目 | 狀態 | 備註 |
|------|----------|------|------|
| I. 程式碼品質 | 遵循 .editorconfig | ✅ | 新增程式碼將遵循 |
| I. 程式碼品質 | 單一職責原則 | ✅ | 渲染邏輯集中於一個方法 |
| II. 測試標準 | 核心邏輯有單元測試 | ⚠️ | 渲染邏輯為 View 層，需整合測試驗證 |
| II. 測試標準 | Bug 修復需重現測試 | ✅ | 將新增手動測試案例 |
| III. UX 一致性 | 繁體中文錯誤訊息 | ✅ | 「標籤渲染失敗」已定義於 spec |
| IV. 效能要求 | 回應時間 ≤ 100ms | ✅ | 需實測驗證 |
| A-1. 文件依據 | 僅依據指定文件 | ✅ | 依據 raw_spec §8.4、raw_delta_label_display.md |
| A-2. 禁止預設設計 | 無額外抽象層 | ✅ | 直接在 View 層實作 |
| A-4. 需求追溯 | 可追溯至需求 | ✅ | FR-001 至 FR-008 |
| A-5. 格式遵循 | 遵循模板結構 | ✅ | 本文件依 plan-template.md |

### 開發約束

| 項目 | 要求 | 狀態 |
|------|------|------|
| 檔案編碼 | .cs: UTF-8 with BOM + CRLF | ✅ |
| 編譯警告 | 禁止提交含警告程式碼 | ✅ |
| 空 catch 區塊 | 禁止（必須記錄或重新拋出）| ✅ |

## Project Structure

### Documentation (this feature)

```text
specs/005-label-display/
├── spec.md              # 功能規格（已完成）
├── plan.md              # 本文件
├── research.md          # Phase 0: 技術研究（本功能省略，見下方說明）
├── data-model.md        # Phase 1: 資料模型（本功能 N/A）
├── quickstart.md        # Phase 1: 快速上手（本功能 N/A）
└── checklists/
    └── requirements.md  # 規格品質檢查（已完成）
```

### Source Code (repository root)

```text
NanmaoLabelPOC/
├── Views/
│   ├── LabelPrintView.xaml       # 預覽區 Canvas 已定義（無需修改）
│   └── LabelPrintView.xaml.cs    # ⬅️ 主要修改檔案：新增渲染邏輯
├── ViewModels/
│   └── LabelPrintViewModel.cs    # PreviewCommands 已實作（無需修改）
└── Services/
    ├── ILabelRenderer.cs         # RenderCommand 定義（無需修改）
    ├── LabelRenderer.cs          # 渲染指令產生（無需修改）
    ├── IBarcodeGenerator.cs      # 條碼生成介面（無需修改）
    └── UnitConverter.cs          # 座標轉換工具（可複用）

NanmaoLabelPOC.Tests/
└── Views/
    └── LabelPrintViewRenderingTests.cs  # ⬅️ 新增：渲染整合測試（可選）
```

**Structure Decision**: 本功能為 Bug Fix，僅修改 `LabelPrintView.xaml.cs`，不新增類別或介面，符合憲章 A-2 禁止預設設計原則。

## Complexity Tracking

> 本功能無違反憲章情況，此區塊留空。

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

---

## Phase 0: Research

由於本功能為明確的 Bug Fix，技術方案已在 `raw_delta_label_display.md` §7 明確定義，**無需額外研究**。

### 技術決策摘要

| 決策項目 | 選擇 | 依據 |
|----------|------|------|
| 渲染觸發機制 | `PropertyChanged` 訂閱 | raw_delta §2.2 事件流程圖 |
| Canvas 渲染方式 | 直接操作 `Canvas.Children` | raw_delta §3.2、§7.2 |
| 座標轉換 | `px = mm × 4`（4:1 縮放比）| raw_delta §3.3、spec FR-007 |
| 字型縮放 | `previewFontSize = specFontSize × 4 / 2.83465` | raw_delta §3.3 |
| 條碼/QR Code 圖片 | 呼叫 `IBarcodeGenerator` 產生 | 既有實作 |

---

## Phase 1: Design

### 1.1 Data Model

**N/A** — 本功能不涉及新資料模型。使用既有 `RenderCommand` 類別（定義於 `ILabelRenderer.cs`）。

### 1.2 API Contracts

**N/A** — 本功能不涉及 API 或服務介面變更。

### 1.3 Implementation Design

#### 1.3.1 渲染流程

```text
[ViewModel.PreviewCommands 變更]
            │
            ▼
[LabelPrintView 訂閱 PropertyChanged]
            │
            ▼
[RenderPreview() 方法]
            │
    ┌───────┴───────┐
    ▼               ▼
[HasCommands?]   [No Commands]
    │               │
    ▼               ▼
[清空 Canvas]    [顯示空白提示]
    │
    ▼
[遍歷 RenderCommand]
    │
    ├─ TextCommand ──▶ 建立 TextBlock
    ├─ BarcodeCommand ▶ 呼叫 IBarcodeGenerator → 建立 Image
    └─ QRCodeCommand ─▶ 呼叫 IBarcodeGenerator → 建立 Image
            │
            ▼
[設定 Canvas.Left / Canvas.Top]
            │
            ▼
[加入 Canvas.Children]
```

#### 1.3.2 關鍵實作細節

| 項目 | 實作方式 | 參考 |
|------|----------|------|
| PropertyChanged 訂閱 | `DataContextChanged` 事件中取得 ViewModel，訂閱 `PropertyChanged` | raw_delta §7.2 |
| Canvas 尺寸 | 固定 400px × 240px（對應 100mm × 60mm，4:1 縮放）| raw_delta §3.3 |
| 座標轉換 | `Canvas.SetLeft(element, command.X * 4)` | spec FR-007 |
| 字型轉換 | `fontSize = command.FontSize * 4 / 2.83465` | spec FR-007 |
| 條碼圖片 | `BitmapImage` 載入 `IBarcodeGenerator.GenerateCode128()` 結果 | 既有服務 |
| QR Code 圖片 | `BitmapImage` 載入 `IBarcodeGenerator.GenerateQRCode()` 結果 | 既有服務 |
| 錯誤處理 | try-catch 包覆渲染邏輯，失敗時顯示錯誤提示 | spec FR-006 |

#### 1.3.3 需取得 IBarcodeGenerator 實例

`LabelPrintView.xaml.cs` 需要呼叫 `IBarcodeGenerator` 來產生條碼圖片。有兩種方式：

**方案 A（推薦）**：ViewModel 提供預渲染的圖片

- `PreviewCommands` 中的 `BarcodeCommand` / `QRCodeCommand` 包含已產生的 `byte[]` 圖片資料
- View 層僅負責將 `byte[]` 轉為 `BitmapImage` 顯示
- **優點**：View 層不需依賴 Service，符合 MVVM 精神
- **狀態**：需檢查 `RenderCommand` 是否已包含圖片資料

**方案 B**：View 層透過 DI 取得 Service

- 在 `DataContextChanged` 時從 `App.Current.Services` 取得 `IBarcodeGenerator`
- **缺點**：View 層直接依賴 Service，違反 MVVM 分層

**決策**：經檢視 `RenderCommand` 類別，僅包含 `Content` 字串，不含預渲染圖片。因此採用 **方案 B**，但將條碼生成邏輯封裝於 Helper 方法中，保持程式碼清晰。

---

## Post-Design Constitution Re-Check ✅

| 原則 | 檢查項目 | 狀態 | 備註 |
|------|----------|------|------|
| I. 程式碼品質 | 單一職責 | ✅ | 渲染邏輯集中於 `RenderPreview()` |
| I. 程式碼品質 | 無魔術數字 | ✅ | 4:1 縮放比定義為常數 |
| II. 測試標準 | 可獨立驗證 | ✅ | 手動測試案例定義於 spec AC-01 至 AC-06 |
| III. UX 一致性 | 錯誤訊息友善 | ✅ | 「標籤渲染失敗」繁體中文 |
| IV. 效能要求 | ≤ 100ms | ✅ | 同步渲染，預期 < 50ms |
| A-4. 需求追溯 | 所有程式碼可追溯 | ✅ | 見下方需求對應表 |

### 需求對應表

| 需求 | 實作位置 | 驗證方式 |
|------|----------|----------|
| FR-001 | `RenderPreview()` | 手動測試 AC-01 |
| FR-002 | `OnDataContextChanged()` 訂閱 | 程式碼審查 |
| FR-003 | `RenderTextCommand()`, `RenderBarcodeCommand()`, `RenderQRCodeCommand()` | 目視檢查 AC-02 |
| FR-004 | `PropertyChanged` 連鎖通知 | 手動測試 AC-03 |
| FR-005 | XAML Visibility 綁定（既有）| 手動測試 AC-05 |
| FR-006 | try-catch 包覆 | 異常測試 |
| FR-007 | 常數 `ScaleFactor = 4.0` | 程式碼審查 |
| FR-008 | 與 PDF 輸出對照 | 手動測試 AC-06 |

---

## Quickstart

本功能為 View 層 Bug Fix，無需獨立 quickstart 文件。

### 驗證步驟

1. 編譯並執行程式
2. 匯入範例 Excel 資料
3. 切換至「標籤列印」分頁
4. 單擊 ListView 任一項目
5. 確認左側預覽區顯示標籤內容（含文字、條碼、QR Code）
6. 切換標籤格式下拉選單，確認預覽區即時更新
7. 輸出 PDF，對照預覽區與 PDF 版面

---

## Next Steps

執行 `/speckit.tasks` 產生任務清單，進入實作階段。
