# Data Model: 南茂標籤列印 POC

**Date**: 2026-01-27
**Phase**: 1 - Design & Contracts
**Status**: Complete

---

## Entity Overview

```text
┌─────────────────────────────────────────────────────────────────────┐
│                           資料模型關係                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────┐         ┌─────────────────────┐                    │
│  │ DataStore   │────────▶│ DataRecord (1..n)   │                    │
│  │ (data.json) │         └─────────────────────┘                    │
│  └─────────────┘                    │                               │
│                                     │ 資料填充                       │
│                                     ▼                               │
│  ┌─────────────────────┐    ┌─────────────────────┐                 │
│  │ LabelTemplate       │───▶│ LabelField (1..n)   │                 │
│  │ (內建 2 種)          │    │ - Text              │                 │
│  └─────────────────────┘    │ - Barcode           │                 │
│                              │ - QRCode            │                 │
│                              └─────────────────────┘                 │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 1. DataRecord

交易資料紀錄，對應 Excel 匯入之單筆資料。

### 欄位定義

| 欄位代碼 | 說明 | 資料類型 | 必填 | 備註 |
|----------|------|----------|------|------|
| ogb19 | 單據編號 | string | ✅ | 主鍵識別，用於 PDF 檔名 |
| nvr_cust | 客戶名稱 | string | ✅ | QW075551-1 CSCUSTOMER |
| nvr_cust_item_no | 客戶料號 | string | ✅ | CSCUSTITEMNO |
| nvr_cust_pn | 客戶 P/N | string | ✅ | Code 128 條碼來源 |
| ogd09 | 數量 | string | ✅ | 儲存為原始數值（不含千分位） |
| obe25 | 日期 | string | ✅ | 格式 yyyy-MM-dd |
| pono | 製令單號 | string | ✅ | QR Code 組合欄位 |
| ima902 | 裝置編號 | string | ✅ | QR Code 組合欄位 |
| nvr_remark10 | 備註 | string | ❌ | 可為空 |
| erpmat | ERP 料號 | string | ❌ | QW075551-2 條碼來源 |
| cscustpo | 客戶採購單號 | string | ❌ | QW075551-2 用 |

### Raw Value vs Display Value [ref: IC-005~006]

```text
┌─────────────┐
│  原始值      │◀── Excel 匯入 → Trim
│  (ogd09)    │
│  "6733"     │
└──────┬──────┘
       │
       ├───────────────────────────────────────────┐
       │                                           │
       ▼                                           ▼
