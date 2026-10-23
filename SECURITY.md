# Security and scope

This project provides plain-language AI prompts and dated guides, with an optional offline version checker. The prompt asks the user's AI to check official sources, explain personal impact, obtain consent before updates, and verify results. Actual access and execution depend on that AI's tools and permissions; copying the prompt does not scan or patch a device by itself. Chat-only assistants must disclose their limits and guide one screen at a time.

The optional Node checker is defensive and offline. It is not an exploit tool, a full vulnerability feed or an intrusion detector. The repository implements no telemetry, credential reads or background monitoring. The AI workflow uses official online sources and may help perform agreed updates, so its actions should not be confused with the offline checker's scope.

The Windows GUI preview is separate: it reads selected installed-app records, executable version metadata and limited Python package metadata without executing Python/Node. Only explicit per-session consent allows scoped WinGet upgrades for recognized Chrome, standard Firefox and standalone MSI Node installations; Node remains on its existing supported major. It does not bypass source/package agreements, hashes, pins, security policies or restart the computer. WinGet contacts its existing source. No scan results are uploaded by this tool; optional diagnostics can contain private paths. App-bundled runtimes, compromised LiteLLM cleanup, independent trust stores and remote PHP hosts are not automatically repaired. Real update installation end-to-end acceptance is not yet established; see [preview coverage and test evidence](docs/WINDOWS.zh-TW.md).

For errors in advisory interpretation or version boundaries, open an issue with the official public source and a synthetic reproduction. Do not post private project lockfiles, tokens, home paths or host inventories. For a new vulnerability in an upstream product, use that vendor's private disclosure channel.

For a security issue in this checker, use GitHub's private vulnerability reporting when available. Otherwise request a private reporting channel without publishing exploit details or sensitive data. Private reporting availability is not asserted by this document.

本專案以白話 AI 提示詞與有日期的指南為主，另提供選用的離線版本檢查工具。提示詞要求 AI 查官方資訊、解釋個人影響，先取得同意才更新，最後確認生效；能代做多少取決於該 AI 實際具備的工具與權限。貼上提示詞本身不等於已掃描或修補，純聊天 AI 必須明說限制並逐步帶你點選。

Repo 不蒐集檢查結果、不讀密碼或金鑰、不做背景常駐，也不判定入侵。選用 Node 檢查工具只做離線版本比對；AI 工作流程則會查線上官方來源，並可協助執行已同意的更新，兩者範圍不同。

Windows 圖形預覽版另提供常見位置的檢查，以及使用者明確同意後的部分 WinGet 更新；Node 留在原主版本。它不執行 Python／Node、不直接覆寫 App 內附元件、不自動清理惡意 LiteLLM 或更新遠端 PHP，也不驗證所有憑證封鎖。不略過條款、雜湊、安全政策或強制重啟。更新會連線至既有來源，詳細診斷可能包含私人路徑。真實安裝的端到端升級驗收尚未完成，請參閱[預覽版驗證範圍](docs/WINDOWS.zh-TW.md)。

回報公告解讀或版本錯誤時，附官方來源與合成範例即可，不要公開私人 lockfile、密鑰、家目錄或整台主機盤點。上游產品的新漏洞請走該廠商私密通報管道；本工具的漏洞優先使用可用的 GitHub 私密回報，若未提供則先詢問私密管道。
