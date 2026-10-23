# Security update guide — no AI required

[Home](../README.md) · [完整繁體中文](NO-AI.zh-TW.md)

**No AI account, repository download, terminal or version-number interpretation required. Choose what you use:**

- [Chrome](#chrome)
- [Firefox](#firefox)
- [An installed AI app or a service connecting multiple AI providers](#ai-apps-and-services)
- [Your own WordPress website, blog or store](#your-own-website)
- [Not sure](#not-sure-what-you-use)

This is a guide, not a device scanner or an automatic repair tool. Do only the applicable steps. Save documents, forms and other work before restarting.

## Chrome

In your usual desktop Chrome:

1. Open **⋮ → Help → About Google Chrome**.
2. Let it check and download updates. Save work, then select **Relaunch** if offered.
3. Reopen the same screen and check that it reports up to date, with no update error or restart still pending.

[Google's official instructions](https://support.google.com/chrome/answer/95414) also provide Android/iPhone/iPad options. Linux, managed and unsupported devices need their applicable official route; do not remove management or security protections.

This completes a browser update step. The eMudhra response also involves a separately delivered certificate blocklist. This guide does not verify your blocklist or other applications' protection. Do not bypass certificate warnings or enter passwords after a connection warning.

## Firefox

In your usual desktop Firefox:

1. Windows: **☰ → Help → About Firefox**. Mac: **Firefox → About Firefox** in the top menu.
2. Let it check and download updates. Save work, then select **Restart to Update Firefox**, if offered.
3. Reopen About Firefox and check there is no pending installation or restart. For errors, follow [Mozilla's official instructions](https://support.mozilla.org/en-US/kb/update-firefox-latest-release).

Microsoft Store installations update through the Store; distribution-provided Linux installations use their existing updater. Do not install a separate copy and assume the original was updated.

Mozilla says standard Firefox configurations received the eMudhra block automatically; some custom configurations need the related releases. This guide does not inspect that state. Do not bypass certificate warnings. [Official incident report](https://blog.mozilla.org/security/2026/10/20/protecting-firefox-users-following-the-compromise-of-emudhra-intermediate-certificates/).

## AI apps and services

Chatting on the ChatGPT, Gemini or Claude website alone does not establish that you installed an affected local AI component.

For an installed app, extension or multi-provider service, check its official announcement or contact support. If you suspect the malicious LiteLLM release, do not start the potentially affected environment to check its version or update it; contact the vendor through your browser first. Not every AI app uses LiteLLM.

Copy this to the app's official support or service operator. Replace only the app/service name:

```text
My app/service: replace with its name

I am not technical. Please confirm whether your product is affected by these October 21, 2026 incidents:
1. Node.js security update: check the actual application and its bundled runtime, not just another system Node installation.
2. Malicious LiteLLM release: inspect installation/release records without starting a potentially compromised environment to check a version.

If affected, provide the official remedy, the buttons I need to press, any shutdown/restart, and how to verify the fix is active.
If compromised LiteLLM artifacts executed, please also address isolation, clean rebuilding and potentially exposed credentials; an upgrade alone is insufficient.
Please identify anything you cannot confirm.

Official sources:
https://nodejs.org/en/blog/vulnerability/october-2026-security-releases
https://docs.litellm.ai/blog/security-incident-oct-21-2026
```

Once the vendor confirms an update is the appropriate remedy, use the app's existing official updater or original store, then reopen and check normal operation. A support acknowledgment or an “up to date” label alone does not prove these incidents are resolved. Keep unresolved cases marked pending.

## Your own website

Ask your host or website maintainer about PHP2Shell if you own a WordPress website, blog, store or self-hosted service. Ordinary visitors do not need to install PHP for this incident.

Copy the following into your host's private support channel and replace the website address. Never include your admin password or private website data in public comments.

```text
My website: replace with your website address

Please confirm that the PHP actually serving my website has the fixes for the two critical vulnerabilities announced by PHP on October 21, 2026 (PHP2Shell).
Updating WordPress/plugins alone is not sufficient.
If an update is needed, confirm backups and compatibility, explain any downtime or added cost, and wait for my approval before proceeding.
Afterward verify the website, login and main functions, and confirm the security fixes are active. If using backported patches, please explain the corresponding fix.

PHP's official announcement:
https://www.php.net/archive/2026.php#2026-10-21-5
```

Sending a support request is not completion. Wait for confirmation that the fix is active and check that the website works.

## Not sure what you use

Start by updating your usual browser. If you only browse websites, do not install an AI app and do not own a website, you do not need to install Node.js, LiteLLM or PHP just for this guide.

For an app with unknown bundled components, send the support text to its official vendor. You do not need to buy or sign up for an AI service.

Record each item as **updated and working / not applicable (confirmed unused) / pending or incomplete**. An unchecked item is not a safety finding.

See the [four-incident explainer](PERSONAL-ALERTS.md) for sources. Checked **October 23, 2026**; consult current official information when acting later. This is not intrusion detection or an all-device security guarantee.

[Report where you got stuck](https://github.com/leavemagic-cyber/ai-security-checkup/issues) with device, app and step; never post passwords, keys, codes or private data. Star/share the [repository](https://github.com/leavemagic-cyber/ai-security-checkup) if useful. No star required.
