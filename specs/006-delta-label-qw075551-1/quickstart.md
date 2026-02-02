# Quickstart: QW075551-1 出貨標籤 Delta Spec 實作

**Feature**: 006-delta-label-qw075551-1
**Date**: 2026-02-02

## 快速驗證步驟

### 1. 建置專案

```bash
# WSL 環境下透過 PowerShell 穿透編譯
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet build NanmaoLabelPOC"
```

### 2. 執行測試

```bash
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet test NanmaoLabelPOC.Tests"
```

### 3. 執行應用程式

```bash
powershell.exe -Command "cd '$(wslpath -w .)'; dotnet run --project NanmaoLabelPOC"
```

---

## 驗證 Checklist

### 視覺驗證 (PDF 輸出)

- [ ] **SC-001**: PDF 頁面尺寸為 100mm × 80mm
- [ ] **SC-002**: 日期格式為 yyyy/MM/dd（如 2025/11/14）
- [ ] **SC-003**: CSCUSTPN 顯示為純文字，非條碼
- [ ] **SC-004**: QR Code 尺寸為 20mm × 20mm
- [ ] **SC-007**: 標籤有外框，無分隔線
- [ ] **SC-008**: "Product NO." 拼寫正確
- [ ] **SC-010**: 無 Code 128 條碼
- [ ] **SC-011**: 欄位標籤為中英文分行顯示

### QR Code 驗證

- [ ] **SC-005**: 掃描 QR Code，內容格式為 `{CSMO};{OUTDEVICENO};{CSQTY};{CSREMARK}`
- [ ] **SC-006**: QR Code 內的數量為原始值（無千分位），文字顯示有千分位

### 長文字驗證

- [ ] **SC-009**: 輸入超長文字，字體自動縮小（最小 6pt）
- [ ] 超長文字在 6pt 下仍無法容納時，顯示截斷加省略號

---

## 測試資料範例

```json
{
  "nvr_cust": "DELTA ELECTRONICS INC.",
  "obe25": "2025-11-14",
  "ogd09": "6733",
  "nvr_cust_item_no": "P/N: 123456789012345",
  "nvr_cust_pn": "E110-X0",
  "pono": "511-251020041",
  "ima902": "FC7800280H-00000-00005",
  "nvr_remark10": "00U35NVVR"
}
```

### 預期 QR Code 內容

```
511-251020041;FC7800280H-00000-00005;6733;00U35NVVR
```

### 預期 CSQTY 顯示值

```
6,733
```

### 預期日期顯示

```
2025/11/14
```

---

## 關鍵檔案

| 檔案 | 說明 |
|------|------|
| `NanmaoLabelPOC/Templates/BuiltInTemplates.cs` | QW075551-1 模板定義 |
| `NanmaoLabelPOC/Services/LabelRenderer.cs` | 渲染邏輯 |
| `NanmaoLabelPOC/Services/PdfExporter.cs` | PDF 輸出 |
| `NanmaoLabelPOC/Models/LabelField.cs` | 欄位模型 |
| `NanmaoLabelPOC.Tests/Services/LabelRendererTests.cs` | 單元測試 |

---

## 故障排除

### PDF 頁面尺寸不正確

檢查 `BuiltInTemplates.CreateQW075551_1()` 中的 `HeightMm` 是否設為 80。

### 日期格式未轉換

檢查 `LabelRenderer.ResolveContent()` 中是否有 FINDPRTDC 的格式轉換邏輯。

### QR Code 位置不正確

檢查 `BuiltInTemplates.CreateQW075551_1()` 中 QRCODE 欄位的 X 座標是否為 5。

### 無標籤外框

檢查 `PdfExporter.CreateDocument()` 中是否有 `.Border()` 呼叫。
