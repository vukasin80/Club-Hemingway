using System;
using System.Drawing;
using System.Windows.Forms;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class LoginForm : Form
    {
        private readonly UserRepository _repo = new();
        private TextBox _txtUser, _txtPass;
        private Label _lblError;
        private Button _btnLogin;
        private int _failCount = 0;

        public LoginForm()
        {
            Build();
        }

        private void Build()
        {
            Text = "☕ Club Hemingway — Prijava";
            Size = new Size(420, 500);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.FromArgb(50, 32, 20)
            };

            var logo = new Label
            {
                Text = "☕",
                Font = new Font("Segoe UI Emoji", 36f),
                ForeColor = Color.FromArgb(210, 160, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 15),
                Size = new Size(420, 60)
            };
            var appName = new Label
            {
                Text = "Club Hemingway",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(210, 160, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 85),
                Size = new Size(420, 35)
            };
            header.Controls.AddRange(new Control[] { logo, appName });

            
            var lblTitle = new Label
            {
                Text = "Prijavite se",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 32, 20),
                Location = new Point(50, 170),
                AutoSize = true
            };

            var lblUser = MiniLabel("Korisničko ime", new Point(50, 210));
            _txtUser = MakeTextBox(new Point(50, 230), false);

            var lblPass = MiniLabel("Lozinka", new Point(50, 280));
            _txtPass = MakeTextBox(new Point(50, 300), true);

            _lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(183, 28, 28),
                Location = new Point(50, 350),
                Size = new Size(310, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _btnLogin = new Button
            {
                Text = "Prijavi se",
                Location = new Point(50, 378),
                Size = new Size(310, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 32, 20),
                ForeColor = Color.FromArgb(210, 160, 80),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += TryLogin;

            var hint = new Label
            {
                Text = "Admin: admin / admin123  |  Konobar: konobar1 / konobar123",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.LightGray,
                Location = new Point(50, 435),
                Size = new Size(310, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            AcceptButton = _btnLogin;

            Controls.AddRange(new Control[]
            {
                header, lblTitle, lblUser, _txtUser,
                lblPass, _txtPass, _lblError, _btnLogin, hint
            });
        }

        private void TryLogin(object sender, EventArgs e)
        {
            _lblError.Text = "";
            if (string.IsNullOrWhiteSpace(_txtUser.Text) || string.IsNullOrWhiteSpace(_txtPass.Text))
            {
                _lblError.Text = "⚠ Unesite korisničko ime i lozinku.";
                return;
            }

            var user = _repo.Login(_txtUser.Text.Trim(), _txtPass.Text);
            if (user == null)
            {
                _failCount++;
                _lblError.Text = $"⚠ Pogrešno korisničko ime ili lozinka. ({_failCount}/5)";
                if (_failCount >= 5)
                {
                    MessageBox.Show("Previše neuspešnih pokušaja. Aplikacija će se zatvoriti.",
                        "Blokiran pristup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                _txtPass.Clear();
                _txtPass.Focus();
                return;
            }

            Session.CurrentUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }

        private Label MiniLabel(string text, Point loc) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = loc,
            AutoSize = true
        };

        private TextBox MakeTextBox(Point loc, bool password)
        {
            var txt = new TextBox
            {
                Location = loc,
                Size = new Size(310, 36),
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = password
            };
            return txt;
        }
    }
}
