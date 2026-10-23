# AI Security Checkup｜AI 工具資安檢查與更新指南

[Full English documentation](README.md)

這是一份小型、離線的檢查工具與操作指南，整理**部分 AI 開發工具的安全公告**，已於 **2026-10-23** 核對廠商來源。先看[白話說明](docs/ADVISORIES.md)與[更新步驟](docs/UPDATE.zh-TW.md)。

只在網站上和 AI 聊天，不代表你的電腦正在執行有漏洞的 Node.js。本機 CLI、MCP、桌面 App、自架工作流要分別盤點。版本落在公告範圍，表示需要處理版本問題；能否被利用，還取決於公告列出的條件。本專案沒有宣稱列出的漏洞全部正在遭到攻擊。

## 一分鐘開始檢查

按 GitHub 的 **Code → Download ZIP**，解壓縮後在該資料夾開啟終端機。也可以用 Git：

```sh
git clone https://github.com/leavemagic-cyber/ai-security-checkup.git
cd ai-security-checkup
node check.mjs
```

不需要 `npm install`、帳號、API 金鑰或管理員權限。執行前可以直接閱讀短小的原始碼。工具只使用 Node 內建功能，不連網、不執行其他程式、不改設定。用舊版 Node 執行檢查，不會順便修好它。如果沒有安裝 Node，而且只用網頁版 AI，直接看手動指南，不必為了檢查而安裝 Node。

Node.js 2026-10-21 這批公告的修補門檻：

| 原本的主版本 | 這批漏洞至少更新到 |
| --- | --- |
| 22.x | 22.23.4 |
| 24.x | 24.22.1 |
| 26.x | 26.12.1 |

這是有日期的最低門檻，之後仍可能需要新修補。請選符合現有主版本及相容性需求的最新受支援修補版。未列出的分支與預覽版會顯示 `REVIEW`，不會因為版本數字較大就被判定安全。[Node 官方公告](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases)。

## 加查專案或指定版本

```sh
# 讀取指定專案的 npm package-lock.json，包括巢狀相依套件
node check.mjs --project /path/to/your/project

# 比對手動提供的版本；不代表已檢查正在執行的 App
node check.mjs --component @modelcontextprotocol/client@2.1.0
node check.mjs --node-version 24.21.0

# JSON 結果可能包含私人本機路徑
node check.mjs --json
```

Windows 專案路徑可寫成 `--project "C:\your-project"`。要另外查看目前名稱為 `node` 的執行中程序，其檔案版本與路徑，可以選擇執行：

```powershell
powershell -NoProfile -File .\scripts\Inspect-RunningNode.ps1
```

如果公司政策擋住腳本，請照手動盤點步驟處理，不要關閉政策。這個 PowerShell 輔助工具只讀取檔案版本資訊與路徑，不會停止或更新程序。磁碟上的版本資訊，也不能證明舊程序已經載入更新後的檔案。

## 結果怎麼看

| 狀態 | 意思 |
| --- | --- |
| `AFFECTED_VERSION` | 版本落在已列出的受影響範圍；核對觸發條件並處理。 |
| `MEETS_BASELINE` | 這份 Node 的版本達到本次公告門檻。 |
| `NO_VERSION_MATCH` | 此套件沒有命中這條公告的版本範圍；其他漏洞未檢查。 |
| `MANUAL_CHECK` | 只看版本不能確認舊憑證已處理，或連線隔離已修改。 |
| `REVIEW`／`INCOMPLETE` | 版本或分支不明、格式不支援、讀取失敗，不能判定通過。 |

結束碼：**0**＝沒有列出的版本警示，**1**＝有受影響版本，**2**＝資料不足或需要人工確認。若同時有受影響版本與人工確認項目，以 2 為準。沒有套件命中，不代表專案或整台電腦安全。

## 檢查範圍與限制

- 比對執行此工具的 Node、明確提供的版本，以及選擇讀取的一份 npm v1／v2／v3 lockfile；資料來自 `advisories.json`。
- 套件精選範圍：Claude Code 外掛版本釘選、MCP TypeScript SDK 的 OAuth／任務／權杖問題、MCP 2.x HTTP 伺服器共用實例、n8n Data Table 專案隔離。每條都有條件與官方來源。
- PATH 中其他 Node 路徑只列出，**不會執行或檢查其版本**。版本管理器裡其他副本、未啟動 App、Electron、App 內建 runtime、其他使用者、Docker、WSL、CI 與遠端主機，都不會自動完整盤點，請逐一照指南檢查。
- Lockfile 記錄預期解析的版本，不能證明安裝、建置或執行中的版本。工具不支援 pnpm、Yarn、Bun 與 Python lockfile；要求讀取的 npm lockfile 缺少或損壞會回報資料不足。
- 工具不讀憑證、MCP 設定或 OAuth 儲存內容，不攻擊目標、不偵測入侵、不安裝修補、不上傳報告。結果可能帶有本機路徑，公開前請遮除。
- 更新系統 Node，不保證 App 內建副本也更新。請透過廠商支援的更新方式處理，安排重新啟動後再查。更新也不會撤銷已被偷走的權杖；如有可能暴露，依公告處理。
- 這是**有日期的資料快照**，沒有背景監控或自動更新漏洞庫。之後使用時，先查目前官方公告。

## 閱讀、更新與分享

- [公告與受影響條件（中英對照）](docs/ADVISORIES.md)
- [更新與驗證](docs/UPDATE.zh-TW.md) · [English update guide](docs/UPDATE.md)
- [可複製的 Threads 貼文](social/THREADS.zh-TW.md)
- [安全回報與限制](SECURITY.md)

測試使用合成版本與暫存資料：`node --test`。測試執行器需 Node 20 以上，建議使用已修補且受支援的版本。CI 涵蓋 Windows、macOS 與 Linux，沒有附攻擊程式。

本專案由 AI 協助整理，公告事實已核對文內廠商來源。社群專案，與文內廠商沒有隸屬關係。採 MIT 授權。
