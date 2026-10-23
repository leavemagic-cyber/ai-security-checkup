# 如何檢查、更新，再確認真的生效

這是給 AI 操作者與熟悉指令的人的進階參考。**沒有資訊背景，請直接到[首頁複製提示詞](../README.zh-TW.md)，讓 AI 幫你查、問過你再協助更新；不需要自己執行下面的指令。**

核對日期：2026-10-23。先確認自己使用哪一種安裝方式，再選對應步驟。一般網頁版 AI 使用者，主要維護瀏覽器與作業系統；只有登入 AI 網站，不能推論本機裝了有漏洞的 Node。

## 1. 先盤點，分清楚每一份

在你平常啟動工具的終端機執行：

```sh
node --version
node -p "process.execPath"
claude --version
codex --version
```

沒有安裝的指令可略過。Windows 再用 `where.exe node`；macOS／Linux 可用 `which -a node`。這些只看當前環境，不能完整找出 App、WSL、容器或別的使用者持有的副本。

若使用 npm 全域套件，可查看：

```sh
npm ls -g --depth=0
```

專案則用本 Repo 檢查 lockfile：

```sh
node check.mjs --project /path/to/project
```

也可在該專案目錄執行 `npm ls @modelcontextprotocol/sdk @modelcontextprotocol/client @modelcontextprotocol/server n8n --all`，查看目前安裝樹；不會因此自動完成修補，退出碼也可能反映相依套件問題。

## 2. 更新 Node.js：選現有主版本的受支援修補版

這批最低版本為 **22.23.4／24.22.1／26.12.1**。[官方公告](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases)。之後若有新版本，優先選符合相容性要求的最新受支援修補版。

- **官方安裝程式**：到 [Node 官方下載頁](https://nodejs.org/en/download)，或公告連結的該主版本 release 頁選對作業系統與架構，再更新。重新開啟終端機後確認實際路徑與版本。
- **nvm、fnm、Volta 等版本管理器**：沿用原本工具，安裝同主版本已修補的版本，再切換目前 shell 及專案釘選；不能只裝新版本，卻仍讓專案選到舊版。
- **Homebrew、WinGet、apt 等套件管理器**：先查已安裝的套件名稱、來源及版本，針對那個套件更新。不同通道可能改變主版本，不要把網路上的通用升級指令直接套到正式環境。
- **Node 20 或其他已停止支援分支**：規劃升到受支援 LTS，測試相容性。[官方時程](https://github.com/nodejs/Release/blob/main/schedule.json)列出 Node 20 已於 2026-04-30 結束支援。單純停在該分支最後一版，不能視為取得這批修補。

先前處理請求的服務仍可能使用舊程序。保存工作後，安排重啟相關服務，確認實際新啟動的 runtime；不要停止不屬於你的工作。

## 3. 更新 Claude Code 與桌面 App

先確認安裝來源。Claude Code 官方的手動更新指令為：

```sh
claude update
claude --version
```

若由 Homebrew、WinGet 或公司管理，沿用該來源。[Claude 官方安裝與更新文件](https://code.claude.com/docs/en/setup)。本文列的外掛釘選漏洞，CLI 最低修補版為 **2.1.179**；更新到當前受支援版本也能包含其他修補。

Claude Desktop 透過桌面 App／官方商店提供的更新方式處理，保存工作後重新啟動，再看 App 的版本。Windows 假 Git 漏洞門檻 **1.8089.0**；macOS Cowork 檔案問題門檻 **1.15962.0**。版本號與 CLI 不同，不要混用。[Windows 公告](https://github.com/anthropics/claude-code/security/advisories/GHSA-4p2j-qj3c-6x84)、[macOS 公告](https://github.com/anthropics/claude-code/security/advisories/GHSA-v234-4jrq-mgg6)。

Codex、Cursor、VS Code 等 App 也可能帶自己的 runtime：沿用各自官方更新方式。npm 安裝的 Codex CLI 可針對原套件更新，再確認版本：

```sh
npm install -g @openai/codex@latest
codex --version
```

其他 Codex 安裝方式依 [OpenAI 官方 CLI 文件](https://learn.chatgpt.com/docs/codex/cli)。這是維護指引，**不代表本文證明所有這些 App 都可利用這批 Node 漏洞**。不要手動覆蓋 App 管理的內建 Node。看到「需要重新啟動」後，先保存工作，再由使用者安排重啟；重啟後仍要重新檢查內建 runtime，不能假設更新一定已包含修補。

## 4. MCP：套件、憑證、程式隔離都要處理

如果你只是使用某個現成 App 裡的 MCP，請更新該 App／整合套件；在其他專案執行 `npm install` 不會修好那個 App。

如果你維護 MCP 程式：

1. 用 lockfile／安裝樹找出真正依賴版本，包括轉接套件與巢狀副本。只改 `package.json` 的文字不足以證明更新。
2. 本文精選 SDK 1.x 問題至少使用 **@modelcontextprotocol/sdk 1.32.0**。拆分套件的 OAuth client 至少 **2.2.0**；直接匯入 core 也至少 **2.2.0**。server 共用實例問題至少 **2.3.0**。這些是不同套件，不是同一個全域 MCP 版本。
3. 以專案原本套件管理器更新相應依賴與 lockfile，再建置與測試；若是上層套件帶入，更新上層套件，不要假設全域安裝可以覆蓋它。
4. 依 [OAuth 公告](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-6qxp-vccf-f47h)處理 `expectedIssuer`、舊憑證的 `issuer`、自訂 provider 儲存邏輯。工具不會替你讀取或清除憑證。若可能已連到惡意伺服器，撤銷權杖並輪替相關密鑰。
5. 依 [隔離公告](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-jm63-cq47-j5jp)改成每個 request/session 建立自己的 server／transport，或使用會新建 server 的 factory。測試兩個使用者同時請求，確認回覆不混流，並檢查升級後是否出現拒絕連線。

## 5. 自架 n8n、Docker、WSL、CI

- n8n 先備份資料庫、設定及持久資料，核對還原方式與相容性。本文 Data Table 公告修補版為 **2.43.5** 或 **2.44.1**，依所用分支選擇；使用目前受支援版本前先確認官方升級路徑。[n8n 官方更新指南](https://docs.n8n.io/hosting/installation/updating/)。
- Docker 要更新 Dockerfile／Compose 指定的 Node 或應用程式映像、重新建置或拉取，並重新部署。不要刪除資料卷來做安全更新；宿主機 Node 更新不會改容器裡的 Node。
- 可在自己的容器用 `docker exec CONTAINER_NAME node --version` 與 `docker exec CONTAINER_NAME node -p "process.execPath"` 檢查；實際名稱替換成自己的容器。
- WSL、遠端主機與 CI runner 各自檢查。CI 的 Node 設定、容器映像與專案釘選都要更新，並確認實際 job 使用的版本。

## 6. 完成的標準

保存工作並安排相關程式重新啟動後，重新確認實際路徑、執行版本、依賴安裝樹、必要的功能與隔離測試。檢查前後結果可以留在自己的電腦，不要上傳含密鑰、私人專案或本機路徑的原始報告。

沒有看見異常不代表沒有受害；單一版本警示也不代表已遭入侵。若有明確可疑存取紀錄，另做事件調查與憑證處理。本 Repo 不會自動做入侵判定。
