# Selected advisories / 精選安全公告

Verified / 核對日期：**2026-10-23**。These are vendor-published vulnerabilities, not proof that a particular machine was compromised or that every issue is actively exploited. 以下是廠商已公開的漏洞；不代表某台電腦已遭入侵，也不代表每個漏洞都已遭實際利用。

## 1. Node.js — 2026-10-21 security batch

Node.js announced ten issues across the 22.x, 24.x and 26.x lines; individual issues affect different subsets. Examples include Brotli decoding memory corruption with potential remote code execution (CVE-2026-111497) and TLS hostname checks that can accept non-canonical IP representations (CVE-2026-105483 / CVE-2026-106069). Processing attacker-controlled data through an affected path is the relevant concern; merely having an AI account does not establish exposure. The advisory rates the highest severity **High**.

白話：某些有問題的壓縮資料，可能讓使用舊版 Node 的程式執行攻擊者的程式；另有漏洞會讓連線時「確認對方身分」的檢查出錯。仍須符合各漏洞的輸入與使用條件，不能推論所有 AI 使用者都已受害。官方最高評級是 **High（高風險）**。

Minimum fixed releases / 最低修補版本：**22.23.4 / 24.22.1 / 26.12.1**，選自己原本的主版本。Check bundled runtimes and containers separately / 內建 runtime 與容器要分別檢查。

