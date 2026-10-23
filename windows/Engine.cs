using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SecurityCheckup
{
    public sealed class Finding
    {
        public string Name, Status, Action, Path, Installed, Minimum, Scope, Kind, Fingerprint, LastUpdateDetail;
        public string[] PackageIds = new string[0];
        public bool CanUpdate;
    }

    public sealed class ScanReport
    {
        public string CheckedAt = "2026-10-23";
        public string Scope = "常見安裝位置與目前使用者；不是完整盤點或入侵檢測。";
        public List<Finding> Items = new List<Finding>();
        public List<string> Errors = new List<string>();
        public string WingetPath;
    }

    public sealed class InstalledApp
    {
        public string Name, Version, Location, Icon, Scope;
        public bool Msi;
    }

    public sealed class ProcessResult
    {
        public int ExitCode;
        public string Output;
        public bool TimedOut;
    }

    public sealed class UpdatePlan
    {
        public Finding Item;
        public string WingetPath;
        public bool Consent;
    }

    public static class Engine
    {
        public const string Guide = "https://github.com/leavemagic-cyber/ai-security-checkup/blob/main/docs/NO-AI.zh-TW.md";
        public const string Repo = "https://github.com/leavemagic-cyber/ai-security-checkup";
        static readonly Dictionary<int, string> NodeMinimum = new Dictionary<int, string> {
            {22, "22.23.4"}, {24, "24.22.1"}, {26, "26.12.1"}
        };
        static readonly HashSet<string> AllowedPackages = new HashSet<string>(StringComparer.Ordinal) {
            "Google.Chrome.EXE", "Google.Chrome", "Mozilla.Firefox", "OpenJS.NodeJS.LTS", "OpenJS.NodeJS"
        };

        public static Version ParseVersion(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            Match m = Regex.Match(value.Trim(), @"^v?(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?$");
            if (!m.Success) return null;
            Version v;
            string normalized = m.Groups[1].Value + "." + m.Groups[2].Value + "." + m.Groups[3].Value + "." + (m.Groups[4].Success ? m.Groups[4].Value : "0");
            return Version.TryParse(normalized, out v) ? v : null;
        }

        public static string NodeFix(string installed)
        {
            Version v = ParseVersion(installed);
            return v != null && NodeMinimum.ContainsKey(v.Major) ? NodeMinimum[v.Major] : null;
        }

        public static string ReadVersion(string path)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            Version v = ParseVersion(info.ProductVersion);
            if (v == null) v = ParseVersion(info.FileVersion);
            return v == null ? null : v.ToString();
        }

        public static string Fingerprint(string path)
        {
            FileInfo info = new FileInfo(path);
            return info.Length + "|" + info.LastWriteTimeUtc.Ticks + "|" + ReadVersion(path);
        }

        public static Finding VersionFinding(string name, string kind, string path, string installed, string minimum)
        {
            Finding f = new Finding { Name = name, Kind = kind, Path = path, Installed = installed, Minimum = minimum };
            Version actual = ParseVersion(installed), baseline = ParseVersion(minimum);
            if (actual == null || baseline == null)
            {
                f.Status = "待確認"; f.Action = "無法判讀或不在本次版本範圍；請由官方確認。";
            }
            else if (actual < baseline)
            {
                f.Status = "需要處理"; f.Action = "程式檔低於本次修補門檻。";
            }
            else
            {
                f.Status = "達本次檔案門檻"; f.Action = "不是整台安全證明；請確認 App 已重啟且正常。";
            }
            return f;
        }

        static IEnumerable<InstalledApp> ReadApps(ScanReport report)
        {
            List<InstalledApp> apps = new List<InstalledApp>();
            foreach (RegistryHive hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                    using (RegistryKey uninstall = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                    {
                        if (uninstall == null) continue;
                        foreach (string child in uninstall.GetSubKeyNames())
                        using (RegistryKey k = uninstall.OpenSubKey(child))
                        {
                            if (k == null) continue;
                            string name = Convert.ToString(k.GetValue("DisplayName"));
                            if (name != "Google Chrome" && name != "Node.js" && !name.StartsWith("Mozilla Firefox", StringComparison.Ordinal)) continue;
                            apps.Add(new InstalledApp { Name = name, Version = Convert.ToString(k.GetValue("DisplayVersion")),
                                Location = Convert.ToString(k.GetValue("InstallLocation")), Icon = Convert.ToString(k.GetValue("DisplayIcon")),
                                Msi = Convert.ToString(k.GetValue("WindowsInstaller")) == "1",
                                Scope = hive == RegistryHive.LocalMachine ? "machine" : "user" });
                        }
                    }
                }
                catch (Exception e) { report.Errors.Add("部分安裝紀錄無法讀取：" + e.GetType().Name); }
            }
            return apps;
        }

        static string Env(string name) { return Environment.GetEnvironmentVariable(name) ?? ""; }
        static string Combine(string root, string tail) { return String.IsNullOrEmpty(root) ? null : System.IO.Path.Combine(root, tail); }

        static string IconPath(string icon)
        {
            if (String.IsNullOrWhiteSpace(icon)) return null;
            string s = Environment.ExpandEnvironmentVariables(icon.Trim());
            if (s.StartsWith("\"")) { int end = s.IndexOf('"', 1); return end > 1 ? s.Substring(1, end - 1) : null; }
            return Regex.Replace(s, @",-?\d+$", "");
        }

        static bool SamePath(string a, string b)
        {
            return !String.IsNullOrEmpty(a) && !String.IsNullOrEmpty(b) && String.Equals(System.IO.Path.GetFullPath(a), System.IO.Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
        }

        static void AddFile(ScanReport report, HashSet<string> seen, string name, string kind, string path, InstalledApp app)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path) || !seen.Add(System.IO.Path.GetFullPath(path))) return;
            try
            {
                string installed = ReadVersion(path);
                string minimum = kind == "node" ? NodeFix(installed) : kind == "chrome" ? "156.0.8078.48" : "158.0.1";
                Finding f = VersionFinding(name, kind, path, installed, minimum);
                f.Fingerprint = Fingerprint(path);
                if (app != null && ParseVersion(app.Version) != null && ParseVersion(app.Version) != ParseVersion(installed))
                {
                    f.Status = "待重啟或確認"; f.Action = "安裝紀錄與實際程式檔不同；先存好工作，依 App 正常方式重啟。";
                }
                else if (app != null && f.Status == "需要處理")
                {
                    f.Scope = app.Scope;
                    if (kind == "chrome") f.PackageIds = app.Msi ? new[] { "Google.Chrome", "Google.Chrome.EXE" } : new[] { "Google.Chrome.EXE", "Google.Chrome" };
                    if (kind == "firefox" && app.Name.IndexOf("ESR", StringComparison.OrdinalIgnoreCase) < 0) f.PackageIds = new[] { "Mozilla.Firefox" };
                    if (kind == "node" && app.Msi && (SamePath(path, Combine(Env("ProgramFiles"), @"nodejs\node.exe")) || SamePath(path, Combine(Env("ProgramFiles(x86)"), @"nodejs\node.exe"))))
                        f.PackageIds = new[] { "OpenJS.NodeJS.LTS", "OpenJS.NodeJS" };
                    f.CanUpdate = f.PackageIds.Length > 0 && report.WingetPath != null;
                    f.Action = f.CanUpdate ? (kind == "node" ? "可在你同意後，更新到相同主版本的本次修補版。" : "可在你同意後，透過 Windows 套件管理員更新。") : "請透過原 App／商店更新；此安裝方式不自動處理。";
                }
                if (kind == "node" && app == null && f.Status == "需要處理") f.Action = "這份 Node 可能由 App 或其他工具管理；要更新原 App，不會直接覆寫。";
                if (kind == "chrome" || kind == "firefox") f.Action += " 憑證封鎖狀態未驗證。";
                report.Items.Add(f);
            }
            catch (Exception e) { report.Items.Add(new Finding { Name = name, Status = "待確認", Action = "無法讀取程式檔：" + e.GetType().Name, Path = path, Kind = kind }); }
        }

        internal static void ScanPythonSites(ScanReport report, string root, int depth)
        {
            if (String.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
            try
            {
                foreach (string dir in Directory.GetDirectories(root, "litellm-*.dist-info", SearchOption.TopDirectoryOnly))
                {
                    string metadata = System.IO.Path.Combine(dir, "METADATA");
                    if (!File.Exists(metadata)) { report.Errors.Add("有 LiteLLM 安裝目錄但缺少版本紀錄；待確認。"); continue; }
                    string[] headers = File.ReadLines(metadata).Take(100).ToArray();
                    string version = headers.Where(s => s.StartsWith("Version: ", StringComparison.Ordinal)).Select(s => s.Substring(9).Trim()).FirstOrDefault();
                    bool named = headers.Any(s => s.Equals("Name: litellm", StringComparison.OrdinalIgnoreCase));
                    bool identified = named && !String.IsNullOrEmpty(version);
                    bool malicious = identified && version == "1.107.0";
                    report.Items.Add(new Finding { Name = "LiteLLM 安裝紀錄", Kind = "litellm", Installed = version, Path = metadata,
                        Status = !identified ? "待確認" : malicious ? "發現受污染版本紀錄" : "僅此紀錄未命中",
                        Action = !identified ? "安裝紀錄不完整，無法確認套件身分與版本。" : malicious ? "不要啟動環境。需隔離、乾淨重建及確認憑證暴露；不會用升級冒充清除。" : "未檢測歷史安裝、是否執行或其他環境；不是沒有中毒的證明。" });
                }
                if (depth == 0) return;
                foreach (string child in Directory.GetDirectories(root, "Python*", SearchOption.TopDirectoryOnly))
                {
                    ScanPythonSites(report, System.IO.Path.Combine(child, @"Lib\site-packages"), 0);
                    ScanPythonSites(report, System.IO.Path.Combine(child, "site-packages"), 0);
                }
            }
            catch (Exception e) { report.Errors.Add("部分 Python 安裝紀錄無法讀取：" + e.GetType().Name); }
        }

        public static ScanReport Scan()
        {
            ScanReport report = new ScanReport();
            string alias = Combine(Env("LOCALAPPDATA"), @"Microsoft\WindowsApps\winget.exe");
            report.WingetPath = File.Exists(alias) ? alias : null;
            List<InstalledApp> apps = ReadApps(report).ToList();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (InstalledApp app in apps)
            {
                string kind = app.Name == "Google Chrome" ? "chrome" : app.Name == "Node.js" ? "node" : "firefox";
                string exe = kind == "chrome" ? "chrome.exe" : kind == "node" ? "node.exe" : "firefox.exe";
                List<string> candidates = new List<string> { IconPath(app.Icon), Combine(app.Location, exe) };
                if (kind == "chrome") candidates.Add(Combine(app.Location, @"Application\chrome.exe"));
                string[] fallbackRoots = app.Scope == "machine" ? new[] { Env("ProgramFiles"), Env("ProgramFiles(x86)") } : new[] { Env("LOCALAPPDATA") };
                foreach (string baseDir in fallbackRoots)
                    candidates.Add(Combine(baseDir, kind == "chrome" ? @"Google\Chrome\Application\chrome.exe" : kind == "node" ? @"nodejs\node.exe" : @"Mozilla Firefox\firefox.exe"));
                string found = candidates.FirstOrDefault(p => !String.IsNullOrEmpty(p) && File.Exists(p) && String.Equals(System.IO.Path.GetFileName(p), exe, StringComparison.OrdinalIgnoreCase));
                if (kind == "firefox" && app.Name.IndexOf("ESR", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.Items.Add(new Finding { Name = app.Name, Kind = "firefox-esr", Status = "待原通道確認", Action = "ESR 不轉成一般版；請用原更新方式處理。", Path = found });
                }
                else if (found != null) AddFile(report, seen, app.Name, kind, found, app);
                else report.Items.Add(new Finding { Name = app.Name, Kind = kind, Status = "待確認", Action = "有安裝紀錄，但未找到實際程式檔。" });
            }
            foreach (string baseDir in new[] { Env("ProgramFiles"), Env("ProgramFiles(x86)"), Env("LOCALAPPDATA") })
            {
                AddFile(report, seen, "Chrome（額外副本）", "chrome", Combine(baseDir, @"Google\Chrome\Application\chrome.exe"), null);
                AddFile(report, seen, "Firefox（額外副本）", "firefox", Combine(baseDir, @"Mozilla Firefox\firefox.exe"), null);
                AddFile(report, seen, "Node（額外副本）", "node", Combine(baseDir, @"nodejs\node.exe"), null);
            }
            foreach (string dir in (Env("PATH")).Split(';'))
                if (!String.IsNullOrWhiteSpace(dir)) AddFile(report, seen, "Node（其他路徑）", "node", Combine(dir.Trim().Trim('"'), "node.exe"), null);
            AddFile(report, seen, "AI App 內附 Node", "node", Combine(Env("USERPROFILE"), @".cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe"), null);
            string bundled = Combine(Env("LOCALAPPDATA"), @"OpenAI\Codex\runtimes\cua_node");
            if (Directory.Exists(bundled))
                try { foreach (string dir in Directory.GetDirectories(bundled)) AddFile(report, seen, "AI App 內附 Node", "node", Combine(dir, @"bin\node.exe"), null); }
                catch (Exception e) { report.Errors.Add("部分 App 元件無法讀取：" + e.GetType().Name); }
            int before = report.Items.Count;
            ScanPythonSites(report, Combine(Env("APPDATA"), "Python"), 1);
            ScanPythonSites(report, Combine(Env("LOCALAPPDATA"), @"Programs\Python"), 1);
            ScanPythonSites(report, Env("ProgramFiles"), 1);
            if (report.Items.Count == before) report.Items.Add(new Finding { Name = "LiteLLM", Kind = "litellm", Status = "常見位置未找到", Action = "未查 App 私有環境、容器或歷史安裝；不代表沒安裝／沒中毒。" });
            report.Items.Add(new Finding { Name = "自己的網站／PHP", Kind = "php", Status = "遠端網站未檢查", Action = "有自己的網站才需詢問主機商；本工具不會更新遠端主機。" });
            if (report.WingetPath == null) report.Errors.Add("這台電腦未找到 Windows 套件管理員；不會自動安裝新工具，請用官方更新入口。");
            return report;
        }

        public static string BuildUpgradeArguments(Finding f, string packageId)
        {
            if (!f.CanUpdate || !AllowedPackages.Contains(packageId) || !f.PackageIds.Contains(packageId) || (f.Scope != "user" && f.Scope != "machine")) throw new InvalidOperationException("不在允許的更新清單。");
            string args = "upgrade --id " + packageId + " --exact --source winget --scope " + f.Scope + " --silent --disable-interactivity";
            if (f.Kind == "node")
            {
                Version installed = ParseVersion(f.Installed), target = ParseVersion(f.Minimum);
                if (installed == null || target == null || installed.Major != target.Major || target <= installed || NodeFix(f.Installed) != f.Minimum) throw new InvalidOperationException("Node 更新必須留在原主版本，且不可降版。");
                args += " --version " + f.Minimum;
            }
            else if (f.Kind != "chrome" && f.Kind != "firefox") throw new InvalidOperationException("這項不支援自動更新。");
            return args;
        }

        public static bool MeetsTarget(Finding f, string version)
        {
            Version actual = ParseVersion(version), minimum = ParseVersion(f.Minimum);
            return actual != null && minimum != null && actual >= minimum && (f.Kind != "node" || actual.Major == minimum.Major);
        }

        public static ProcessResult Run(string exe, string args, int queryTimeout)
        {
            StringBuilder output = new StringBuilder();
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(exe, args) { UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
                DataReceivedEventHandler collect = delegate(object sender, DataReceivedEventArgs e) {
                    if (e.Data != null) lock (output) { if (output.Length < 65536) output.AppendLine(e.Data); }
                };
                process.OutputDataReceived += collect; process.ErrorDataReceived += collect;
                process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
                if (queryTimeout > 0 && !process.WaitForExit(queryTimeout))
                {
                    // Only a read-only package lookup has a deadline; installers are never killed here.
                    process.Kill(); process.WaitForExit();
                    return new ProcessResult { ExitCode = -1, TimedOut = true, Output = output.ToString() };
                }
                process.WaitForExit();
                return new ProcessResult { ExitCode = process.ExitCode, Output = output.ToString() };
            }
        }

        public static string Execute(UpdatePlan plan, Func<string, string, int, ProcessResult> run)
        {
            if (plan == null || !plan.Consent) throw new InvalidOperationException("尚未取得本次更新同意。");
            Finding f = plan.Item;
            if (plan.WingetPath == null || f == null || !f.CanUpdate) throw new InvalidOperationException("沒有可執行的更新計畫。");
            if (!File.Exists(f.Path) || Fingerprint(f.Path) != f.Fingerprint) throw new InvalidOperationException("檢查後程式已變更，請重新檢查再同意。");
            foreach (string id in f.PackageIds)
            {
                string arguments = BuildUpgradeArguments(f, id);
                ProcessResult lookup = run(plan.WingetPath, "list --id " + id + " --exact --source winget --scope " + f.Scope + " --disable-interactivity", 120000);
                f.LastUpdateDetail = "查詢代碼：" + unchecked((uint)lookup.ExitCode).ToString("X8") + "\r\n" + lookup.Output;
                if (lookup.TimedOut) throw new InvalidOperationException("官方更新來源查詢逾時；未開始更新。");
                if (unchecked((uint)lookup.ExitCode) == 0x8A150014) continue;
                if (lookup.ExitCode != 0) throw new InvalidOperationException("無法確認原安裝對應的更新來源。請使用官方入口。");
                if (!File.Exists(f.Path) || Fingerprint(f.Path) != f.Fingerprint) throw new InvalidOperationException("查詢期間程式已變更，未開始更新；請重新檢查。");
                ProcessResult result = run(plan.WingetPath, arguments, 0);
                f.LastUpdateDetail += "\r\n更新代碼：" + unchecked((uint)result.ExitCode).ToString("X8") + "\r\n" + result.Output;
                string actual = File.Exists(f.Path) ? ReadVersion(f.Path) : null;
                if (result.ExitCode == 0 && MeetsTarget(f, actual))
                    return "程式檔已達本次門檻；請存好工作並正常重啟 App，再確認能用。憑證封鎖與整台裝置安全未驗證。";
                if (result.ExitCode == 0) return "安裝程序完成，但實際檔案未達本次門檻。可能仍待重啟；尚未判定修補完成。";
                return "更新未完成。可能需要原更新通道、權限、條款或特定版本尚未提供；沒有略過這些要求。可開啟詳細資料供協助者確認。";
            }
            return "找不到與原安裝方式相符的套件，沒有安裝另一份程式。請使用 App 官方更新。";
        }
    }
}
