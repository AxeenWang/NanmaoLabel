# 📜 文字編輯格式契約・編輯與版本控制聲明



本文件整合並整理原有規範與 AI 行為契約，目的在於**降低誤解、避免格式災難、並確保人類與 AI 行為一致**。



---



## 一、專案立場與開發環境



- 主要開發工具：**Visual Studio 2026**

- 主要作業系統：**Windows**

- 本專案明確採取 Windows / Visual Studio 導向之格式策略



---



## 二、換行字元（Line Ending）政策



- **除 `.sh` 以外的所有文字檔案：**

  - 使用 `CRLF`（`\r\n`）

- **`.sh` 檔案：**

  - 必須使用 `LF`（`\n`）



禁止事項：

- 禁止混用 LF / CRLF

- 禁止自動正規化既有檔案



---



## 三、編碼（Encoding）政策



### `.cs` 原始碼檔案



- **必須使用 UTF-8 with BOM**

- 不得移除 BOM

- 不得轉為 UTF-8 without BOM

- 不得轉為 ANSI / CP950



> 說明：  

> 在 Windows + Visual Studio 環境下，  

> UTF-8 with BOM 為避免誤判為 CP950 的最穩定方案。



### 其他文字檔案



- 使用 UTF-8

- 不強制 BOM

- 不得使用 ANSI / CP950



---



## 四、`.editorconfig` 規範



```ini

root = true



[*]

charset = utf-8

end_of_line = crlf

insert_final_newline = true

trim_trailing_whitespace = true



[*.cs]

charset = utf-8-bom



[*.sh]

end_of_line = lf

```



---



## 五、`.gitattributes` 規範



```gitattributes

* -text



*.cs   text eol=crlf

*.md   text eol=crlf

*.json text eol=crlf

*.xml  text eol=crlf

*.yml  text eol=crlf

*.yaml text eol=crlf



*.sh   text eol=lf



*.png binary

*.jpg binary

*.jpeg binary

*.gif binary

*.ico binary

*.pdf binary

*.zip binary

*.7z  binary

```



---



## 六、🤖 AI Coding Assistant 行為契約



以下規範**專供 AI 使用**，屬強制性條款。



```text

IMPORTANT – DO NOT MISINTERPRET:



- `.cs` files MUST use UTF-8 with BOM and CRLF.

- `.sh` files MUST use LF.

- Other text files use UTF-8 and CRLF.

- Preserve existing encoding, BOM, and line endings.

- Do NOT normalize or reformat files.

- Avoid any formatting-only diffs.

```



---



## 七、契約地位



- 本文件優先於個人習慣

- 優先於編輯器預設行為

- 優先於 AI 的自動最佳化策略

- 僅次於專案能否存活



