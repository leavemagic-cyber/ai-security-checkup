# Check, update and verify actual use

Verified: 2026-10-23. Identify the installation method before choosing an update path. People using AI only through a website primarily maintain their browser and operating system; signing in does not establish a local vulnerable Node installation.

## 1. Inventory each copy

In the terminal where you normally launch the tool:

```sh
node --version
node -p "process.execPath"
claude --version
codex --version
```

Skip commands for tools you do not have. On Windows also use `where.exe node`; on macOS/Linux, `which -a node`. These describe the current environment, not every app, WSL distribution, container or user.

For npm global installations:

```sh
npm ls -g --depth=0
```

For a project lockfile:

```sh
node check.mjs --project /path/to/project
```

In that project's directory, `npm ls @modelcontextprotocol/sdk @modelcontextprotocol/client @modelcontextprotocol/server n8n --all` can also show the installed dependency tree. This does not patch it; exit codes may also indicate dependency problems.

## 2. Update Node within the appropriate supported major

This batch's minimums are **22.23.4 / 24.22.1 / 26.12.1**. [Official advisory](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases). Later, choose the latest supported patch compatible with your application.

- **Official installers:** use the [Node download page](https://nodejs.org/en/download) or the advisory's release links; select the correct major, OS and architecture. Open a new terminal and verify the actual path and version.
- **nvm, fnm, Volta or other version managers:** use the existing manager to install a patched release in the same major, then switch the shell and project pins. Installing a new version while the project selects the old one is insufficient.
- **Homebrew, WinGet, apt and similar managers:** identify the installed package, source and version first, then update that package. Channels may switch majors; do not apply a generic command blindly to production.
- **Node 20 or another end-of-life branch:** plan and test migration to a supported LTS. The [official schedule](https://github.com/nodejs/Release/blob/main/schedule.json) lists Node 20's end of support as 2026-04-30. Its last patch is not evidence of receiving this batch's fixes.

Existing services may still run an older process. Save work, arrange a restart of the relevant services, and verify the newly launched runtime. Do not stop other people's work.

## 3. Update Claude Code and desktop apps

Identify the installation source. Claude Code's official manual update command is:

```sh
claude update
claude --version
```

For Homebrew, WinGet or managed deployments, use that source. [Claude's official installation/update guide](https://code.claude.com/docs/en/setup). The plugin pinning issue listed here is fixed in CLI **2.1.179**; the current supported version can include additional fixes.

Update Claude Desktop through the app/store's supported mechanism. Save work and restart it, then check its app version. The Windows malicious-Git issue is fixed in **1.8089.0**; the macOS Cowork file issue in **1.15962.0**. These are not CLI version numbers. [Windows advisory](https://github.com/anthropics/claude-code/security/advisories/GHSA-4p2j-qj3c-6x84), [macOS advisory](https://github.com/anthropics/claude-code/security/advisories/GHSA-v234-4jrq-mgg6).

Codex, Cursor, VS Code and other apps may bundle runtimes. Use each vendor's supported updater. For an existing npm installation of Codex CLI, update that package and verify:

```sh
npm install -g @openai/codex@latest
codex --version
```

For other Codex installation methods, follow the [official OpenAI CLI documentation](https://learn.chatgpt.com/docs/codex/cli). This is maintenance guidance, **not evidence that all these apps are exploitable through the Node issues**. Do not overwrite app-managed Node binaries. If an updater requires restart, save work and let the user arrange it; recheck bundled runtimes afterward rather than assuming the update contains these fixes.

## 4. MCP packages, credentials and isolation

If you use MCP inside an existing app, update that app/integration. Installing a package in a different project does not patch the app.

If you maintain MCP code:

1. Find the actual resolved and installed dependencies, including adapters and nested copies. Editing only the manifest is insufficient.
2. For the selected SDK 1.x issues here, use **@modelcontextprotocol/sdk 1.32.0+**. The split OAuth client needs **2.2.0+**, and directly imported core also needs **2.2.0+**. The shared-server issue needs **server 2.3.0+**. These are different packages, not one global MCP version.
3. Update the relevant dependencies and lockfile with the project's existing package manager, then build and test. If introduced transitively, update the parent package; global installation does not override it.
4. Follow the [OAuth advisory](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-6qxp-vccf-f47h) for `expectedIssuer`, old credentials' `issuer` and custom provider persistence. This checker never reads or clears credentials. If an affected client may have connected to a malicious server, revoke tokens and rotate relevant secrets.
5. Follow the [isolation advisory](https://github.com/modelcontextprotocol/typescript-sdk/security/advisories/GHSA-jm63-cq47-j5jp): create server/transport instances per request or session, or use a factory producing new servers. Test simultaneous requests from two users for isolation and check post-upgrade connection rejection behavior.

## 5. Self-hosted n8n, Docker, WSL and CI

- Back up n8n's database, configuration and persistent data; confirm restore and compatibility steps. This Data Table issue is fixed in **2.43.5** or **2.44.1** on the respective branches. Check the supported upgrade path before moving to a current release. [Official n8n update guide](https://docs.n8n.io/hosting/installation/updating/).
- Update Node/app image references in Dockerfiles or Compose, rebuild or pull and redeploy. Preserve persistent volumes. Updating the host's Node does not patch a container's Node.
- In your own container, use `docker exec CONTAINER_NAME node --version` and `docker exec CONTAINER_NAME node -p "process.execPath"`, substituting the real container name.
- Inspect WSL, remote hosts and CI runners separately. Update CI Node configuration, images and project pins; verify the version actually used by the job.

## 6. What completion means

After saving work and arranging the relevant restarts, verify actual paths, running versions, installed dependencies and necessary functional/isolation tests. Keep before/after results locally; do not upload raw secrets, private project data or local paths.

No visible anomaly does not prove absence of compromise; a version flag does not prove an intrusion. Investigate suspicious access separately, including credential response. This repository does not automatically determine compromise.
