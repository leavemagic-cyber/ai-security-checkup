using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Reflection;

[assembly: AssemblyTitle("Security Checkup")]
[assembly: AssemblyVersion("0.4.0.0")]
[assembly: AssemblyFileVersion("0.4.0.0")]

namespace SecurityCheckup
{
    public sealed class MainForm : Form
    {
        readonly Button scan = new Button(), update = new Button();
        readonly ListView list = new ListView();
        readonly Label summary = new Label(), hint = new Label();
        ScanReport report;
        bool busy;
        readonly List<string> updateNotes = new List<string>();
        readonly List<string> updateDiagnostics = new List<string>();
        public bool UpdateEnabled { get { return update.Enabled; } }
        public bool RenderOnly;
        protected override bool ShowWithoutActivation { get { return RenderOnly; } }

        public MainForm()
        {
            Text = "安全更新小幫手｜不用 AI，也能先檢查再更新";
            ClientSize = new Size(980, 690); MinimumSize = new Size(880, 620);
            StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Microsoft JhengHei UI", 10F); BackColor = Color.FromArgb(245, 247, 251);

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(22) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            Controls.Add(layout);

            Label title = new Label { Text = "先檢查，再由你決定更新", Dock = DockStyle.Fill, Font = new Font(Font.FontFamily, 23F, FontStyle.Bold), ForeColor = Color.FromArgb(24, 48, 77), TextAlign = ContentAlignment.MiddleLeft };
            layout.Controls.Add(title, 0, 0);
            hint.Text = "不用 AI、帳號或終端機。檢查只讀取常見位置，不會啟動 AI／Python。\r\n本次門檻：2026/10/23。未查全部硬碟、容器、其他使用者或遠端網站。";
            hint.Dock = DockStyle.Fill; hint.ForeColor = Color.FromArgb(65, 78, 94);
            layout.Controls.Add(hint, 0, 1);

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            StyleButton(scan, "1. 開始檢查", true); StyleButton(update, "2. 檢查後才能更新", false); update.Enabled = false;
            Button details = new Button(); StyleButton(details, "查看詳細資料", false);
            actions.Controls.Add(scan); actions.Controls.Add(update); actions.Controls.Add(details);
            layout.Controls.Add(actions, 0, 2);

            list.Dock = DockStyle.Fill; list.View = View.Details; list.FullRowSelect = true; list.MultiSelect = false;
            list.GridLines = true; list.HideSelection = false; list.BackColor = Color.White;
            list.Columns.Add("項目", 180); list.Columns.Add("檢查結果", 180); list.Columns.Add("接下來", 530);
            layout.Controls.Add(list, 0, 3);

            summary.Text = "尚未檢查。按「開始檢查」，不會立即更新。";
            summary.Dock = DockStyle.Fill; summary.Padding = new Padding(0, 12, 0, 0); summary.ForeColor = Color.FromArgb(45, 63, 84);
            layout.Controls.Add(summary, 0, 4);

            FlowLayoutPanel links = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            AddLink(links, "不會用？開啟免下載教學", Engine.Guide);
            AddLink(links, "GitHub 原始碼／回報／按星", Engine.Repo);
            Label privacy = new Label { Text = "不會上傳檢查結果", AutoSize = true, Padding = new Padding(12, 5, 0, 0), ForeColor = Color.DimGray };
            links.Controls.Add(privacy); layout.Controls.Add(links, 0, 5);

            scan.Click += async delegate { await ScanAsync(); };
            update.Click += async delegate { await UpdateAsync(); };
            details.Click += delegate { ShowDetails(); };
            FormClosing += delegate(object sender, FormClosingEventArgs e) {
                if (busy) { e.Cancel = true; MessageBox.Show(this, "正在檢查或更新，請等目前步驟結束。工具不會強制中止安裝程序。", "處理中", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            };
        }

        static void StyleButton(Button b, string text, bool primary)
        {
            b.Text = text; b.AutoSize = false; b.Width = primary ? 170 : 230; b.Height = 44; b.Margin = new Padding(0, 6, 12, 6);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderColor = Color.FromArgb(195, 205, 218);
            b.BackColor = primary ? Color.FromArgb(30, 87, 175) : Color.White;
            b.ForeColor = primary ? Color.White : Color.FromArgb(26, 48, 78);
        }

        static void AddLink(FlowLayoutPanel panel, string text, string url)
        {
            LinkLabel l = new LinkLabel { Text = text, AutoSize = true, Padding = new Padding(0, 5, 20, 0) };
            l.LinkClicked += delegate {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
                catch (Exception e) { MessageBox.Show("無法開啟官方網頁：" + e.Message); }
            };
            panel.Controls.Add(l);
        }

        void SetBusy(bool value)
        {
            busy = value; scan.Enabled = !value;
            update.Enabled = !value && report != null && report.Items.Any(f => f.CanUpdate);
        }

        public void DisplayReport(ScanReport result)
        {
            report = result; list.Items.Clear();
            foreach (Finding f in report.Items)
            {
                ListViewItem row = new ListViewItem(f.Name);
                row.SubItems.Add(f.Status); row.SubItems.Add(f.Action); row.Tag = f;
                row.ForeColor = f.CanUpdate ? Color.FromArgb(150, 56, 0) : f.Status.Contains("污染") ? Color.FromArgb(170, 25, 45) : Color.FromArgb(38, 55, 73);
                list.Items.Add(row);
            }
            foreach (string error in report.Errors) list.Items.Add(new ListViewItem(new[] { "檢查限制", "待確認", error }));
            int count = report.Items.Count(f => f.CanUpdate);
            update.Text = count > 0 ? "2. 更新這 " + count + " 項…" : "沒有可自動更新的項目";
            summary.Text = count > 0 ? "有 " + count + " 項可協助更新；按鈕會先讓你確認。其他待處理項目不會自動變成安全。" : "這次沒有找到可自動更新的項目。請留意清單中的「需要處理／待確認」；不代表整台安全。";
            summary.Text += "\r\n更新後還要正常重啟 App、確認能用。瀏覽器憑證封鎖狀態未驗證。";
            SetBusy(false);
        }

        async Task ScanAsync()
        {
            SetBusy(true); summary.Text = "正在檢查安裝紀錄與程式檔；不會啟動可疑環境…";
            try { DisplayReport(await Task.Run(() => Engine.Scan())); }
            catch (Exception e) { report = null; list.Items.Clear(); summary.Text = "檢查未完成：" + e.Message + "。請開啟免下載教學。"; }
            finally { SetBusy(false); }
        }

        bool ConfirmUpdates(List<Finding> items)
        {
            using (Form dialog = new Form { Text = "要幫你更新這幾項嗎？", StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(620, 390), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Font = Font })
            {
                string names = String.Join("\r\n", items.Select(f => "• " + f.Name + (f.Kind == "node" ? "（維持原主版本）" : "")));
                TextBox explanation = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.None, BackColor = dialog.BackColor, Location = new Point(24, 22), Size = new Size(572, 285), Text =
                    "只更新以下項目：\r\n" + names + "\r\n\r\n使用 Windows 套件管理員從既有 winget 來源處理，不收取本工具費用。需要連網，會向更新來源查詢這些軟體。\r\n\r\n請先儲存工作。更新可能要求關閉 App 或系統權限確認；工具不會強制重啟電腦或關掉你的程式。不會自動接受額外條款、付費或解除安全保護。\r\n\r\n更新後會重新查程式檔；未完成及仍待確認的項目會說明。" };
                Button yes = new Button { Text = "同意並開始更新", DialogResult = DialogResult.OK, Location = new Point(240, 329), Size = new Size(185, 40) };
                Button no = new Button { Text = "先不要", DialogResult = DialogResult.Cancel, Location = new Point(440, 329), Size = new Size(150, 40) };
                dialog.Controls.Add(explanation); dialog.Controls.Add(yes); dialog.Controls.Add(no); dialog.CancelButton = no;
                dialog.ActiveControl = no;
                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        async Task UpdateAsync()
        {
            if (report == null || busy) return;
            List<Finding> items = report.Items.Where(f => f.CanUpdate).ToList();
            if (items.Count == 0 || !ConfirmUpdates(items)) return;
            SetBusy(true); updateNotes.Clear(); updateDiagnostics.Clear();
            try
            {
                foreach (Finding f in items)
                {
                    summary.Text = "正在處理 " + f.Name + "。可能需要下載或 Windows 權限確認，請勿關閉工具。";
                    try
                    {
                        string result = await Task.Run(() => Engine.Execute(new UpdatePlan { Item = f, WingetPath = report.WingetPath, Consent = true }, Engine.Run));
                        updateNotes.Add(f.Name + "：" + result);
                    }
                    catch (Exception e) { updateNotes.Add(f.Name + "：未完成。" + e.Message); }
                    finally { if (!String.IsNullOrEmpty(f.LastUpdateDetail)) updateDiagnostics.Add(f.Name + "\r\n" + f.LastUpdateDetail); }
                }
                DisplayReport(await Task.Run(() => Engine.Scan()));
                summary.Text = "更新程序已結束；請查看結果，未完成不會當作修好。存好工作後正常重啟相關 App，再確認能用。";
                MessageBox.Show(this, String.Join("\r\n\r\n", updateNotes), "本次更新結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e) { summary.Text = "更新後的檢查未完成：" + e.Message + "。尚未判定修補完成。"; }
            finally { SetBusy(false); }
        }

        void ShowDetails()
        {
            string text = "本次資料核對：2026/10/23。詳細資料可能含你的本機路徑，請勿整份公開。\r\n\r\n";
            if (report == null) text += "尚未取得檢查結果。";
            else
            {
                text += String.Join("\r\n\r\n", report.Items.Select(f => f.Name + "｜" + f.Status + "\r\n" + f.Action + "\r\n檔案版本：" + (f.Installed ?? "無法確認") + "\r\n" + f.Path));
                if (report.Errors.Count > 0) text += "\r\n\r\n檢查未完成範圍：\r\n" + String.Join("\r\n", report.Errors);
            }
            if (updateNotes.Count > 0) text = String.Join("\r\n\r\n", updateNotes) + "\r\n\r\n-----\r\n" + text;
            if (updateDiagnostics.Count > 0) text += "\r\n\r\n更新診斷（可能含本機路徑）：\r\n" + String.Join("\r\n\r\n", updateDiagnostics);
            using (Form dialog = new Form { Text = "檢查與更新結果（請勿整份公開）", StartPosition = FormStartPosition.CenterParent, Size = new Size(820, 600), Font = Font })
            {
                dialog.Controls.Add(new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, Text = text });
                dialog.ShowDialog(this);
            }
        }
    }

    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                if (args.Length == 2 && args[0] == "--self-test") return Tests.Run(args[1]);
                if (args.Length == 2 && args[0] == "--scan")
                {
                    File.WriteAllText(args[1], new JavaScriptSerializer().Serialize(Engine.Scan()), new System.Text.UTF8Encoding(false));
                    return 0;
                }
                if (args.Length == 2 && args[0] == "--render")
                {
                    using (MainForm form = new MainForm())
                    {
                        form.RenderOnly = true; form.ShowInTaskbar = false; form.StartPosition = FormStartPosition.Manual;
                        form.Location = new Point(-30000, -30000);
                        form.DisplayReport(Engine.Scan()); form.Show(); Application.DoEvents();
                        using (Bitmap image = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(image, new Rectangle(0, 0, form.Width, form.Height)); image.Save(args[1], System.Drawing.Imaging.ImageFormat.Png); }
                        form.Close();
                    }
                    return 0;
                }
                if (args.Length != 0) return 2;
                Application.Run(new MainForm()); return 0;
            }
            catch (Exception e)
            {
                if (args.Length == 2) { File.WriteAllText(args[1] + ".error.txt", e.ToString()); return 2; }
                MessageBox.Show("工具未完成，沒有判定修補成功。\r\n" + e.Message + "\r\n請改用 Repo 的免下載教學。", "未完成", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 2;
            }
        }
    }
}
