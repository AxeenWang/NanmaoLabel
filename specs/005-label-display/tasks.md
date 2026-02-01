# Tasks: ListView 選取後標籤預覽顯示

**Input**: Design documents from `/specs/005-label-display/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)
**Type**: Bug Fix / 功能補齊

**Tests**: 本功能為 View 層 Bug Fix，渲染邏輯難以單元測試，採用手動測試驗證（spec AC-01 至 AC-06）。

**Organization**: 任務依 User Story 分組，支援獨立實作與測試。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行（不同檔案、無相依性）
- **[Story]**: 所屬 User Story（例如 US1、US2、US3）
- 描述中包含精確檔案路徑

## Path Conventions

本專案為 WPF Desktop Application：
- **主程式**: `NanmaoLabelPOC/`
- **測試專案**: `NanmaoLabelPOC.Tests/`

---

## Phase 1: Setup（準備工作）

**Purpose**: 確認既有架構與取得必要服務實例

- [x] T001 確認 `PreviewCanvas` 已定義於 `NanmaoLabelPOC/Views/LabelPrintView.xaml`（x:Name="PreviewCanvas"）
  - ✅ Canvas 定義於 LabelPrintView.xaml:57-59，尺寸 400×240px 符合規格
- [x] T002 確認 `IBarcodeGenerator` 已註冊於 `NanmaoLabelPOC/App.xaml.cs` DI 容器
  - ⚠️ 使用 Poor Man's DI，需在 Phase 2 新增靜態 Services 屬性供 View 層存取

---

## Phase 2: Foundational（基礎建設）

**Purpose**: 定義常數與建立渲染基礎架構

**⚠️ CRITICAL**: 此階段完成後方可開始 User Story 實作

- [x] T003 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 新增渲染常數區塊：
  - `ScaleFactor = 4.0`（4:1 縮放比 = 400px / 100mm）
  - `PtToPxFactor = 4.0 / 2.83465`（pt 轉 px 縮放係數）
  - `PreviewFontFamily = "Microsoft JhengHei"`
  - ✅ 已實作於 LabelPrintView.xaml.cs #region Constants

- [x] T004 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 新增 `IBarcodeGenerator` 欄位與初始化邏輯（於 `OnDataContextChanged` 中從 `App.Current.Services` 取得）
  - ✅ 已新增 `_barcodeGenerator` 欄位
  - ✅ 已在 App.xaml.cs 新增 `App.BarcodeGenerator` 靜態屬性

- [x] T005 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 新增 `SubscribeToViewModel()` 方法：
  - 於 `OnDataContextChanged` 中呼叫
  - 訂閱 ViewModel 的 `PropertyChanged` 事件
  - 當屬性名稱為 `PreviewCommands` 時呼叫 `RenderPreview()`
  - ✅ 已實作 `SubscribeToViewModel()` 與 `OnViewModelPropertyChanged()`

**Checkpoint**: 基礎架構就緒，可開始實作各 User Story 渲染邏輯

---

## Phase 3: User Story 1 - 單擊 ListView 項目即時預覽標籤 (Priority: P1) 🎯 MVP

**Goal**: 使用者單擊 ListView 項目後，預覽區顯示該筆資料的完整標籤

**Independent Test**: 匯入範例資料 → 單擊 ListView 任一項目 → 確認預覽區顯示標籤內容

### Implementation for User Story 1

- [x] T006 [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `RenderPreview()` 主方法：
  - 取得 ViewModel.PreviewCommands
  - 若為 null 或空，清空 Canvas 並返回
  - 清空 `PreviewCanvas.Children`
  - 呼叫 `RenderCommands(commands)` 遍歷渲染
  - ✅ 已實作，含 try-catch 錯誤處理

- [x] T007 [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `RenderCommands(IReadOnlyList<RenderCommand>)` 方法：
  - 遍歷 RenderCommand 集合
  - 依 `command.Skip` 判斷是否略過
  - 依 `command.CommandType` 分派至對應 Render 方法
  - ✅ 已實作 switch 分派邏輯

- [x] T008 [P] [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `RenderTextCommand(RenderCommand)` 方法：
  - 建立 `TextBlock` 元素
  - 設定 `Text = command.Content`
  - 設定 `FontFamily = PreviewFontFamily`
  - 設定 `FontSize = command.FontSize * PtToPxFactor`
  - 設定 `FontWeight = command.IsBold ? FontWeights.Bold : FontWeights.Normal`
  - 設定 `TextAlignment` 依據 `command.Alignment`
  - 設定 `Canvas.SetLeft(textBlock, command.X * ScaleFactor)`
  - 設定 `Canvas.SetTop(textBlock, command.Y * ScaleFactor)`
  - 設定 `Width = command.Width * ScaleFactor`
  - 設定 `Height = command.Height * ScaleFactor`
  - 設定 `TextTrimming = TextTrimming.CharacterEllipsis`
  - 加入 `PreviewCanvas.Children`
  - ✅ 已實作，含 ConvertAlignment 輔助方法

- [x] T009 [P] [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `RenderBarcodeCommand(RenderCommand)` 方法：
  - 呼叫 `_barcodeGenerator.GenerateCode128(command.Content, heightMm)` 取得 byte[]
  - 將 byte[] 轉為 `BitmapImage`
  - 建立 `Image` 元素並設定 `Source`
  - 計算條碼高度（預留 3mm 給下方文字）
  - 設定座標與尺寸
  - 加入 `PreviewCanvas.Children`
  - ✅ 已實作，含條碼下方文字渲染

- [x] T010 [P] [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `RenderQRCodeCommand(RenderCommand)` 方法：
  - 呼叫 `_barcodeGenerator.GenerateQRCode(command.Content, sizeMm)` 取得 byte[]
  - 將 byte[] 轉為 `BitmapImage`
  - 建立 `Image` 元素並設定 `Source`
  - 設定座標與尺寸（正方形）
  - 加入 `PreviewCanvas.Children`
  - ✅ 已實作

- [x] T011 [P] [US1] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 實作 `ByteArrayToBitmapImage(byte[])` Helper 方法：
  - 建立 `BitmapImage`
  - 使用 `MemoryStream` 載入 byte[]
  - 返回 `BitmapImage`
  - ✅ 已實作，含 Freeze() 確保跨執行緒安全

**Checkpoint**: User Story 1 完成 — 單擊 ListView 項目可顯示標籤預覽

---

## Phase 4: User Story 2 - 切換標籤格式即時更新預覽 (Priority: P2)

**Goal**: 使用者切換標籤格式下拉選單時，預覽區以新格式重新渲染

**Independent Test**: 選取 ListView 項目 → 切換標籤格式 → 確認預覽區更新為新格式

### Implementation for User Story 2

- [x] T012 [US2] 驗證 `SelectedTemplate` 變更時 `PreviewCommands` 連鎖通知正確觸發（檢查 `LabelPrintViewModel.cs` 中的 `[NotifyPropertyChangedFor]` 屬性）
  - ✅ 已驗證：`_selectedTemplate` 欄位使用 `[NotifyPropertyChangedFor(nameof(PreviewCommands))]` 屬性
  - ✅ 連鎖通知機制正確：SelectedTemplate 變更 → PreviewCommands PropertyChanged

- [x] T013 [US2] 於 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 確認 `SubscribeToViewModel()` 已正確處理格式切換（無需額外程式碼，因 `PreviewCommands` 已連鎖通知）
  - ✅ 已驗證：`SubscribeToViewModel()` 正確訂閱 `PropertyChanged` 事件
  - ✅ 已驗證：`OnViewModelPropertyChanged()` 監聽 `PreviewCommands` 並呼叫 `RenderPreview()`
  - ✅ 無需額外程式碼修改

**Checkpoint**: User Story 2 完成 — 切換格式可即時更新預覽

---

## Phase 5: User Story 3 - 空白狀態正確顯示提示 (Priority: P3)

**Goal**: 無選取項目或渲染失敗時顯示適當提示訊息

**Independent Test**: 啟動程式且無資料 → 確認顯示引導提示；強制渲染失敗 → 確認顯示錯誤提示

### Implementation for User Story 3

- [ ] T014 [US3] 確認 XAML 中空白狀態提示已透過 `HasSelectedRecord` Visibility 綁定正確顯示（已實作於既有 `LabelPrintView.xaml`，無需修改）

- [ ] T015 [US3] 在 `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` 的 `RenderPreview()` 方法中加入 try-catch 包覆：
  - catch 區塊顯示錯誤提示 TextBlock（「⚠️ 標籤渲染失敗」）
  - 使用 `Debug.WriteLine()` 記錄錯誤詳情

**Checkpoint**: User Story 3 完成 — 所有邊界情況皆顯示適當提示

---

## Phase 6: Polish & 驗收

**Purpose**: 最終驗證與程式碼品質確認

- [ ] T016 執行 `powershell.exe -Command "cd '$(wslpath -w .)'; dotnet build NanmaoLabelPOC"` 確認編譯成功且無警告

- [ ] T017 執行手動測試案例 AC-01：單擊 ListView 任一項目，確認預覽區顯示標籤

- [ ] T018 執行手動測試案例 AC-02：確認標籤預覽包含所有元素（文字、條碼、QR Code）

- [ ] T019 執行手動測試案例 AC-03：切換標籤格式（QW075551-1 ↔ QW075551-2），確認預覽區即時更新

- [ ] T020 執行手動測試案例 AC-04：切換不同 ListView 項目，確認預覽區正確更新

- [ ] T021 執行手動測試案例 AC-05：確認無選取項目時顯示空白狀態提示

- [ ] T022 執行手動測試案例 AC-06：輸出 PDF，對照預覽區與 PDF 版面一致性（誤差 ≤ ±0.5mm）

- [ ] T023 程式碼審查：確認無魔術數字、無空 catch 區塊、遵循命名規範

---

## Dependencies & Execution Order

### Phase Dependencies

```text
Phase 1 (Setup)
     │
     ▼
