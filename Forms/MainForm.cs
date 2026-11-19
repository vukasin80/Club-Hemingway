using System;
using System.Drawing;
using System.Windows.Forms;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class MainForm : Form
    {
        private readonly OrderRepository _orderRepo = new();
        private Panel _sidebar, _content;
        private Label _lblTime, _lblUser;
        private System.Windows.Forms.Timer _clock;

        public MainForm()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            Text = $"☕ Club Hemingway — {Session.CurrentUser?.FullName}";
            Size = new Size(1150, 720);
            MinimumSize = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 240, 235);
            Font = new Font("Segoe UI", 10f);

            
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 225,
                BackColor = Color.FromArgb(50, 32, 20)
            };

            var logo = new Label
            {
                Text = "☕ Club\nHemingway",
                ForeColor = Color.FromArgb(210, 160, 80),
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(0, 12, 0, 0)
            };

            
            _lblUser = new Label
            {
                Text = $"👤 {Session.CurrentUser?.FullName}\n    {Session.CurrentUser?.Role}",
                ForeColor = Color.FromArgb(160, 140, 110),
                Font = new Font("Segoe UI", 8.5f),
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(18, 6, 0, 0),
                BackColor = Color.FromArgb(38, 24, 14)
            };

            _sidebar.Controls.Add(_lblUser);
            _sidebar.Controls.Add(logo);

            
            bool isAdmin = Session.IsAdmin;

            AddNavBtn("🏠  Dashboard",    LoadDashboard,    1);
            AddNavBtn("📋  Narudžbine",   OpenOrders,       2);
            AddNavBtn("☕  Meni",          OpenMenu,         3);
            AddNavBtn("👥  Zaposleni",     OpenEmployees,    4);

            if (isAdmin)
            {
                AddNavBtn("📊  Izveštaji",    OpenReports,      5);
                AddNavBtn("🔑  Korisnici",    OpenUsers,        6);
            }

            
            var sep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(80, 55, 35)
            };

           
            var btnLogout = new Button
            {
                Text = "🚪  Odjavi se",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 100, 80),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Dock = DockStyle.Bottom,
                Height = 50,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 30, 20);
            btnLogout.Click += (s, e) => Logout();

            _sidebar.Controls.Add(sep);
            _sidebar.Controls.Add(btnLogout);

           
            _content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            
            _clock = new System.Windows.Forms.Timer { Interval = 1000 };
            _clock.Tick += (s, e) => { if (_lblTime != null) _lblTime.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clock.Start();

            Controls.Add(_content);
            Controls.Add(_sidebar);
        }

        private void AddNavBtn(string text, Action action, int insertIdx)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 185, 165),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Dock = DockStyle.Top,
                Height = 50,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 55, 35);
            btn.Click += (s, e) => action();
            _sidebar.Controls.Add(btn);
            _sidebar.Controls.SetChildIndex(btn, insertIdx);
        }

        
        private void LoadDashboard()
        {
            _content.Controls.Clear();

            var (revenue, closedCount) = _orderRepo.GetTodayStats();
            int openOrders = _orderRepo.GetOpen().Count;

            var title = MakeLabel("Dashboard", 22, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(20, 20);

            _lblTime = MakeLabel(DateTime.Now.ToString("HH:mm:ss"), 13, FontStyle.Regular, Color.Gray);
            _lblTime.Location = new Point(20, 55);

            var cardRevenue  = MakeCard("💰 Prihod danas",           $"{revenue:N0} RSD",   Color.FromArgb(46, 125, 50),   new Point(20, 95));
            var cardClosed   = MakeCard("✅ Zatvorene narudžbine",   closedCount.ToString(), Color.FromArgb(21, 101, 192),  new Point(255, 95));
            var cardOpen     = MakeCard("🔓 Otvorene narudžbine",    openOrders.ToString(), Color.FromArgb(183, 28, 28),   new Point(490, 95));

            var todayLabel = MakeLabel("Današnje narudžbine", 13, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            todayLabel.Location = new Point(20, 265);

            var grid = new DataGridView
            {
                Location = new Point(20, 292),
                Size = new Size(860, 330),
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9.5f)
            };
            StyleGrid(grid);
            grid.Columns.Add("table", "Sto");
            grid.Columns.Add("emp", "Konobar");
            grid.Columns.Add("status", "Status");
            grid.Columns.Add("time", "Vreme");
            grid.Columns.Add("total", "Iznos");

            foreach (var o in _orderRepo.GetToday())
                grid.Rows.Add($"Sto #{o.TableNumber}", o.EmployeeName,
                    o.Status == "Open" ? "🔓 Otvorena" : "✅ Zatvorena",
                    o.CreatedAt.ToString("HH:mm"), $"{o.TotalAmount:N0} RSD");

            _content.Controls.AddRange(new Control[]
                { title, _lblTime, cardRevenue, cardClosed, cardOpen, todayLabel, grid });
        }

        private void OpenOrders()    { Show(new OrdersPanel(_orderRepo)); }
        private void OpenMenu()      { Show(new MenuPanel()); }
        private void OpenEmployees() { Show(new EmployeesPanel()); }
        private void OpenReports()   { Show(new ReportsPanel()); }
        private void OpenUsers()
        {
            if (!Session.IsAdmin)
            { MessageBox.Show("Pristup zabranjen.", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Show(new UsersPanel());
        }

        private void Show(UserControl panel)
        {
            _content.Controls.Clear();
            panel.Dock = DockStyle.Fill;
            _content.Controls.Add(panel);
        }

        private void Logout()
        {
            if (!SrpskiDijalog.Pitaj("Da li želite da se odjavite?", "Odjava", "🚪")) return;
            _clock.Stop();
            Session.CurrentUser = null;
            Hide();
            using var login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                Text = $"☕ Caffe Shop Manager — {Session.CurrentUser?.FullName}";
                _lblUser.Text = $"👤 {Session.CurrentUser?.FullName}\n    {Session.CurrentUser?.Role}";
                _clock.Start();
                Show();
                LoadDashboard();
            }
            else
            {
                Application.Exit();
            }
        }

        
        public static Label MakeLabel(string text, float size, FontStyle style, Color color)
            => new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, AutoSize = true };

        public static void StyleGrid(DataGridView grid)
        {
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 32, 20);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 38;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 160, 80);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 245, 240);
            grid.GridColor = Color.FromArgb(230, 225, 220);
            grid.RowTemplate.Height = 34;
        }

        private Panel MakeCard(string title, string value, Color accent, Point loc)
        {
            var card = new Panel { Location = loc, Size = new Size(220, 145), BackColor = Color.White };
            var stripe = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = accent };
            var lbl = MakeLabel(title, 9f, FontStyle.Regular, Color.Gray);
            lbl.Location = new Point(12, 18);
            var val = MakeLabel(value, 21, FontStyle.Bold, accent);
            val.Location = new Point(12, 52);
            card.Controls.AddRange(new Control[] { stripe, lbl, val });
            return card;
        }
    }
}
