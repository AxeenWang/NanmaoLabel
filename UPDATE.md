# 更新紀錄

本文件記錄 NanmaoLabel 專案的版本更新歷程。

---

## [0.2.0] - 2026-02-01

### PR #4: ListView/DataGrid 樣式改善 (004-listview-style)

#### 新增
- ListView/DataGrid 共用樣式資源檔與 ControlTemplate
- Selected 狀態視覺效果（背景色 #0078D4、文字白色）
- Hover 狀態視覺效果（背景色 #F0F0F0）

#### 修正
- 修正 DataGrid Selected 狀態文字顏色未變白的問題
- 修正 ListView/DataGrid 文字垂直居中與欄位寬度

#### 驗證
- 完成跨分頁（標籤列印 / 資料管理）視覺一致性驗證

---

## [0.1.0] - 2026-01-27

### PR #3: 按鈕樣式規範 (003-button-style)

#### 新增
- 按鈕顏色 Token 與基礎樣式定義
- 按鈕位置分區佈局
- 按鈕狀態觸發器與頁簽樣式
- 刪除按鈕二次確認保護機制
- 操作回饋動畫與 Loading 狀態
- 鍵盤 Focus 狀態指示

### PR #2: Excel 匯入警告改善 (002-import-warning)

#### 新增
- 訊息分級資料模型（Info / Warning / Error）
- ImportResultDialog 匯入結果對話框
- 允許底線欄位名稱匯入
- 分號警告整合驗證測試

#### 變更
- 編譯組態從 Any CPU 改為 x64

---

## [0.0.1] - 2026-01-26

### PR #1: 標籤列印 POC (001-label-print-poc)

#### 初始功能
- 專案結構建立（MVVM 架構）
- Excel 匯入功能 (`ExcelImporter`)
- JSON 資料儲存 (`DataStore`)
- 標籤渲染引擎 (`LabelRenderer`)
- 條碼生成服務 (`BarcodeGenerator`)
- PDF 輸出服務 (`PdfExporter`)
- 內建標籤格式（QW075551-1、QW075551-2）
- 標籤列印分頁 UI
- 資料管理分頁 UI（CRUD）
- 分頁功能與效能監控
- 單元測試專案