Phase 2 (Foundational) ─────────────────────────────────┐
     │                                                   │
     ▼                                                   ▼
Phase 3 (US1: 單擊預覽) ──▶ Phase 4 (US2: 格式切換) ──▶ Phase 5 (US3: 空白狀態)
                                                         │
                                                         ▼
                                                   Phase 6 (驗收)
```

### User Story Dependencies

| User Story | 相依性 | 說明 |
|------------|--------|------|
| US1 (P1) | Phase 2 完成 | 核心渲染邏輯，無其他 US 相依 |
| US2 (P2) | US1 完成 | 依賴 US1 的渲染邏輯 |
| US3 (P3) | US1 完成 | 依賴 US1 的渲染邏輯，加入錯誤處理 |

### Parallel Opportunities

```text
Phase 3 (US1) 內部可平行：
- T008 RenderTextCommand    ┐
- T009 RenderBarcodeCommand ├─ 可平行執行（不同方法）
- T010 RenderQRCodeCommand  │
- T011 ByteArrayToBitmapImage┘
```

---

## Parallel Example: User Story 1

```bash
# 可平行執行的渲染方法實作：
Task: "實作 RenderTextCommand 方法"
Task: "實作 RenderBarcodeCommand 方法"
Task: "實作 RenderQRCodeCommand 方法"
Task: "實作 ByteArrayToBitmapImage Helper 方法"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. ✅ 完成 Phase 1: Setup（確認既有架構）
2. ✅ 完成 Phase 2: Foundational（常數與訂閱機制）
3. ✅ 完成 Phase 3: User Story 1（核心渲染邏輯）
4. **STOP and VALIDATE**: 手動測試 AC-01、AC-02
5. 若通過，可先交付 MVP

