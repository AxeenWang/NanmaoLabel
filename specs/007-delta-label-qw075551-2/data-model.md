# Data Model: QW075551-2 物料標籤渲染

**Feature**: 007-delta-label-qw075551-2
**Date**: 2026-02-02
**Status**: Complete

## 1. 概述

本文件定義 QW075551-2 物料標籤的資料模型與欄位對應關係。

## 2. 標籤模板定義

### 2.1 LabelTemplate

| 屬性 | 值 | 說明 |
|------|-----|------|
| Code | "QW075551-2" | 標籤代碼 |
| Name | "物料標籤" | 標籤名稱 [FR-002] |
| WidthMm | 100 | 標籤寬度 (mm) |
| HeightMm | 80 | 標籤高度 (mm) [FR-001, Clarification] |
| HasBorder | true | 繪製外框 [FR-010] |

### 2.2 欄位總覽

```
QW075551-2 物料標籤（13 個 LabelField）
├── Title (1)
│   └── "物料標籤" - 14pt Bold, 置中
├── 第一列 (4)
│   ├── 單號標籤行 - "單號:{cscustpo}" 8pt
│   ├── 單號數值行 - {cscustpo} 11pt Bold
│   ├── 代碼標籤行 - "代碼:17008" 8pt
│   └── 代碼數值行 - "17008" 11pt Bold
├── 第二列 (4)
│   ├── ERP料號標籤行 - "ERP料號:{erpmat}" 8pt
│   ├── ERP料號數值行 - {erpmat} 11pt Bold
│   ├── 規格型號標籤行 - "規格型號:{nvr_cust_item_no}" 8pt
│   └── 規格型號數值行 - {nvr_cust_item_no} 11pt Bold
└── 第三列 (4)
    ├── 數量標籤行 - "數量:{ogd09}" 8pt
    ├── 數量數值行 - {ogd09} 11pt Bold (千分位)
    ├── D/C標籤行 - "D/C (LOT NO. ):{nvr_remark10}" 8pt
    └── D/C數值行 - {nvr_remark10} 11pt Bold
```

## 3. CodeSoft 欄位對照表

[ref: spec.md CodeSoft 欄位對照表]

| Item | CodeSoft 欄位 | 資料來源欄位 | 顯示前綴 | 顯示類型 | 需求追溯 |
|------|---------------|--------------|----------|----------|----------|
| 1 | CSCUSTPO | cscustpo | 單號: | Text | FR-005 |
| 2 | CSNUMBER | "17008" (固定值) | 代碼: | Text | FR-006 |
| 3 | ERPPARTNO | erpmat | ERP料號: | Text | FR-011 |
| 4 | CSCUSTITEMNO | nvr_cust_item_no | 規格型號: | Text | FR-005 |
| 5 | CSQTY | ogd09 | 數量: | Text (千分位) | FR-007 |
| 6 | CSREMARK | nvr_remark10 | D/C (LOT NO. ): | Text | FR-005 |

## 4. 欄位定義詳表

### 4.1 標題欄位

```csharp
new LabelField
{
    Name = "Title",
    FieldType = FieldType.Text,
    DataSource = "物料標籤",  // [FR-002]
    IsConstant = true,
    X = 5, Y = 3, Width = 90, Height = 6,
    FontSize = 14, IsBold = true,
    Alignment = TextAlignment.Center
}
```

### 4.2 第一列欄位

#### 4.2.1 單號 (左欄)

```csharp
// 小字行：標籤 + 值
new LabelField
{
    Name = "CSCUSTPO_Label",
    FieldType = FieldType.Text,
    DataSource = "單號:",
    IsConstant = true,
    X = 5, Y = 14, Width = 12, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "CSCUSTPO_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "cscustpo",
    IsConstant = false,
    X = 17, Y = 14, Width = 33, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
},
// 大字行：純數值
new LabelField
{
    Name = "CSCUSTPO",
    FieldType = FieldType.Text,
    DataSource = "cscustpo",
    IsConstant = false,
    X = 5, Y = 19, Width = 45, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
}
```

#### 4.2.2 代碼 (右欄)

```csharp
// 小字行：標籤 + 值 (固定值)
new LabelField
{
    Name = "CSNUMBER_Label",
    FieldType = FieldType.Text,
    DataSource = "代碼:",
    IsConstant = true,
    X = 55, Y = 14, Width = 10, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "CSNUMBER_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "17008",  // [FR-006] 固定值
    IsConstant = true,
    X = 65, Y = 14, Width = 30, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
// 大字行：純數值 (固定值)
new LabelField
{
    Name = "CSNUMBER",
    FieldType = FieldType.Text,
    DataSource = "17008",  // [FR-006] 固定值
    IsConstant = true,
    X = 55, Y = 19, Width = 40, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left
}
```

