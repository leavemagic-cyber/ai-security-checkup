# Security and scope

This project provides plain-language AI prompts and dated guides, with an optional offline version checker. The prompt asks the user's AI to check official sources, explain personal impact, obtain consent before updates, and verify results. Actual access and execution depend on that AI's tools and permissions; copying the prompt does not scan or patch a device by itself. Chat-only assistants must disclose their limits and guide one screen at a time.

The optional checker is defensive and offline. It is not an exploit tool, a full vulnerability feed or an intrusion detector. The repository implements no telemetry, credential reads, automatic patching or background work. The AI workflow uses official online sources and may help perform agreed updates, so its actions should not be confused with the offline checker's scope.

For errors in advisory interpretation or version boundaries, open an issue with the official public source and a synthetic reproduction. Do not post private project lockfiles, tokens, home paths or host inventories. For a new vulnerability in an upstream product, use that vendor's private disclosure channel.

For a security issue in this checker, use GitHub's private vulnerability reporting when available. Otherwise request a private reporting channel without publishing exploit details or sensitive data. Private reporting availability is not asserted by this document.

本專案以白話 AI 提示詞與有日期的指南為主，另提供選用的離線版本檢查工具。提示詞要求 AI 查官方資訊、解釋個人影響，先取得同意才更新，最後確認生效；能代做多少取決於該 AI 實際具備的工具與權限。貼上提示詞本身不等於已掃描或修補，純聊天 AI 必須明說限制並逐步帶你點選。

Repo 本身不蒐集資料、不讀憑證、不實作自動修補或背景常駐，也不判定入侵。選用檢查工具只做離線版本比對；AI 工作流程則會查線上官方來源，並可協助執行已同意的更新，兩者範圍不同。回報公告解讀或版本錯誤時，附官方來源與合成範例即可，不要公開私人 lockfile、密鑰、家目錄或整台主機盤點。上游產品的新漏洞請走該廠商私密通報管道；本工具的漏洞優先使用可用的 GitHub 私密回報，若未提供則先詢問私密管道。