┌─────────────────┐                     ┌─────────────────┐
│  Raw Value      │                     │  Display Value  │
│  用於編碼        │                     │  用於顯示        │
│  "6733"         │                     │  "6,733"        │
│                 │                     │                 │
│  → Barcode 編碼  │                     │  → UI 顯示       │
│  → QR Code 編碼  │                     │  → 標籤文字      │
└─────────────────┘                     └─────────────────┘
```

**關鍵規則**:
- **禁止** 將 Display Value 用於 Barcode/QR Code 編碼
- 數量欄位 (ogd09) 儲存時保留原始數值，不含千分位
- 顯示層自行加上千分位格式化

### 驗證規則

| 欄位 | 驗證規則 | 錯誤處理 [ref: raw_spec 8.9] |
|------|----------|------------------------------|
| ogd09 | 僅允許 0-9 數字 | 阻擋輸入或顯示驗證錯誤 |
| obe25 | 日期格式 yyyy-MM-dd | 匯入時自動轉換 |
| 所有欄位 | Trim 前後空白 | 自動處理 |
| nvr_* 欄位 | 不得含分號 | 匯入時警告 |

---

## 2. LabelTemplate

標籤格式定義，POC 階段內建兩種格式。

### 欄位定義

| 欄位 | 說明 | 資料類型 |
|------|------|----------|
| Code | 標籤代碼 | string |
| Name | 標籤名稱 | string |
| WidthMm | 標籤寬度 (mm) | double |
| HeightMm | 標籤高度 (mm) | double |
| Fields | 欄位清單 | List<LabelField> |

### 內建格式

| 代碼 | 名稱 | 尺寸 | 欄位數 |
|------|------|------|--------|
| QW075551-1 | 出貨標籤 | 100mm × 60mm | 15 (含標籤文字) |
| QW075551-2 | 出貨標籤 | 100mm × 60mm | 13 (含標籤文字) |

---

## 3. LabelField

標籤欄位定義，描述單一欄位的渲染屬性。

### 欄位定義

| 欄位 | 說明 | 資料類型 | 備註 |
|------|------|----------|------|
| Name | 欄位識別名稱 | string | 如 "CSCUSTOMER", "QRCODE" |
| FieldType | 欄位類型 | enum | Text, Barcode, QRCode |
| DataSource | 資料來源 | string | 欄位代碼或常數值 |
| IsConstant | 是否為常數 | bool | true = 直接輸出 DataSource 值 |
| CombinePattern | 組合模式 | string? | QR Code 用，如 "{pono};{ima902}" |
| X | X 座標 (mm) | double | 原點為左上角 |
| Y | Y 座標 (mm) | double | |
| Width | 寬度 (mm) | double | |
| Height | 高度 (mm) | double | |
| FontSize | 字型大小 (pt) | double? | 僅 Text 類型使用 |
| IsBold | 是否粗體 | bool | |
| Alignment | 對齊方式 | enum | Left, Center, Right |

### FieldType 列舉

| 值 | 說明 | 渲染行為 |
|------|------|----------|
| Text | 純文字 | 支援裁切 (Ellipsis)、千分位顯示 |
| Barcode | 一維條碼 | Code 128，**禁止裁切** [ref: IC-007] |
| QRCode | 二維條碼 | Level M，**禁止裁切** [ref: IC-007] |

### 文字溢出處理 [ref: IC-024]

| 欄位類型 | Ellipsis | 縮字 | 裁切 |
|----------|----------|------|------|
| Text | ✅ 是 | ❌ 否 | ✅ 是 |
| Barcode | ❌ 否 | ❌ 否 | ❌ 否 |
| QRCode | ❌ 否 | ❌ 否 | ❌ 否 |

---

## 4. Data Store Schema (data.json)

### JSON 結構

```json
{
  "lastModified": "2026-01-27T14:30:52+08:00",
  "records": [
    {
      "ogb19": "G25A111577",
      "nvr_cust": "XinshengIntelligent",
      "nvr_cust_item_no": "XPA72EA0I-008",
      "nvr_cust_pn": "E110-X0",
      "ogd09": "6733",
      "obe25": "2025-11-14",
      "pono": "511-251020041",
      "ima902": "FC7800280H-00000-00005",
      "nvr_remark10": "00U35NVVR",
      "erpmat": "4D010018",
      "cscustpo": "P600-X1725A3002627"
    }
  ]
}
```

### 欄位說明

| 欄位 | 說明 | 格式 |
|------|------|------|
| lastModified | 最後修改時間 | ISO 8601 (含時區) [ref: IC-017] |
| records | 資料紀錄陣列 | DataRecord[] |

### Single Source of Truth [ref: IC-018]

- `data.json` 為系統唯一資料真相來源
- 禁止記憶體或 UI 元件維護獨立資料狀態
- 所有畫面顯示與 PDF 輸出皆源自此檔案

---

## 5. QW075551-1 欄位對應 [ref: raw_spec 5.1]

| Item | 標籤欄位 | DataSource | Type | X (mm) | Y (mm) | W (mm) | H (mm) | Font | Align |
|------|----------|------------|------|--------|--------|--------|--------|------|-------|
| 1 | 標題 | "出貨標籤 Shipping Label" | Text | 5 | 2 | 90 | 6 | 14pt Bold | Center |
| 2 | 標籤 "Customer 客戶名稱" | const | Text | 5 | 10 | 30 | 4 | 9pt | Left |
| 3 | CSCUSTOMER | nvr_cust | Text | 36 | 10 | 59 | 5 | 11pt Bold | Left |
| 4 | 標籤 "Date" | const | Text | 5 | 17 | 10 | 4 | 9pt | Left |
| 5 | FINDPRTDC | obe25 | Text | 16 | 17 | 25 | 5 | 11pt | Left |
| 6 | 標籤 "Q'ty" | const | Text | 55 | 17 | 10 | 4 | 9pt | Left |
| 7 | CSQTY | ogd09 (Display) | Text | 66 | 17 | 29 | 5 | 11pt Bold | Left |
| 8 | 標籤 "Product NO. 產品型號" | const | Text | 5 | 24 | 40 | 4 | 9pt | Left |
| 9 | CSCUSTITEMNO | nvr_cust_item_no | Text | 5 | 29 | 90 | 5 | 11pt | Left |
| 10 | CSCUSTPN | nvr_cust_pn (Raw) | Barcode | 5 | 35 | 60 | 10 | - | Left |
| 11 | 標籤 "MO:" | const | Text | 5 | 47 | 10 | 4 | 9pt | Left |
| 12 | CSMO | pono | Text | 16 | 47 | 40 | 4 | 10pt | Left |
| 13 | 標籤 "Device:" | const | Text | 5 | 51 | 15 | 4 | 9pt | Left |
| 14 | OUTDEVICENO | ima902 | Text | 21 | 51 | 35 | 4 | 10pt | Left |
| 15 | 標籤 "Remark:" | const | Text | 5 | 55 | 15 | 4 | 9pt | Left |
| 16 | CSREMARK | nvr_remark10 | Text | 21 | 55 | 35 | 4 | 10pt | Left |
| 17 | QRCODE | {pono};{ima902};{ogd09};{nvr_remark10} (Raw) | QRCode | 75 | 40 | 20 | 20 | - | - |

---

## 6. QW075551-2 欄位對應 [ref: raw_spec 5.2]

| Item | 標籤欄位 | DataSource | Type | X (mm) | Y (mm) | W (mm) | H (mm) | Font | Align |
|------|----------|------------|------|--------|--------|--------|--------|------|-------|
| 1 | 標題 | "出貨標籤 Shipping Label" | Text | 5 | 2 | 90 | 6 | 14pt Bold | Center |
| 2 | 標籤 "Customer PO:" | const | Text | 5 | 10 | 30 | 4 | 9pt | Left |
| 3 | CSCUSTPO | cscustpo | Text | 36 | 10 | 59 | 5 | 11pt Bold | Left |
| 4 | 標籤 "CS Number:" | const | Text | 5 | 17 | 25 | 4 | 9pt | Left |
| 5 | CSNUMBER | "17008" (常數) | Text | 31 | 17 | 15 | 5 | 11pt | Left |
| 6 | 標籤 "Q'ty" | const | Text | 55 | 17 | 10 | 4 | 9pt | Left |
| 7 | CSQTY | ogd09 (Display) | Text | 66 | 17 | 29 | 5 | 11pt Bold | Left |
| 8 | 標籤 "ERP Part NO." | const | Text | 5 | 24 | 30 | 4 | 9pt | Left |
| 9 | ERPPARTNO | erpmat (Raw) | Barcode | 5 | 29 | 60 | 10 | - | Left |
| 10 | 標籤 "Customer Item:" | const | Text | 5 | 42 | 30 | 4 | 9pt | Left |
| 11 | CSCUSTITEMNO | nvr_cust_item_no | Text | 36 | 42 | 30 | 4 | 10pt | Left |
| 12 | 標籤 "Remark:" | const | Text | 5 | 48 | 15 | 4 | 9pt | Left |
| 13 | CSREMARK | nvr_remark10 | Text | 21 | 48 | 45 | 4 | 10pt | Left |
| 14 | QRCODE | {cscustpo};{erpmat};{ogd09} (Raw) | QRCode | 75 | 40 | 20 | 20 | - | - |

---

## 7. State Transitions

### DataRecord 生命週期

```text
┌─────────────────┐
│  Excel 匯入      │
│  (Overwrite)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     ┌─────────────────┐
│  記憶體編輯      │────▶│  儲存至 JSON    │
│  (資料管理分頁)  │◀────│  (lastModified) │
└─────────────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐
│  標籤列印分頁    │
│  (唯讀顯示)      │
└─────────────────┘
```

### 修改狀態追蹤

| 狀態 | 狀態列顯示 | 行為 |
|------|------------|------|
| 未修改 | `✅ 就緒` | [儲存] 按鈕停用 |
| 已修改 | `⚠️ 已修改（未儲存）` | [儲存] 按鈕啟用 |
| 切換分頁（未儲存） | 彈出確認對話框 | [儲存] [不儲存] [取消] |

---

## 8. Validation Rules Summary

| 規則 | 適用時機 | 錯誤處理 |
|------|----------|----------|
| 必填欄位不可為空 | 標籤輸出前 | 彈出警告視窗 |
| 數量欄位僅允許數字 | 輸入時、匯入時 | 阻擋或顯示錯誤 |
| 欄位名稱僅允許英數字 | Excel 匯入時 | 視為欄位缺失 [ref: IC-016] |
| 欄位值不得含分號 | QR Code 組合欄位 | 顯示警告 |
| 匯入數值含千分位 | Excel 匯入時 | 視為格式錯誤 [ref: IC-020] |
