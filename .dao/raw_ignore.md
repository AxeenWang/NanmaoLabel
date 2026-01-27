# 🚫 忽略契約（Ignore Contract）

本文件定義本專案於版本控制（Git）中**必須忽略（ignore）**之檔案與目錄，
目的在於避免將「可再生、環境相關、個人化、或無法審查」之產物納入版本控制。

本契約屬 **強制性規範**，適用於所有參與者與自動化工具（包含 AI）。

---

## 一、忽略契約的基本原則

1. **可再生產物不得提交**
   - 任何可由工具、編譯流程、或指令重新產生之檔案，一律忽略。

2. **環境與個人差異不得進入 Repo**
   - 與個人機器、IDE、OS、帳號或暫存狀態相關之檔案，一律忽略。

3. **輸出結果 ≠ 原始碼**
   - Build 結果、快取、暫存資料，不屬於原始碼的一部分。

4. **安全優先**
   - 憑證、金鑰、連線資訊，無論是否測試用途，一律忽略。

---

## 二、Visual Studio / .NET 專案忽略規範

### 1. 建置與輸出目錄

```gitignore
bin/
obj/
[Bb]uild/
[Dd]ist/
```

---

### 2. Visual Studio / IDE 產物

```gitignore
.vs/
.vscode/
.idea/
*.user
*.suo
*.userosscache
*.sln.docstates
```

---

### 3. 測試、快取與暫存

```gitignore
TestResults/
*.cache
*.tmp
*.log
```

---

## 三、作業系統相關忽略項目

### Windows

```gitignore
Thumbs.db
Desktop.ini
$RECYCLE.BIN/
```

### macOS

```gitignore
.DS_Store
.AppleDouble
.LSOverride
```

### Linux

```gitignore
*~
.nfs*
```

---

## 四、套件與相依性

> 本專案以 **套件定義檔** 為唯一可信來源，
> 實體下載結果不得提交。

```gitignore
packages/
node_modules/
```

---

## 五、機密與設定檔（強制忽略）

```gitignore
*.env
*.env.*
*.secret
*.key
*.pfx
*.snk
appsettings.*.json
```

> 若需提交設定範例，請使用：
> - `appsettings.example.json`
> - `sample.env`

---

## 六、AI / 工具行為限制（重要）

```text
- AI 不得移除、修改或重新格式化 `.gitignore`
- AI 不得將已被忽略的檔案重新加入版本控制
- AI 不得因「方便展示」而提交忽略項目
```

---

## 七、違規處理

1. 提交忽略項目者：
   - 視為違反專案契約
   - Pull Request 得直接退回

2. 已誤提交之忽略檔案：
   - 必須自 Repo 中移除
   - 並補齊 `.gitignore` 規則

---

## 八、摘要（給 README 用）

> 本專案嚴格區分「原始碼」與「環境產物」，  
> 所有可再生、個人化、或具安全風險之檔案  
> 皆不得納入版本控制。