Sources / 來源：[Node advisory](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases), [22.23.4](https://nodejs.org/en/blog/release/v22.23.4), [24.22.1](https://nodejs.org/en/blog/release/v24.22.1), [26.12.1](https://nodejs.org/en/blog/release/v26.12.1).

## 2. Claude Desktop on Windows — CVE-2026-88066

Published 2026-10-13, High. A malicious `git.exe` placed in a project directory could execute when the affected Windows desktop app opened that directory, because executable lookup searched the working directory. Fixed in **1.8089.0**; the vendor recommends the latest version for manual updaters. This is a **Claude Desktop** issue even though its advisory is hosted in the Claude Code repository.

白話：拿到別人的專案，裡面若藏了假 Git，舊版 Windows Claude Desktop 可能在開啟資料夾時就執行它。請更新桌面 App；只更新 Node 或 Claude Code CLI，不能證明這個桌面漏洞已修好。修補門檻為 **1.8089.0**，手動更新者依廠商建議更新到最新版。

Source / 來源：[vendor advisory](https://github.com/anthropics/claude-code/security/advisories/GHSA-4p2j-qj3c-6x84).

## 3. Claude Desktop on macOS — GHSA-v234-4jrq-mgg6

Published 2026-09-25, High. A malicious executable file in a Cowork shared folder could run host commands when opened. Affected **>=1.1.3918, <1.15962.0**; fixed in **1.15962.0**. The advisory also discusses a previously patched guest-kernel vulnerability and its combined impact. Keep that distinction; do not describe the standalone file-handling bug as requiring no click.

白話：Cowork 工作資料夾裡的惡意檔案，可能在你打開時跑到 Mac 主機執行。受影響版本為 **>=1.1.3918、<1.15962.0**，請更新到 **1.15962.0** 以上。官方另說明與舊 VM 核心漏洞串連的情況；單獨這個檔案處理問題，不能直接寫成「完全不用點擊就中招」。

Source / 來源：[vendor advisory](https://github.com/anthropics/claude-code/security/advisories/GHSA-v234-4jrq-mgg6).

## 4. Claude Code plugin pinning — CVE-2026-86063

Published 2026-10-09, High. An attacker controlling an upstream plugin repository on a Git host permitting SHA-shaped branch names could defeat a pinned commit during plugin installation. Requires the user to install the malicious plugin. Fixed in **@anthropic-ai/claude-code 2.1.179**. The description also names Desktop's installation flow, but the version table supplies a Code package minimum; it does not establish a Desktop patch number for this issue.

白話：即使以為外掛已鎖定到審過的版本，在公告條件下仍可能裝到攻擊者的程式。需要安裝惡意來源的外掛才符合這條情境。Claude Code 至少更新到 **2.1.179**；公告沒有為此列出 Desktop 的修補版本，不能把 CLI 版本號套到桌面 App。

Source / 來源：[vendor advisory](https://github.com/anthropics/claude-code/security/advisories/GHSA-pq7j-f95f-qfcv).

## 5. MCP OAuth credentials — CVE-2026-104850

Published 2026-09-30, High. An affected HTTP OAuth client holding credentials could send them to an authorization server selected by an untrusted MCP server. Affects `@modelcontextprotocol/sdk` **1.12.0–1.30.1** and specific `@modelcontextprotocol/client` **2.0.0–2.1.0** configurations. **stdio clients and SDK servers are excluded from this advisory.**

白話：某些走 HTTP／OAuth 的 MCP 客戶端，連到惡意 MCP 時，可能把既有登入權杖或客戶端密鑰交給對方。不是「所有 MCP 都中」，本公告不包含 stdio 客戶端與 SDK 伺服器。

Fix / 修補：SDK **1.31.0+**，或 client **2.2.0+**；若直接匯入 core，也需 **2.2.0+**。For the other selected SDK 1.x advisories in this repo, use **1.32.0+**. 本 Repo 另列的 SDK 1.x 問題，門檻為 **1.32.0+**。

**Upgrading alone can be insufficient / 只升級可能不夠：**

- Bundled providers need `expectedIssuer`; 舊有內建 provider 需指定正確 `expectedIssuer`。
- Previously stored credentials need the correct `issuer`, or must be cleared through the application's supported flow so the user signs in again; 舊憑證要補正確 issuer，或依 App 支援流程清除後重新登入，不能隨便填攻擊者提供的 issuer。
- Custom providers must persist the full values, including `issuer`; 自訂儲存邏輯要完整保存 issuer。
- If exposure to an untrusted server may have occurred, rotate the relevant secret/signing key and revoke tokens; 若可能已暴露，依官方建議輪替相關密鑰並撤銷權杖。

Direct `refreshAuthorization()` / `exchangeAuthorization()` calls, interactive new sign-in and 2.x `skipIssuerMetadataValidation: true` have separate caveats in the official advisory. 這些情況請直接照官方限制處理；本工具不讀取或修改憑證。

Source / 來源：[vendor advisory and full migration conditions](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-6qxp-vccf-f47h).

## 6. MCP HTTP client isolation — CVE-2026-111770

Published 2026-10-15, High. Sharing one server or stateless transport across multiple HTTP clients could deliver one client's responses or notifications to another. Affects `@modelcontextprotocol/server` **2.0.0–2.2.0**. stdio servers and SDK 1.x >=1.26.0 are excluded from this advisory.

白話：多人連同一個 HTTP MCP 服務，若程式共用錯誤的實例，甲的回覆可能跑到乙那裡。server 套件 **2.0.0–2.2.0** 受影響；不包含 stdio 伺服器與本公告排除的 SDK 1.x >=1.26.0。

Fix / 修補：**server 2.3.0+ AND code changes**：create server/transport per request or session, or use `createMcpHandler()` with a fresh-server factory. **還要改程式**，讓每個 request/session 使用自己的實例，或提供每次建立新 server 的 factory。升級後直接共用實例可能改成拒絕請求；請測試多使用者隔離與錯誤回傳。

Source / 來源：[vendor advisory](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-jm63-cq47-j5jp).

Additional selected SDK 1.x advisories / 另列的 SDK 1.x 公告：experimental task isolation / 實驗性任務隔離 [CVE-2026-111767](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-22jm-h49p-29qw), and bearer token audience validation / bearer 權杖適用服務檢查 [CVE-2026-111771](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-rvq5-wwqv-78pq). Both list **SDK 1.32.0** as a fix. Read each advisory for other split-package versions; this checker does not cover all split packages.

## 7. n8n project isolation — GHSA-wvjj-qg37-pv73

Published 2026-10-14, High. A member who can create and run workflows could read, change or delete Data Table rows in other projects. Fixed in **2.43.5** and **2.44.1** on the respective maintenance branches. Version 2.44.0 is affected even though it is numerically newer than 2.43.5. The advisory body does not establish a 1.x fix; the checker reports 1.x for review.

白話：同一套 n8n 裡，能建立與執行工作流的成員，可能越權操作別的專案資料表。2.43 分支至少 **2.43.5**，2.44 分支至少 **2.44.1**；不能看到 2.44.0 比 2.43.5 大就認為安全。先備份資料，再依自己的部署方式更新。此公告正文未建立 1.x 的修補門檻，不能自行補一個。

Source / 來源：[vendor advisory](https://github.com/n8n-io/n8n/security/advisories/GHSA-wvjj-qg37-pv73).
