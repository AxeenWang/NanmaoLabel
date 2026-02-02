# Data Model: QW075551-1 出貨標籤 Delta Spec 實作

**Feature**: 006-delta-label-qw075551-1
**Date**: 2026-02-02

## 模型變更摘要

本次變更主要涉及 `LabelField` 模型擴充與 `BuiltInTemplates` 中 QW075551-1 的定義更新。

---

## 1. LabelField 模型擴充

### 新增屬性

| 屬性 | 類型 | 預設值 | 說明 | 需求追溯 |
|------|------|--------|------|----------|
| MinFontSize | double | 6 | 長文字縮小時的最小字體大小 (pt) | FR-008 |
| AutoShrinkFont | bool | false | 是否啟用自動縮小字體 | FR-008 |

### 完整模型定義

```csharp
public class LabelField
{
    // 現有屬性
    public string Name { get; set; } = string.Empty;
    public FieldType FieldType { get; set; } = FieldType.Text;
    public string DataSource { get; set; } = string.Empty;
    public bool IsConstant { get; set; }
    public string? CombinePattern { get; set; }
    public bool UseDisplayValue { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double? FontSize { get; set; }
    public bool IsBold { get; set; }
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    // 新增屬性 [FR-008]
    public double MinFontSize { get; set; } = 6;
    public bool AutoShrinkFont { get; set; } = false;
}
```

---

## 2. RenderCommand 模型擴充

### 新增屬性

| 屬性 | 類型 | 說明 | 需求追溯 |
|------|------|------|----------|
| ActualFontSize | double | 經過縮小計算後的實際字體大小 | FR-008 |
| RequiresWrap | bool | 是否需要換行 | FR-008 |

---

## 3. QW075551-1 模板定義變更

### 基本屬性變更

| 屬性 | 原值 | 新值 | 需求追溯 |
|------|------|------|----------|
| HeightMm | 60 | 80 | FR-001 |

### 欄位定義變更

#### 移除的欄位

| 欄位名稱 | 原因 | 需求追溯 |
|----------|------|----------|
| MoLabel | 合併至 Remarks 區段 | FR-013 |
| DeviceLabel | 合併至 Remarks 區段 | FR-013 |
| RemarkLabel | 合併至 Remarks 區段 | FR-013 |

#### 新增的欄位

| 欄位名稱 | FieldType | DataSource | X | Y | Width | Height | 說明 | 需求追溯 |
|----------|-----------|------------|---|---|-------|--------|------|----------|
| RemarksLabel | Text | "Remarks" | 5 | 50 | 20 | 4 | Remarks 區段標籤 | FR-013 |

#### 修改的欄位

| 欄位名稱 | 變更項目 | 原值 | 新值 | 需求追溯 |
|----------|----------|------|------|----------|
| CSCUSTPN | FieldType | Barcode | Text | FR-005 |
| CSCUSTPN | Y | 35 | 41 | FR-002 |
| CSCUSTPN | Height | 10 | 5 | FR-002 |
| QRCODE | X | 75 | 5 | FR-012 |
| QRCODE | Y | 40 | 55 | FR-012 |
| CSMO | X | 16 | 28 | FR-014 |
| CSMO | Y | 47 | 55 | FR-014 |
| CSMO | Width | 40 | 67 | FR-014 |
| OUTDEVICENO | X | 21 | 28 | FR-014 |
| OUTDEVICENO | Y | 51 | 60 | FR-014 |
| OUTDEVICENO | Width | 35 | 67 | FR-014 |
| CSREMARK | X | 21 | 28 | FR-014 |
| CSREMARK | Y | 55 | 65 | FR-014 |
| CSREMARK | Width | 35 | 67 | FR-014 |
| CustomerLabel | DataSource | "Customer 客戶名稱" | "Customer\n客戶名稱" | FR-019 |
| CustomerLabel | Height | 4 | 8 | FR-019 |
| ProductNoLabel | DataSource | "Product NO. 產品型號" | "Product NO.\n產品型號" | FR-019 |
| ProductNoLabel | Height | 4 | 8 | FR-019 |

### 完整欄位座標表

| # | 欄位名稱 | FieldType | X | Y | Width | Height | FontSize | AutoShrinkFont |
|---|----------|-----------|---|---|-------|--------|----------|----------------|
| 1 | Title | Text | 5 | 2 | 90 | 6 | 14 | false |
| 2 | CustomerLabel | Text | 5 | 10 | 20 | 8 | 9 | false |
| 3 | CSCUSTOMER | Text | 28 | 10 | 67 | 5 | 11 | true |
| 4 | DateLabel | Text | 5 | 19 | 10 | 4 | 9 | false |
| 5 | FINDPRTDC | Text | 16 | 19 | 25 | 5 | 11 | true |
| 6 | QtyLabel | Text | 55 | 19 | 10 | 4 | 9 | false |
| 7 | CSQTY | Text | 66 | 19 | 29 | 5 | 11 | true |
| 8 | ProductNoLabel | Text | 5 | 26 | 20 | 8 | 9 | false |
| 9 | CSCUSTITEMNO | Text | 5 | 35 | 90 | 5 | 11 | true |
| 10 | CSCUSTPN | Text | 5 | 41 | 90 | 5 | 11 | true |
| 11 | RemarksLabel | Text | 5 | 50 | 20 | 4 | 9 | false |
| 12 | QRCODE | QRCode | 5 | 55 | 20 | 20 | - | false |
| 13 | CSMO | Text | 28 | 55 | 67 | 4 | 10 | true |
| 14 | OUTDEVICENO | Text | 28 | 60 | 67 | 4 | 10 | true |
| 15 | CSREMARK | Text | 28 | 65 | 67 | 4 | 10 | true |

---

## 4. 欄位對照表 (CodeSoft → 資料來源)

| Item | CodeSoft 欄位 | 資料來源欄位 | 顯示類型 | 格式處理 |
|------|---------------|--------------|----------|----------|
| 1 | CSCUSTOMER | nvr_cust | Text | Trim |
| 2 | FINDPRTDC | obe25 | Text | yyyy-MM-dd → yyyy/MM/dd |
| 3 | CSQTY | ogd09 | Text | 千分位 (Display Value) |
| 4 | CSCUSTITEMNO | nvr_cust_item_no | Text | Trim |
| 5 | CSCUSTPN | nvr_cust_pn | Text | Trim |
| 6 | CSMO | pono | Text | Trim |
| 7 | OUTDEVICENO | ima902 | Text | Trim |
| 8 | CSREMARK | nvr_remark10 | Text | Trim |
| 9 | QRCODE | CSMO;OUTDEVICENO;CSQTY;CSREMARK | QRCode | 分號串接，Raw Value |

---

## 5. 狀態轉換

本功能不涉及狀態機或生命週期管理。

---

## 6. 驗證規則

| 欄位 | 規則 | 說明 |
|------|------|------|
| MinFontSize | >= 1 | 最小字體不得小於 1pt |
| FINDPRTDC | 日期格式 | 轉換失敗時保留原值 |
| QRCODE 組合 | 空值保留分號 | 如 `;FC7800280H;;` |