### Incremental Delivery

1. Setup + Foundational → 基礎架構就緒
2. 加入 User Story 1 → 測試 AC-01、AC-02 → MVP 交付
3. 加入 User Story 2 → 測試 AC-03 → 功能完整
4. 加入 User Story 3 → 測試 AC-05 → 體驗完善
5. 完成驗收測試 → 合併至主分支

---

## Summary

| 項目 | 數量 |
|------|------|
| 總任務數 | 23 |
| Phase 1 (Setup) | 2 |
| Phase 2 (Foundational) | 3 |
| Phase 3 (US1) | 6 |
| Phase 4 (US2) | 2 |
| Phase 5 (US3) | 2 |
| Phase 6 (驗收) | 8 |
| 可平行任務 | 4 (T008-T011) |

### 主要修改檔案

| 檔案 | 修改類型 |
|------|----------|
| `NanmaoLabelPOC/Views/LabelPrintView.xaml.cs` | 新增約 100-150 行渲染邏輯 |

---

## Notes

- [P] 任務 = 不同檔案或方法，無相依性
- [Story] 標籤對應規格中的 User Story
- 每個 User Story 應可獨立完成與測試
- 每個任務或邏輯群組完成後進行 commit
- 在任何 Checkpoint 可暫停驗證
- 避免：模糊任務、同檔案衝突、跨 Story 相依
