using System;
using System.Drawing;
using System.Windows.Forms;
using CaffeShop.Database;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class UsersPanel : UserControl
    {
        private readonly UserRepository _repo = new();
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDeactivate, _btnPassword;

        public UsersPanel() { Build(); LoadUsers(); }

        private void Build()
        {
            BackColor = Color.FromArgb(245, 240, 235);

            var title = MainForm.MakeLabel("🔑 Upravljanje korisnicima", 18, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(10, 10);

            var note = MainForm.MakeLabel("Samo Admin nalog može upravljati korisnicima.", 9.5f, FontStyle.Italic, Color.Gray);
            note.Location = new Point(10, 45);

            _btnAdd = Btn("➕ Novi korisnik", Color.FromArgb(46, 125, 50), new Point(10, 70));
            _btnEdit = Btn("✏️ Izmeni", Color.FromArgb(21, 101, 192), new Point(175, 70));
            _btnPassword = Btn("🔒 Promena lozinke", Color.FromArgb(130, 85, 20), new Point(340, 70));
            _btnDeactivate = Btn("❌ Deaktiviraj", Color.FromArgb(183, 28, 28), new Point(510, 70));

            _btnAdd.Click += (s, e) => ShowDialog(null);
            _btnEdit.Click += (s, e) => EditSelected();
            _btnPassword.Click += (s, e) => ChangePassword();
            _btnDeactivate.Click += (s, e) => DeactivateSelected();

            _grid = new DataGridView
            {
                Location = new Point(10, 125),
                Size = new Size(940, 460),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            MainForm.StyleGrid(_grid);
            _grid.Columns.Add("id", "ID");
            _grid.Columns.Add("username", "Korisničko ime");
            _grid.Columns.Add("fullname", "Ime i prezime");
            _grid.Columns.Add("role", "Uloga");
            _grid.Columns.Add("status", "Status");
            _grid.Columns["id"].Visible = false;

            Controls.AddRange(new Control[] { title, note, _btnAdd, _btnEdit, _btnPassword, _btnDeactivate, _grid });
        }

        private void LoadUsers()
        {
            _grid.Rows.Clear();
            foreach (var u in _repo.GetAll())
            {
                int idx = _grid.Rows.Add(u.Id, u.Username, u.FullName,
                    u.Role == UserRole.Admin ? "🔑 Admin" : "🧑 Konobar",
                    u.IsActive ? "✅ Aktivan" : "❌ Neaktivan");
                if (!u.IsActive)
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
            }
        }

        private int? GetSelectedId()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return Convert.ToInt32(_grid.SelectedRows[0].Cells["id"].Value);
        }

        private void ShowDialog(User existing)
        {
            using var dlg = new UserDialog(existing);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            if (existing == null) _repo.Add(dlg.User);
            else { dlg.User.Id = existing.Id; _repo.Update(dlg.User); }
            LoadUsers();
        }

        private void EditSelected()
        {
            var id = GetSelectedId();
            if (id == null) return;
            var all = _repo.GetAll();
            var user = all.Find(u => u.Id == id);
            ShowDialog(user);
        }

        private void ChangePassword()
        {
            var id = GetSelectedId();
            if (id == null) return;
            using var dlg = new ChangePasswordDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _repo.ChangePassword(id.Value, dlg.NewPassword);
            MessageBox.Show("Lozinka uspešno promenjena.", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeactivateSelected()
        {
            var id = GetSelectedId();
            if (id == null) return;
            if (id == Session.CurrentUser.Id)
            {
                MessageBox.Show("Ne možete deaktivirati sopstveni nalog!", "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!SrpskiDijalog.Pitaj("Deaktivirati ovog korisnika?", "Potvrda", "⚠️")) return;
            _repo.Delete(id.Value);
            LoadUsers();
        }

        private Button Btn(string text, Color color, Point loc)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = new Size(155, 38),
                FlatStyle = FlatStyle.Flat, BackColor = color,
                ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }

    
    public class UserDialog : Form
    {
        public User User { get; private set; }
        private TextBox _txtUsername, _txtFullName, _txtPassword;
        private ComboBox _cmbRole;
        private Label _lblPassNote;
        private readonly bool _isEdit;

        public UserDialog(User existing = null)
        {
            _isEdit = existing != null;
            Text = _isEdit ? "Izmeni korisnika" : "Novi korisnik";
            Size = new Size(380, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            int y = 20;
            Label Lbl(string t) { var l = new Label { Text = t, Location = new Point(20, y + 3), AutoSize = true }; return l; }

            var lUser = Lbl("Korisničko ime:");
            _txtUsername = new TextBox { Location = new Point(160, y), Width = 180 };
            y += 40;

            var lFull = Lbl("Ime i prezime:");
            _txtFullName = new TextBox { Location = new Point(160, y), Width = 180 };
            y += 40;

            var lRole = Lbl("Uloga:");
            _cmbRole = new ComboBox { Location = new Point(160, y), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRole.Items.AddRange(new[] { "Admin", "Konobar" });
            _cmbRole.SelectedIndex = 1;
            y += 40;

            _lblPassNote = new Label
            {
                Text = _isEdit ? "(ostaviti prazno = ne menja se)" : "Lozinka:",
                Location = new Point(20, y + 3), AutoSize = true,
                ForeColor = _isEdit ? Color.Gray : Color.Black
            };
            _txtPassword = new TextBox { Location = new Point(160, y), Width = 180, UseSystemPasswordChar = true };
            y += 40;

            if (_isEdit)
            {
                _txtUsername.Text = existing.Username;
                _txtFullName.Text = existing.FullName;
                _cmbRole.SelectedItem = existing.Role.ToString();
            }

            y += 10;
            var btnSave = new Button
            {
                Text = "Sačuvaj", Location = new Point(160, y), Width = 110,
                BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtFullName.Text))
                { MessageBox.Show("Popunite obavezna polja."); return; }
                if (!_isEdit && string.IsNullOrWhiteSpace(_txtPassword.Text))
                { MessageBox.Show("Unesite lozinku za novog korisnika."); return; }

                User = new User
                {
                    Username = _txtUsername.Text.Trim(),
                    FullName = _txtFullName.Text.Trim(),
                    Role = _cmbRole.SelectedItem.ToString() == "Admin" ? UserRole.Admin : UserRole.Konobar,
                    IsActive = true,
                    PasswordHash = _isEdit && string.IsNullOrWhiteSpace(_txtPassword.Text)
                        ? null
                        : DatabaseManager.HashPassword(_txtPassword.Text)
                };
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { lUser, _txtUsername, lFull, _txtFullName, lRole, _cmbRole, _lblPassNote, _txtPassword, btnSave });
        }
    }

    
    public class ChangePasswordDialog : Form
    {
        public string NewPassword { get; private set; }
        private TextBox _txtNew, _txtConfirm;

        public ChangePasswordDialog()
        {
            Text = "Promena lozinke";
            Size = new Size(340, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            var l1 = new Label { Text = "Nova lozinka:", Location = new Point(20, 20), AutoSize = true };
            _txtNew = new TextBox { Location = new Point(140, 17), Width = 160, UseSystemPasswordChar = true };
            var l2 = new Label { Text = "Potvrdi:", Location = new Point(20, 60), AutoSize = true };
            _txtConfirm = new TextBox { Location = new Point(140, 57), Width = 160, UseSystemPasswordChar = true };

            var btnOk = new Button
            {
                Text = "Sačuvaj", Location = new Point(140, 130), Width = 110,
                BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += (s, e) =>
            {
                if (_txtNew.Text.Length < 6) { MessageBox.Show("Lozinka mora imati min. 6 karaktera."); return; }
                if (_txtNew.Text != _txtConfirm.Text) { MessageBox.Show("Lozinke se ne podudaraju!"); return; }
                NewPassword = _txtNew.Text;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { l1, _txtNew, l2, _txtConfirm, btnOk });
        }
    }
}
