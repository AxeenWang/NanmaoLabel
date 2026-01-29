# Data Model: Excel 匯入警告分級

**Feature Branch**: `002-import-warning`
**Date**: 2026-01-29

## 1. 新增實體

### 1.1 MessageSeverity (列舉)

訊息嚴重程度等級。

| 值 | 說明 | 處理方式 |
|----|------|----------|
| `Error` | 匯入失敗，無法繼續 | 中斷匯入，顯示錯誤對話框 |
| `Warning` | 匯入成功，但部分資料可能不完整 | 完成匯入，顯示警告摘要 |
| `Info` | 提示資訊，不影響結果 | 可選顯示，預設隱藏 |

**對應需求**：FR-003

---

### 1.2 ImportMessage (類別)

匯入過程中產生的單一訊息。

| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| `Severity` | `MessageSeverity` | 是 | 訊息等級 |
| `Message` | `string` | 是 | 訊息內容（繁體中文） |
| `FieldName` | `string?` | 否 | 關聯的欄位名稱 |

**對應需求**：FR-003, FR-009

**驗證規則**：
- `Message` 不可為空字串
- `Severity` 必須為有效列舉值

---

## 2. 修改實體

### 2.1 ImportResult (修改)

匯入操作的整體結果。

| 欄位 | 型別 | 變更 | 說明 |
|------|------|------|------|
| `Success` | `bool` | 無 | 是否成功 |
| `Records` | `List<DataRecord>` | 無 | 匯入的資料紀錄 |
| `ErrorMessage` | `string?` | 無 | 錯誤訊息（Error 等級） |
| `Warnings` | `List<string>` | 標記廢棄 | 保留向後相容，回傳 Warning+Error 訊息 |
| `Messages` | `List<ImportMessage>` | **新增** | 結構化訊息清單（所有等級） |
| `RecordCount` | `int` | 無 | 匯入筆數（計算屬性） |

**對應需求**：FR-007, FR-008

**新增計算屬性**：

| 屬性 | 型別 | 說明 |
|------|------|------|
| `ErrorCount` | `int` | Error 等級訊息數量 |
| `WarningCount` | `int` | Warning 等級訊息數量 |
| `InfoCount` | `int` | Info 等級訊息數量 |
| `HasErrors` | `bool` | 是否有 Error 等級訊息 |
| `HasWarnings` | `bool` | 是否有 Warning 等級訊息 |
| `HasInfos` | `bool` | 是否有 Info 等級訊息 |

---

## 3. 訊息分類矩陣

### 3.1 Error 等級訊息

| 情境 | 訊息範例 | 欄位名稱 |
|------|----------|----------|
| 檔案不存在 | 「找不到指定的檔案」 | null |
| 檔案格式錯誤 | 「檔案格式不正確，請確認為 .xlsx 格式」 | null |
| Excel 無資料表 | 「Excel 檔案中沒有資料表」 | null |

**對應需求**：FR-004, FR-009

---

### 3.2 Warning 等級訊息

| 情境 | 訊息範例 | 欄位名稱 |
|------|----------|----------|
| 必要欄位缺失 | 「缺少必要欄位：ogb19（單據編號）」 | `ogb19` |
| 欄位值含分號 | 「欄位 'pono' 值 'ABC;123' 包含分號，可能影響 QR Code」 | `pono` |
| 數量欄位含千分位 | 「數量欄位 'ogd09' 包含千分位格式 '1,234'，已自動移除」 | `ogd09` |
| 數量欄位非數字 | 「數量欄位 'ogd09' 值 'abc' 包含非數字字元」 | `ogd09` |

**對應需求**：FR-005, FR-009, FR-010

---

### 3.3 Info 等級訊息

| 情境 | 訊息範例 | 欄位名稱 |
|------|----------|----------|
| 額外欄位被忽略 | 「欄位 'extra_col' 不在對應清單中，已忽略」 | `extra_col` |
| 空白列被略過 | 「第 5 列為空白列，已略過」 | null |
| 非法欄位名稱 | 「欄位名稱 'field@name' 包含非法字元，已忽略」 | `field@name` |

**對應需求**：FR-006, FR-009

---

## 4. 狀態轉換

### 4.1 匯入流程狀態

```
[開始] → 檔案檢查 → 格式檢查 → 讀取資料 → 欄位對應 → 資料解析 → [結束]
              ↓           ↓                      ↓           ↓
           Error       Error                  Info       Warning
```

**狀態說明**：
- 檔案/格式檢查失敗 → `Success = false`，立即返回
- 欄位對應問題 → 記錄 Info，繼續處理
- 資料解析問題 → 記錄 Warning，繼續處理

---

## 5. 向後相容性

### 5.1 Warnings 屬性（廢棄但保留）

```csharp
[Obsolete("Use Messages property instead")]
public List<string> Warnings => Messages
    .Where(m => m.Severity != MessageSeverity.Info)
    .Select(m => m.Message)
    .ToList();
```

**說明**：
- 回傳 Error + Warning 等級的訊息文字
- 現有使用 `Warnings` 的程式碼可繼續運作
- 建議逐步遷移至 `Messages` 屬性
