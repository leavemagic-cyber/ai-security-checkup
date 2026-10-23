安全更新小幫手（Windows）

1. 雙擊 SecurityCheckup.exe。
2. 按「開始檢查」。這一步不會更新。
3. 有可自動更新項目時，按更新按鈕；看清楚清單及影響，再選「同意並開始更新」。
4. 閱讀結果。先存好工作，再依相關 App 正常方式重新啟動、確認能用。

不需要 AI、帳號、Node 或開終端機。Windows 10/11、.NET Framework 4.8；自動更新需要系統既有 WinGet 及網路。
第一版介面為繁體中文。工具沒有程式碼簽章；如果 Windows 或公司政策不允許執行，改用下方免下載教學，不要關閉防護或略過安全警告。

自動更新範圍：能確認原安裝方式的 Chrome、Firefox 一般版，以及獨立 MSI 安裝、仍在本次支援主版本的 Node。
Node 只更新到同主版本的本次修補版。App 內藏 Node、ESR、版本管理器、可攜版與其他不明安裝方式需由原更新通道處理。
LiteLLM 只靜態讀取部分常見 Python 安裝紀錄，不會啟動 Python／pip，也不會自動清理惡意版本或更換憑證。
不檢查遠端網站／PHP、不檢查整台硬碟、容器、WSL、其他使用者、所有 App 私有環境或歷史安裝。
瀏覽器檔案更新不等於已驗證 eMudhra 憑證封鎖；檔案版本符合門檻不代表整台電腦安全。
工具不會強制關掉程式或重新啟動電腦，不會讀取密碼、金鑰或上傳檢查結果。
更新會透過 WinGet 查詢更新來源，仍受 Windows／公司政策、來源條款及原安裝方式限制。無法完成的項目會保留為待處理。
詳細結果可能含本機路徑，請不要整份公開。

本次公告門檻核對：2026/10/23。之後使用仍應查看當下官方資訊。
原始碼與回報：https://github.com/leavemagic-cyber/ai-security-checkup
免下載教學：https://github.com/leavemagic-cyber/ai-security-checkup/blob/main/docs/NO-AI.zh-TW.md
官方 WinGet 更新說明：https://learn.microsoft.com/en-us/windows/package-manager/winget/upgrade
