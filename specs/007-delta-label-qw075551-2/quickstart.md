# Quickstart: QW075551-2 物料標籤渲染

**Feature**: 007-delta-label-qw075551-2
**Date**: 2026-02-02

## 1. 快速開始

### 1.1 建置專案

```bash
cd NanmaoLabelPOC
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet build"
```

### 1.2 執行測試

```bash
cd NanmaoLabelPOC.Tests
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet test"
```

### 1.3 執行應用程式

```bash
cd NanmaoLabelPOC
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet run"
```

## 2. 修改要點

### 2.1 主要修改檔案

```
NanmaoLabelPOC/Templates/BuiltInTemplates.cs
```

### 2.2 修改方法

```csharp
private static LabelTemplate CreateQW075551_2()
{
    return new LabelTemplate
    {
        Code = "QW075551-2",
        Name = "物料標籤",      // 變更自「出貨標籤」
        WidthMm = 100,
        HeightMm = 80,          // 變更自 60
        HasBorder = true,       // 新增外框
        Fields = new List<LabelField>
        {
            // 見 data-model.md 詳細定義
        }
    };
}
```

## 3. 驗證步驟

### 3.1 單元測試驗證

1. 新增測試案例於 `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs`
2. 測試項目：
   - 標籤尺寸為 100mm × 80mm
   - 標題為「物料標籤」
   - 無 Barcode 欄位
   - 無 QR Code 欄位
   - CSQTY 有千分位格式
   - 6 個欄位皆有雙行顯示

### 3.2 PDF 輸出驗證

1. 執行應用程式
2. 匯入測試 Excel 資料
3. 選擇 QW075551-2 模板
4. 匯出 PDF
5. 目視檢查版面符合 spec.md 版面配置參考

## 4. 測試資料

### 4.1 Excel 測試欄位

| 欄位名稱 | 測試值 |
|----------|--------|
| cscustpo | P600-X172509050001 |
| erpmat | 4D010018 |
| nvr_cust_item_no | XPA72EA0I-008 |
| ogd09 | 6733 |
| nvr_remark10 | 00U35NVVR |

### 4.2 預期輸出

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              物料標籤                                            │
│                                                                                 │
│ 單號:P600-X172509050001                    代碼:17008                            │
│ P600-X172509050001                         17008                                 │
│                                                                                 │
│ ERP料號:4D010018                           規格型號:XPA72EA0I-008                │
│ 4D010018                                   XPA72EA0I-008                         │
│                                                                                 │
│ 數量:6,733                                 D/C (LOT NO. ):00U35NVVR              │
│ 6,733                                      00U35NVVR                             │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 5. 常見問題

### Q1: 長文字溢出怎麼處理？

A: 系統會自動縮小字體至最小 6pt，若仍溢出則允許換行。相關屬性：
- `AutoShrinkFont = true`
- `MinFontSize = 6` (預設值)

### Q2: 如何驗證千分位格式？

A: 檢查 CSQTY 欄位的 `UseDisplayValue = true` 設定，以及 PDF 輸出中數量欄位是否顯示為 `6,733` 而非 `6733`。

### Q3: 外框線條粗細如何設定？

A: 外框線條粗細由 PdfExporter 實作決定，建議 0.5pt ~ 1pt，無需在模板中指定。
