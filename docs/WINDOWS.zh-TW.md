# Windows 雙擊小幫手（預覽版）

**不用 AI、不用帳號、不用另外安裝 Node，也不用開終端機。**

[下載 SecurityCheckup.exe](https://github.com/leavemagic-cyber/ai-security-checkup/releases/download/v0.4.0/SecurityCheckup.exe) · [發布說明與檔案校驗值](https://github.com/leavemagic-cyber/ai-security-checkup/releases/tag/v0.4.0) · [回首頁](../README.zh-TW.md)

## 怎麼用

1. 下載後雙擊 **SecurityCheckup.exe**。
2. 按 **開始檢查**。這一步只查安裝紀錄與程式檔，不會更新。
3. 有可自動更新項目時，按更新按鈕。工具會列出項目與影響；先存好工作，再選 **同意並開始更新**。
4. 看結果，依相關 App 正常方式重啟，再確認能正常使用。**下載或安裝完成，還不等於所有修補已生效。**

這是第一個 Windows 預覽版，介面為繁體中文。需要 Windows 10／11 與 .NET Framework 4.8；自動更新還需要系統原有的 Windows 套件管理員（WinGet）和網路。沒有 WinGet 時會保留檢查結果，改用[免下載懶人包](NO-AI.zh-TW.md)，不會自行安裝額外工具。

目前沒有程式碼簽章。若 Windows 或公司政策不允許執行，請使用[免下載教學](NO-AI.zh-TW.md)，不要關閉防護或略過安全警告。

## 哪些可以自動處理

| 找到的項目 | 處理方式 |
| --- | --- |
| Chrome、Firefox 一般版 | 實際檔案低於本次門檻、且能確認原安裝時，你同意後透過 WinGet 更新。 |
| 獨立 MSI 安裝的 Node | 留在原主版本，指定本次安全修補版；找不到相符套件／版本時不會改裝另一份。 |
| App 內藏 Node、版本管理器、可攜版、Firefox ESR | 顯示待處理，請用原 App／通道更新，不直接覆寫內附元件或換主版本。 |
| LiteLLM | 靜態查部分常見安裝紀錄；不啟動 Python／pip，不把升級當成惡意程式清除。 |
| 自己網站的 PHP | 遠端網站未檢查；用[可複製給主機商的文字](NO-AI.zh-TW.md#我有自己的網站)。 |

「沒有可自動更新的項目」可能代表需要手動處理、已達本次檔案門檻、未找到或資料不足，**不代表整台電腦安全**。瀏覽器憑證封鎖清單沒有自動驗證；App 是否已載入新檔案、能否正常運作，仍需重啟後確認。

工具只查常見位置與目前使用者，不盤點整台硬碟、所有 App 私有環境、容器、WSL、其他使用者、歷史安裝或遠端服務。未找到某個套件不代表它從未存在。若發現受污染 LiteLLM 紀錄，依[官方事件指引](https://docs.litellm.ai/blog/security-incident-oct-21-2026)隔離、乾淨重建並處理可能暴露的憑證；本工具不會自動完成這些工作。

## 更新與隱私

只在你同意後，才使用系統既有的 WinGet 及 `winget` 來源，查詢原安裝對應的軟體並更新。[Microsoft 官方更新說明](https://learn.microsoft.com/en-us/windows/package-manager/winget/upgrade)。更新來源會收到查詢；不是離線更新。

不整批升級其他軟體、不跳過雜湊或安全檢查、不自動接受額外條款，也不強制結束你的 App 或重啟電腦。需要權限、條款、原安裝通道或版本不可取得時，會回報未完成。

本工具沒有遙測、不讀密碼或金鑰、不上傳檢查結果。選看「詳細資料」時可能顯示本機路徑與 WinGet 診斷，請不要整份公開。WinGet 及安裝程式本身仍依其設定與廠商政策運作。

## 這個版本驗證到哪裡

本機已完成實際只讀檢查、圖形介面檢視，以及 27 項關鍵測試，包含不同意不執行、過期檢查結果不執行、Node 不跨主版本、不降版、來源找不到不另裝、版本不明不判定通過、安裝回傳成功仍重查檔案、LiteLLM 靜態紀錄與相似檔名不誤判。GitHub Windows 建置會重新編譯及執行這些測試。

**自動升級的安裝路徑目前使用合成回應測試；尚未在真實需要升級的安裝上完成端到端更新驗收。** 因此先以預覽版提供；不要把測試通過當作每一台電腦都已實測。核對日期：2026/10/23。

有幫助請分享 [GitHub](https://github.com/leavemagic-cyber/ai-security-checkup)，也歡迎按 ⭐。卡住可[回報](https://github.com/leavemagic-cyber/ai-security-checkup/issues)裝置、App 及步驟；不要提供密碼、金鑰或私人資料。
