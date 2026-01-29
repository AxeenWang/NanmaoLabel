# Research: Excel 匯入警告分級

**Feature Branch**: `002-import-warning`
**Date**: 2026-01-29

## 1. 欄位名稱規則修正

### 決策
修正正規表達式從 `^[A-Za-z0-9]+$` 改為 `^[A-Za-z0-9_]+$`，允許底線字元。

### 理由
- raw_spec 附錄 A 定義的欄位名稱包含底線：`nvr_cust`、`nvr_cust_item_no`、`nvr_cust_pn`、`nvr_remark10`
- 原規格 13.11 與附錄 A 存在矛盾，raw_delta_import.md 明確指出此為規格內部矛盾
- 修正後與實際資料源一致

### 替代方案
1. **欄位名稱自動轉換**（如 `nvr_cust` → `nvrcust`）- 已被排除，raw_delta_import.md 明確列為 Out of Scope
2. **維持現狀 + 內部對應**（已實作於 `GetFieldValue`）- 目前程式碼已有此機制，但外部欄位名稱仍被判定為非法

### 影響範圍
- `ExcelImporter.cs:34` - `FieldNamePattern()` 正規表達式
- 測試檔案需更新相應測試案例

---

## 2. 訊息分級設計

### 決策
新增 `MessageSeverity` 列舉與 `ImportMessage` 類別，取代現有 `List<string> Warnings`。

### 理由
- 現有設計僅有單一 `Warnings` 清單，無法區分嚴重程度
- 規格要求三級分類：Error（中斷）、Warning（繼續但警示）、Info（可忽略）
- 結構化訊息便於 UI 分組顯示

### 設計選擇

#### MessageSeverity 列舉
```csharp
public enum MessageSeverity
{
    Error,    // 中斷匯入
    Warning,  // 繼續匯入，但顯示警告
    Info      // 資訊提示，預設隱藏
}
```

#### ImportMessage 類別
```csharp
public class ImportMessage
{
    public MessageSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? FieldName { get; init; }
}
```

### 向後相容性
- 保留 `ImportResult.Warnings` 屬性（標記為 `[Obsolete]`），回傳所有非 Info 訊息
- 新增 `ImportResult.Messages` 屬性，回傳結構化訊息清單
- 現有使用 `Warnings` 的程式碼可繼續運作

---

## 3. 訊息分類對照

### 依據 raw_delta_import.md 3.3 節

| 訊息類型 | 分級 | 實作位置 |
|----------|------|----------|
| 檔案不存在 | Error | `Import()` 開頭 |
| 檔案格式錯誤 | Error | `Import()` 開頭 |
| 必要欄位缺失 | Warning | `BuildColumnMapping()` |
| 欄位值含分號 | Warning | `CheckSemicolonWarnings()` |
| 數量欄位含千分位 | Warning | `GetQuantityFieldValue()` |
| 額外欄位被忽略 | Info | `BuildColumnMapping()` |
| 空白列被略過 | Info | `Import()` 迴圈內 |

### Error 特殊處理
Error 等級訊息需中斷匯入流程，設定 `ImportResult.Success = false` 並立即返回。

---

## 4. UI 顯示設計

### 決策
使用 WPF `Expander` 控件實作 Info 區段收合/展開。

### 理由
- `Expander` 為 WPF 內建控件，無需額外套件
- 符合 Windows 標準 UI 模式
- 支援動畫與無障礙功能

### 匯入結果對話框格式

```
匯入成功，共 10 筆資料

[!] 警告（1）：
  • 欄位 'pono' 值 'ABC;123' 包含分號，可能影響 QR Code

▶ [i] 資訊（2）：[點擊展開]
  • 欄位 'extra_col' 不在對應清單中，已忽略
  • 3 列空白列已略過
```

### 實作選項
1. **MessageBox + 自訂格式字串** - 簡單但缺乏互動性
2. **自訂 Dialog Window** - 支援 Expander，需新增 XAML 檔案
3. **修改現有 MessageBox 呼叫** - 快速但無法實作收合功能

**選擇方案 2**：建立 `ImportResultDialog.xaml` 自訂對話框，支援：
- Error/Warning/Info 分組顯示
- Info 區段使用 Expander 預設收合
- 大量訊息時顯示摘要

---

## 5. 測試策略

### 新增測試案例

| 測試類別 | 測試目標 | 對應需求 |
|----------|----------|----------|
| FieldNameValidation | 底線欄位應通過驗證 | FR-001 |
| FieldNameValidation | 其他特殊字元應拒絕 | FR-002 |
| MessageSeverity | Error 應中斷匯入 | FR-004 |
| MessageSeverity | Warning 應允許繼續 | FR-005 |
| MessageSeverity | Info 應正確分類 | FR-006 |
| ImportResult | Messages 應包含正確分級 | FR-003, FR-009 |

### 測試資料
需建立測試用 Excel 檔案或使用 Mock 物件模擬 DataTable。

---

## 6. 風險評估

| 風險 | 機率 | 影響 | 緩解措施 |
|------|------|------|----------|
| 現有 Excel 匯入失敗 | 低 | 高 | 保留向後相容性、完整回歸測試 |
| UI 對話框效能問題 | 低 | 中 | 限制顯示訊息數量（前 10 條） |
| 測試覆蓋不足 | 中 | 中 | 先撰寫測試再實作 |