### 4.3 第二列欄位

#### 4.3.1 ERP料號 (左欄)

```csharp
// 小字行
new LabelField
{
    Name = "ERPPARTNO_Label",
    FieldType = FieldType.Text,
    DataSource = "ERP料號:",
    IsConstant = true,
    X = 5, Y = 32, Width = 18, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "ERPPARTNO_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "erpmat",
    IsConstant = false,
    X = 23, Y = 32, Width = 27, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
},
// 大字行
new LabelField
{
    Name = "ERPPARTNO",
    FieldType = FieldType.Text,  // [FR-011] 由 Barcode 改為 Text
    DataSource = "erpmat",
    IsConstant = false,
    X = 5, Y = 37, Width = 45, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
}
```

#### 4.3.2 規格型號 (右欄)

```csharp
// 小字行
new LabelField
{
    Name = "CSCUSTITEMNO_Label",
    FieldType = FieldType.Text,
    DataSource = "規格型號:",
    IsConstant = true,
    X = 55, Y = 32, Width = 18, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "CSCUSTITEMNO_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "nvr_cust_item_no",
    IsConstant = false,
    X = 73, Y = 32, Width = 22, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
},
// 大字行
new LabelField
{
    Name = "CSCUSTITEMNO",
    FieldType = FieldType.Text,
    DataSource = "nvr_cust_item_no",
    IsConstant = false,
    X = 55, Y = 37, Width = 40, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
}
```

### 4.4 第三列欄位

#### 4.4.1 數量 (左欄)

```csharp
// 小字行
new LabelField
{
    Name = "CSQTY_Label",
    FieldType = FieldType.Text,
    DataSource = "數量:",
    IsConstant = true,
    X = 5, Y = 50, Width = 12, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "CSQTY_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "ogd09",
    IsConstant = false,
    UseDisplayValue = true,  // [FR-007] 千分位
    X = 17, Y = 50, Width = 33, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
},
// 大字行
new LabelField
{
    Name = "CSQTY",
    FieldType = FieldType.Text,
    DataSource = "ogd09",
    IsConstant = false,
    UseDisplayValue = true,  // [FR-007] 千分位
    X = 5, Y = 55, Width = 45, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
}
```

#### 4.4.2 D/C (LOT NO.) (右欄)

```csharp
// 小字行
new LabelField
{
    Name = "CSREMARK_Label",
    FieldType = FieldType.Text,
    DataSource = "D/C (LOT NO. ):",
    IsConstant = true,
    X = 55, Y = 50, Width = 30, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left
},
new LabelField
{
    Name = "CSREMARK_LabelValue",
    FieldType = FieldType.Text,
    DataSource = "nvr_remark10",
    IsConstant = false,
    X = 85, Y = 50, Width = 10, Height = 4,
    FontSize = 8, IsBold = false,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
},
// 大字行
new LabelField
{
    Name = "CSREMARK",
    FieldType = FieldType.Text,
    DataSource = "nvr_remark10",
    IsConstant = false,
    X = 55, Y = 55, Width = 40, Height = 6,
    FontSize = 11, IsBold = true,
    Alignment = TextAlignment.Left,
    AutoShrinkFont = true
}
```

## 5. 欄位統計

| 類型 | 數量 | 說明 |
|------|------|------|
| 標題 | 1 | 常數文字 |
| 小字行標籤 | 6 | 常數文字（前綴） |
| 小字行數值 | 6 | 變數或常數 |
| 大字行數值 | 6 | 變數或常數 |
| **總計** | **19** | |

## 6. 移除項目

| 項目 | 原類型 | 移除原因 | 需求追溯 |
|------|--------|----------|----------|
| ERPPARTNO Barcode | Barcode (Code 128) | 改為 Text | FR-011 |
| QRCODE | QRCode | 規格不含 QR Code | FR-011 |

## 7. 資料處理規則

| 欄位 | 規則 | 需求追溯 |
|------|------|----------|
| 所有 Text | Trim 前後空白 | FR-012 |
| CSNUMBER | 固定值 "17008" | FR-006 |
| CSQTY | 千分位格式化 (UseDisplayValue = true) | FR-007 |
| 空值欄位 | 保留版面位置，顯示空字串 | FR-009 |
| 長文字 | AutoShrinkFont 縮小字體，最小 6pt | FR-008 |
