using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TRScriptSync
{
    public partial class MainForm : Form
    {
        static readonly Color Panel = Color.FromArgb(32, 32, 36);
        static readonly Color Border = Color.FromArgb(58, 58, 64);
        static readonly Color Muted = Color.FromArgb(150, 150, 158);
        static readonly Color Accent = Color.FromArgb(88, 166, 255);
        static readonly Color Ok = Color.FromArgb(110, 200, 140);
        static readonly Color Err = Color.FromArgb(232, 110, 110);

        readonly List<string> _log = new List<string>();
        List<ReadyFile> _ready = new List<ReadyFile>();
        Point _dragOffset;
        bool _dragging;
        bool _busy;
        int _barValue;
        int _barMax = 1;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            LoadConfigToUi();
            MouseDown += OnDragStart;
            MouseMove += OnDragMove;
            MouseUp += OnDragEnd;
            foreach (Control c in new Control[] { titleLabel, subtitleLabel, folderCaptionLabel, mysqlCaptionLabel, statusLabel, progressPanel })
            {
                c.MouseDown += OnDragStart;
                c.MouseMove += OnDragMove;
                c.MouseUp += OnDragEnd;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Border))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void browseButton_Click(object sender, EventArgs e)
        {
            if (_busy)
                return;
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Script folder";
                dlg.UseDescriptionForTitle = true;
                string cur = (folderBox.Text ?? "").Trim().Trim('"');
                if (Directory.Exists(cur))
                    dlg.SelectedPath = cur;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    folderBox.Text = dlg.SelectedPath;
                }
            }
        }

        void scanButton_Click(object sender, EventArgs e)
        {
            Start(false);
        }

        void importButton_Click(object sender, EventArgs e)
        {
            Start(true);
        }

        void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;
            string p = files[0];
            folderBox.Text = Directory.Exists(p) ? p : Path.GetDirectoryName(p);
        }

        void progressPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Panel);
            int max = _barMax <= 0 ? 1 : _barMax;
            int w = (int)(progressPanel.Width * ((double)_barValue / max));
            if (w > 0)
            {
                using (var b = new SolidBrush(Accent))
                    e.Graphics.FillRectangle(b, 0, 0, Math.Min(w, progressPanel.Width), progressPanel.Height);
            }
        }

        void SetProgress(int value, int max, string file)
        {
            Ui(() =>
            {
                _barMax = max <= 0 ? 1 : max;
                _barValue = value;
                progressPanel.Invalidate();
                if (!string.IsNullOrEmpty(file))
                    AppendLog(file);
            });
        }

        void AppendLog(string line)
        {
            _log.Add(line);
            while (_log.Count > 40)
                _log.RemoveAt(0);
            fileLogBox.Text = string.Join("\r\n", _log.ToArray());
            fileLogBox.SelectionStart = fileLogBox.TextLength;
            fileLogBox.ScrollToCaret();
        }

        void Start(bool import)
        {
            if (_busy)
                return;
            AppConfig cfg = SaveUiToConfig();
            string folder = (cfg.Folder ?? "").Trim().Trim('"');
            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                SetStatus("Select a script folder.", Err);
                return;
            }

            _busy = true;
            scanButton.Enabled = false;
            importButton.Enabled = false;
            browseButton.Enabled = false;
            if (!import)
            {
                _log.Clear();
                fileLogBox.Text = "";
            }
            SetStatus(import ? "Importing..." : "Scanning...", Muted);
            SetProgress(0, 1, "");

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (!import)
                    {
                        List<ReadyFile> ready = ImportJobs.Scan(cfg, line => SetProgress(_barValue, 1, line));
                        Ui(() => _ready = ready);
                        Done(true, ready.Count + " file(s) match a table. Click Import.");
                    }
                    else
                    {
                        List<ReadyFile> ready = _ready;
                        if (ready == null || ready.Count == 0)
                            ready = ImportJobs.Scan(cfg, null);
                        string msg = ImportJobs.ImportReady(cfg, ready, SetProgress);
                        Done(true, msg);
                    }
                }
                catch (Exception ex)
                {
                    Done(false, ex.Message);
                }
            });
        }

        void Done(bool ok, string message)
        {
            Ui(() =>
            {
                _busy = false;
                scanButton.Enabled = true;
                importButton.Enabled = true;
                browseButton.Enabled = true;
                if (ok)
                    _barValue = _barMax;
                progressPanel.Invalidate();
                SetStatus(message, ok ? Ok : Err);
            });
        }

        void LoadConfigToUi()
        {
            AppConfig cfg = AppConfig.Load();
            folderBox.Text = cfg.Folder ?? "";
            hostBox.Text = cfg.Host ?? "127.0.0.1";
            portBox.Text = cfg.Port.ToString();
            userBox.Text = cfg.User ?? "root";
            passBox.Text = cfg.Password ?? "";
            dbBox.Text = cfg.Database ?? "tr_game_db";
            keepExtraCheck.Checked = cfg.KeepExtraColumns;
            matchFileCheck.Checked = cfg.MatchFile;
            rebuildItemsCheck.Checked = cfg.RebuildItemLookups;
        }

        AppConfig SaveUiToConfig()
        {
            var cfg = new AppConfig();
            cfg.Folder = (folderBox.Text ?? "").Trim();
            cfg.Host = (hostBox.Text ?? "").Trim();
            int port;
            cfg.Port = int.TryParse((portBox.Text ?? "").Trim(), out port) ? port : 3306;
            cfg.User = (userBox.Text ?? "").Trim();
            cfg.Password = passBox.Text ?? "";
            cfg.Database = (dbBox.Text ?? "").Trim();
            cfg.KeepExtraColumns = keepExtraCheck.Checked;
            cfg.MatchFile = matchFileCheck.Checked;
            cfg.RebuildItemLookups = rebuildItemsCheck.Checked;
            return cfg;
        }

        void SetStatus(string text, Color color)
        {
            statusLabel.Text = text ?? "";
            statusLabel.ForeColor = color;
        }

        void Ui(Action a)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
                BeginInvoke(a);
            else
                a();
        }

        void OnDragStart(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _dragging = true;
            _dragOffset = e.Location;
            if (sender != this && sender is Control)
            {
                Control c = (Control)sender;
                _dragOffset = new Point(e.X + c.Left, e.Y + c.Top);
            }
        }

        void OnDragMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
                return;
            Point screen = PointToScreen(e.Location);
            if (sender != this && sender is Control)
            {
                Control c = (Control)sender;
                screen = c.PointToScreen(e.Location);
            }
            Location = new Point(screen.X - _dragOffset.X, screen.Y - _dragOffset.Y);
        }

        void OnDragEnd(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }
    }
}
