using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecurityCheckup
{
    static class Tests
    {
        public static int Run(string output)
        {
            List<string> passed = new List<string>();
            Action<bool, string> check = (ok, name) => { if (!ok) throw new Exception(name); passed.Add(name); };
            Action<Action, string> rejects = (action, name) => { bool failed = false; try { action(); } catch (InvalidOperationException) { failed = true; } check(failed, name); };
            foreach (string[] pair in new[] { new[] {"22.23.3", "22.23.4"}, new[] {"24.22.0", "24.22.1"}, new[] {"26.12.0", "26.12.1"} })
            {
                check(Engine.VersionFinding("n", "node", null, pair[0], pair[1]).Status == "需要處理", "affected " + pair[0]);
                check(Engine.VersionFinding("n", "node", null, pair[1], pair[1]).Status == "達本次檔案門檻", "boundary " + pair[1]);
            }
            check(Engine.NodeFix("25.99.0") == null && Engine.NodeFix("24.22.1-rc.1") == null, "unsupported and prerelease are not passed");
            check(Engine.VersionFinding("n", "node", null, null, null).Status == "待確認", "missing version is unresolved");
            Finding f = new Finding { Name = "Node.js", Kind = "node", Installed = "24.21.0", Minimum = "24.22.1", Scope = "machine", PackageIds = new[] {"OpenJS.NodeJS.LTS"}, CanUpdate = true };
            string args = Engine.BuildUpgradeArguments(f, "OpenJS.NodeJS.LTS");
            check(args.Contains("--version 24.22.1") && args.Contains("--scope machine"), "same branch and existing scope pinned");
            check(Engine.MeetsTarget(f, "24.22.1") && !Engine.MeetsTarget(f, "26.12.1"), "post-update verification rejects an unexpected Node major");
            check(!new[] {"--all", "--force", "--accept-", "--allow-reboot", "--ignore-", "--include-", "install "}.Any(s => args.Contains(s)), "no bulk upgrade, forced install, accepted terms, reboot or bypass");
            f.Minimum = "26.12.1"; rejects(() => Engine.BuildUpgradeArguments(f, "OpenJS.NodeJS.LTS"), "cross-major update rejected");
            f.Minimum = "24.20.0"; rejects(() => Engine.BuildUpgradeArguments(f, "OpenJS.NodeJS.LTS"), "downgrade rejected");
            f.Minimum = "24.22.1"; rejects(() => Engine.BuildUpgradeArguments(f, "Untrusted.Package"), "unlisted package rejected");
            f.CanUpdate = false; rejects(() => Engine.BuildUpgradeArguments(f, "OpenJS.NodeJS.LTS"), "noneligible finding cannot update");
            int calls = 0;
            Func<string, string, int, ProcessResult> fake = (exe, command, timeout) => { calls++; return new ProcessResult { ExitCode = 0, Output = "synthetic" }; };
            rejects(() => Engine.Execute(new UpdatePlan { Item = f, WingetPath = "fake", Consent = false }, fake), "no consent means zero process launches");
            check(calls == 0, "declining update launches nothing");
            f.CanUpdate = true; f.Path = System.Reflection.Assembly.GetExecutingAssembly().Location; f.Fingerprint = "stale";
            rejects(() => Engine.Execute(new UpdatePlan { Item = f, WingetPath = "fake", Consent = true }, fake), "changed installation invalidates consent");
            check(calls == 0, "stale plan launches nothing");
            f.Fingerprint = Engine.Fingerprint(f.Path);
            string verification = Engine.Execute(new UpdatePlan { Item = f, WingetPath = "fake", Consent = true }, fake);
            check(calls == 2 && verification.Contains("實際檔案未達"), "installer exit zero is not sufficient verification");
            calls = 0;
            string absent = Engine.Execute(new UpdatePlan { Item = f, WingetPath = "fake", Consent = true }, (exe, command, timeout) => { calls++; return new ProcessResult { ExitCode = unchecked((int)0x8A150014) }; });
            check(calls == 1 && absent.Contains("沒有安裝另一份"), "unmatched installation does not install new software");
            calls = 0;
            rejects(() => Engine.Execute(new UpdatePlan { Item = f, WingetPath = "fake", Consent = true }, (exe, command, timeout) => { calls++; return new ProcessResult { TimedOut = true }; }), "lookup timeout does not update");
            check(calls == 1, "timeout launched only the lookup");
            ProcessResult actualProcess = Engine.Run(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"), "/d /c exit 7", 10000);
            check(actualProcess.ExitCode == 7, "real child-process failure is propagated");
            string fixture = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(output)), "static-metadata-fixture");
            string metadata = System.IO.Path.Combine(fixture, "litellm-1.107.0.dist-info");
            Directory.CreateDirectory(metadata);
            File.WriteAllText(System.IO.Path.Combine(metadata, "METADATA"), "Name: litellm\nVersion: 1.107.0\n");
            File.WriteAllText(System.IO.Path.Combine(fixture, "cook.pth"), "raise RuntimeError('must never execute')\n");
            ScanReport staticReport = new ScanReport();
            Engine.ScanPythonSites(staticReport, fixture, 0);
            check(staticReport.Items.Count == 1 && staticReport.Items[0].Status == "發現受污染版本紀錄" && !staticReport.Items[0].CanUpdate, "malicious-release metadata is inspected without executing or offering upgrade-as-cleanup");
            File.WriteAllText(System.IO.Path.Combine(metadata, "METADATA"), "Name: unrelated\nVersion: 1.107.0\n");
            staticReport = new ScanReport(); Engine.ScanPythonSites(staticReport, fixture, 0);
            check(staticReport.Items.Single().Status == "待確認", "similar filename alone is not a malicious-package finding");
            using (MainForm form = new MainForm()) check(!form.UpdateEnabled, "GUI cannot update before a scan");
            File.WriteAllLines(output, passed.ToArray(), new System.Text.UTF8Encoding(false));
            return 0;
        }
    }
}
