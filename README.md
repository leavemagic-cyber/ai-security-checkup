# AI Security Checkup — let AI help you check and update

[完整繁體中文](README.zh-TW.md)

**Saw a security warning and don't know whether it affects you? Paste the prompt below into your AI. Let it investigate, ask, help you update and verify the result.**

Some vulnerabilities can expose account access or files, or allow someone else's code to run on your computer. Your actual apps and versions determine exposure. You do not need to understand vulnerability identifiers first.

## Three steps

1. **Copy the prompt below into your AI.**
2. **Let it investigate and explain what needs attention in plain language.**
3. **When ready, reply “Yes, help me update.”** Let it help complete the update and verify the fix.

No repository download, Node installation or command-line knowledge is needed for this workflow. An AI with computer access can help execute updates. A chat-only AI must disclose its limits and guide you one screen at a time. The same prompt can guide phone updates. **A prompt cannot grant capabilities the AI does not have.**

## Copy and paste this prompt

Use the code block's copy button, or open the [standalone prompt](prompts/QUICK-UPDATE.md).

````
Be my personal computer security helper. I have no technical background. Investigate for me and help me complete the fixes in plain language.

Check first; do not update immediately. Establish whether you can read or control my device. Retrieve information available to you instead of asking me to open a terminal, find version numbers or read code. If you cannot inspect the device, say so and guide me one screen at a time; never pretend you scanned it. Ask what device I use only if necessary.

Follow this workflow:

1. Identify the operating system, browsers, everyday applications and AI tools I actually use. Prioritize official security announcements from the last seven days that affect individuals, plus older serious issues that remain unpatched. Check products I use; do not install new tools just to inspect, or scan private documents. Failure to locate a program is not proof it is absent.

2. Compare current vendor advisories with actual installed versions, installation sources and exposure conditions. Explain which app, what could happen and whether I need to act now; show at most the five most important items first. Link official sources, but explain rather than relying on vulnerability identifiers or jargon. Distinguish confirmed affected versions, ordinary update recommendations and unresolved checks.

3. Prepare a workable update route, then ask: “Would you like me to update these items?” State which apps, any cost, required closures or restarts, and potential loss of unsaved work. Wait for my explicit “Yes, help me update” before proceeding. This prompt is not update consent.

4. After I agree, complete the actions you can perform. Use the existing official updater, official store or existing package manager, and update only the agreed items. Do not install unknown repair software, weaken protections, delete my data or indiscriminately upgrade everything. Explain and ask again if a different major version, payment or additional material impact becomes necessary. Guide me only where sign-in, system permissions or your tool rules require my own action. Never request or display passwords, keys or verification codes.

5. Recheck the version and state actually in use and confirm normal operation. Applications can bundle old components; updating an external copy does not prove every copy is fixed. If an advisory requires signing in again, revoking potentially exposed access or other follow-up work, guide me in plain language. Clearly state incomplete items, why they remain and the simplest next action. A completed download is not a verified fix.

Finish briefly: what was handled, what remains and whether I can continue normal use. Never publish or upload my device information.
````

## What needs attention recently?

[Read the personal-user alerts and simplest actions](docs/PERSONAL-ALERTS.md). Each entry has a date and official source. Do not install products you do not use just to patch them.

Recent issues extend beyond AI tools to browsers, operating systems and everyday apps. The prompt asks the AI to verify current vendor information instead of treating this repository's dated version table as permanently current.

**If this saved time or helped you finish an update, please give the repository a ⭐ and share it with someone who needs it.** It is free to use; no star or personal data is required.

Prepared with AI assistance. Advisory facts are dated and linked to vendor sources. This project does not detect intrusions or promise that an entire device is secure.

<details>
<summary>Advanced: optional offline checker and full technical documentation</summary>

A small, offline checklist and dependency-free checker for **selected AI development tooling advisories**, verified against vendor sources on **2026-10-23**. Start with the [plain-language advisory guide](docs/ADVISORIES.md) and [update steps](docs/UPDATE.md).

Using an AI website does not by itself mean your computer runs the affected Node.js. Local CLIs, MCP integrations, desktop apps and self-hosted workflows need their own inventory. A matching version indicates a version-level issue; exploitability still depends on the advisory's conditions. This project makes no claim that every listed issue is actively exploited.

## Check in a minute

Download this repository using GitHub's **Code → Download ZIP**, extract it and open a terminal in the extracted directory. Or clone it:

```sh
git clone https://github.com/leavemagic-cyber/ai-security-checkup.git
cd ai-security-checkup
node check.mjs
```

No `npm install`, account, API key or administrator access is needed. Read the short source before running. The checker uses only Node built-ins, makes no network requests, executes no other binaries and changes no settings. Running the checker in an older Node runtime does not patch that runtime. If Node is not installed, use the manual guide instead of installing it just to check a web-only AI account.

The 2026-10-21 Node.js advisory's fixed baselines are:

| Existing major | Minimum fixed release for this batch |
| --- | --- |
| 22.x | 22.23.4 |
| 24.x | 24.22.1 |
| 26.x | 26.12.1 |

These are dated minimums, not a promise that no later patches are needed. Use the latest supported patch appropriate for your existing major and compatibility requirements. Unlisted branches and prereleases receive `REVIEW`; an unlisted/newer major is never automatically declared safe. [Official Node advisory](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases).

## Optional checks

```sh
# Read one project's npm package-lock.json, including nested copies
node check.mjs --project /path/to/your/project

# Compare an explicitly supplied version; this does NOT inspect a running app
node check.mjs --component @modelcontextprotocol/client@2.1.0
node check.mjs --node-version 24.21.0

# Machine-readable output (local paths can be private)
node check.mjs --json
```

On Windows, use a quoted project path such as `--project "C:\your-project"`. To inspect metadata for currently running processes named `node`, optionally run:

```powershell
powershell -NoProfile -File .\scripts\Inspect-RunningNode.ps1
```

If your organization's policy blocks the script, use the manual inventory steps; do not disable that policy. The PowerShell helper only reads file-version metadata and paths. It does not stop or update processes. Metadata on disk cannot prove an existing process loaded new bytes.

## Understand the result

| Status | Meaning |
| --- | --- |
| `AFFECTED_VERSION` | Version is in a listed affected range; check exposure conditions and remediate. |
| `MEETS_BASELINE` | This Node version meets this dated batch's minimum only. |
| `NO_VERSION_MATCH` | No affected range matched for this package/advisory; other vulnerabilities remain unchecked. |
| `MANUAL_CHECK` | A package version cannot establish required credential migration or correct connection isolation. |
| `REVIEW` / `INCOMPLETE` | Unknown version/branch, unsupported input or read failure; no passing conclusion. |

Exit codes: **0** = no listed version flags, **1** = an affected version, **2** = incomplete or manual review required. Code 2 takes precedence if both an affected version and an incomplete/manual result exist. An empty list of package matches does not prove the project or machine is secure.

## Coverage and limits

- Checks the executing Node runtime, explicitly supplied versions and an optional npm v1/v2/v3 lockfile against `advisories.json`.
- Selected package coverage: Claude Code plugin pinning, MCP TypeScript SDK OAuth/task/token issues, MCP 2.x shared HTTP server instances and n8n Data Table project isolation. See each entry's conditions and official source.
- PATH candidates are listed, **not executed or version-checked**. Other version-manager installations, stopped apps, Electron, bundled app runtimes, other users, Docker, WSL, CI and remote servers are not automatically inventoried. Use the guide for each environment.
- A lockfile is intended resolution, not proof of the installed, built or running version. pnpm, Yarn, Bun and Python lockfiles are unsupported. A requested missing or invalid npm lockfile is an incomplete result.
- The tool does not inspect credentials, MCP configuration or OAuth storage, exploit targets, detect intrusions, install patches or upload reports. Reports may contain local paths; redact them before sharing.
- Updating system Node does not necessarily update an app's bundled copy. Use the app vendor's supported updater and recheck after a deliberate restart. Updates do not revoke already stolen tokens; follow the advisory if exposure is plausible.
- This is a **dated snapshot**. There is no background monitor or automatic catalog refresh. Check current official advisories before acting later.

## Read, update and share

- [Advisories and exposure conditions / 漏洞白話說明](docs/ADVISORIES.md)
- [Update and verify](docs/UPDATE.md) · [更新與驗證（繁中）](docs/UPDATE.zh-TW.md)
- [Copyable Threads post](social/THREADS.zh-TW.md)
- [Security reporting and limitations](SECURITY.md)

Tests use synthetic versions and temporary fixtures: `node --test` (Node 20+ for the test runner; supported patched runtimes are recommended). CI checks Windows, macOS and Linux. No exploit payloads are included.

Prepared with AI assistance; advisory statements were checked against the linked vendor sources. Community-maintained, unaffiliated with the named vendors. MIT license.

</details>
