# Recent security issues: the simplest actions for you

Verified **2026-10-23**. Check whether you use a product, then choose its simplest update route. **You do not need to interpret versions: paste [this prompt into your AI](../prompts/QUICK-UPDATE.md), let it investigate, ask your permission and help update.**

## New announcements and updates this week

| Do you use it? | What could happen? | Simplest action |
| --- | --- | --- |
| Chrome | The October 20 stable release contains many security fixes. Browsers process websites and documents daily; old versions can give malicious content opportunities to exploit bugs. This does not mean every issue is actively exploited. | Desktop ⋮ menu → Help → About Google Chrome. On phones, update through the existing app store. Stay on your stable channel; no beta is needed. |
| Firefox | October 20–21 updates block compromised website certificates, helping avoid continued trust in identity documents that became unsafe. | Desktop menu → Help → About Firefox; on Mac use the Firefox menu. Store/managed installations use their existing route. On Android, update through Google Play. |
| AI apps, extensions and local tools | The October 21 Node advisory includes issues that can allow attacker code under specific conditions. Updating an external copy does not necessarily update an app's bundled copy. | Let AI identify the actual app, use its official updater/store and check bundled components. Do not overwrite internal files or install another Node copy just to patch it. |
| Your own website or self-hosted service | PHP's October 21 advisory describes errors that can lead to remote code execution. This concerns the host running PHP, not every person visiting a PHP website. | Confirm usable backups, then update supported PHP patches through the existing hosting panel or ask the host to confirm. Updating WordPress alone does not prove PHP is patched. |

Mozilla already distributed a certificate blocklist automatically; these Firefox releases also embed it for people who did not receive the automatic list. AI issues depend on the actual app and conditions; signing into an AI website alone does not establish local exposure.

Official sources:

- Chrome: [October 20 release](https://chromereleases.googleblog.com/2026/10/stable-channel-update-for-desktop_02018666648.html), [update instructions](https://support.google.com/chrome/answer/95414).
- Firefox: [October 20 desktop advisory](https://www.mozilla.org/en-US/security/advisories/mfsa2026-111/), [October 21 Android advisory](https://www.mozilla.org/en-US/security/advisories/mfsa2026-116/), [update instructions](https://support.mozilla.org/en-US/kb/update-firefox-latest-release).
- AI tools: [October 21 Node advisory](https://nodejs.org/en/blog/vulnerability/october-2026-security-releases), [verified Claude, MCP and n8n advisories](ADVISORIES.md).
- Website hosts: [October 21 PHP advisory](https://github.com/php/php-src/security/advisories/GHSA-w8hw-w9fx-j79w).

## Earlier issues still worth fixing if you have not

| Do you use it? | Why care? | Simplest action |
| --- | --- | --- |
| Windows | The October 13 Windows Shell issue is in the Known Exploited Vulnerabilities catalog. Crafted content delivered by messages, links or attachments can evade some internet-file protections; the attacker still needs to entice user action. | Start → Settings → Windows Update → Check for updates. Install the security updates offered for this computer. |
| iPhone, iPad or Mac | Apple's September 28 advisory describes malicious files leading to code execution and reports of sophisticated targeted exploitation. | iPhone/iPad: Settings → General → Software Update. Mac: System Settings → General → Software Update. Let AI identify the appropriate security update; do not force a major upgrade. |
| Android | The October 5 bulletin includes critical privilege and service-disruption issues. Some require no extra click, but device versions, conditions and protections still matter. | In Settings, find Software Update/System Update; also check Google Play system updates. Menu locations and availability vary, so let AI guide you one screen at a time. |

These have separate dates, are not all new this week and do not mean every device can be remotely taken over. If the OS is out of support, “no updates available” is not a safety conclusion; ask AI to establish an available official patch route.

Official sources:

- Windows: [Microsoft advisory](https://msrc.microsoft.com/update-guide/vulnerability/CVE-2026-96951), [machine-readable bulletin](https://api.msrc.microsoft.com/cvrf/v3.0/cvrf/2026-Oct), [CISA catalog](https://www.cisa.gov/known-exploited-vulnerabilities-catalog), [official update instructions](https://support.microsoft.com/en-us/windows/deployment/updates-lifecycle/install-windows-updates).
- Apple: [iPhone/iPad advisory](https://support.apple.com/en-us/149226), [Mac advisory](https://support.apple.com/en-us/149228), [current releases](https://support.apple.com/en-us/100100), [iPhone/iPad guide](https://support.apple.com/en-us/118575), [Mac guide](https://support.apple.com/en-us/108382).
- Android: [this month's bulletin](https://source.android.com/docs/security/bulletin/2026/2026-10-01), [official update instructions](https://support.google.com/android/answer/7680439).

## If a restart is required

Save forms, documents and ongoing work first; back up where needed. Saved data normally remains, but unsaved content may be lost, running jobs can be interrupted, and the device/app is temporarily unavailable. Timing depends on the device and update. Let AI identify affected work before you choose a restart time.

**Completion means verifying what took effect, not just finishing a download.** Return to the [homepage](../README.md), copy the prompt and let AI handle supported actions, guiding you only where sign-in or confirmation needs your own participation.

If this helped, give the repository a ⭐ and share it. These are selected alerts; verify current vendor information whenever acting later.
