using System;
using System.Drawing;
using System.Windows.Forms;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class EmployeesPanel : UserControl
    {
        private readonly EmployeeRepository _repo = new();
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDeactivate;
        private CheckBox _chkShowAll;

        public EmployeesPanel() { Build(); Load(); }

        private void Build()
        {
            BackColor = Color.FromArgb(245, 240, 235);

            var title = MainForm.MakeLabel("👥 Upravljanje zaposlenima", 18, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(10, 10);

            _btnAdd = Btn("➕ Dodaj", Color.FromArgb(46, 125, 50), new Point(10, 55));
            _btnEdit = Btn("✏️ Izmeni", Color.FromArgb(21, 101, 192), new Point(175, 55));
            _btnDeactivate = Btn("❌ Deaktiviraj", Color.FromArgb(183, 28, 28), new Point(340, 55));

            _chkShowAll = new CheckBox
            {
                Text = "Prikaži sve (uključujući neaktivne)",
                Location = new Point(520, 63),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f)
            };
            _chkShowAll.CheckedChanged += (s, e) => Load();

            _btnAdd.Click += (s, e) => ShowDialog(null);
            _btnEdit.Click += (s, e) => EditSelected();
            _btnDeactivate.Click += (s, e) => DeactivateSelected();

            _grid = new DataGridView
            {
                Location = new Point(10, 110),
                Size = new Size(940, 480),
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
            _grid.Columns.Add("name", "Ime i prezime");
            _grid.Columns.Add("pos", "Pozicija");
            _grid.Columns.Add("phone", "Telefon");
            _grid.Columns.Add("email", "Email");
            _grid.Columns.Add("hire", "Datum zaposlenja");
            _grid.Columns.Add("active", "Status");
            _grid.Columns["id"].Visible = false;

            Controls.AddRange(new Control[] { title, _btnAdd, _btnEdit, _btnDeactivate, _chkShowAll, _grid });
        }

        private void Load()
        {
            _grid.Rows.Clear();
            var list = _chkShowAll.Checked ? _repo.GetAll() : _repo.GetActive();
            foreach (var e in list)
            {
                int idx = _grid.Rows.Add(e.Id, e.FullName, e.Position, e.Phone,
                    e.Email, e.HireDate.ToString("dd.MM.yyyy"), e.IsActive ? "✅ Aktivan" : "❌ Neaktivan");
                if (!e.IsActive)
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
            }
        }

        private void ShowDialog(Employee existing)
        {
            using var dlg = new EmployeeDialog(existing);
            if (dlg.ShowDialog() != DialogResult.OK) return;

            if (existing == null)
                _repo.Add(dlg.Employee);
            else
            {
                dlg.Employee.Id = existing.Id;
                _repo.Update(dlg.Employee);
            }
            Load();
        }

        private Employee GetSelected()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            var row = _grid.SelectedRows[0];
            return new Employee
            {
                Id = Convert.ToInt32(row.Cells["id"].Value),
                FirstName = row.Cells["name"].Value.ToString().Split(' ')[0],
                LastName = row.Cells["name"].Value.ToString().Split(' ')[1],
                Position = row.Cells["pos"].Value.ToString(),
                Phone = row.Cells["phone"].Value.ToString(),
                Email = row.Cells["email"].Value.ToString(),
                HireDate = DateTime.ParseExact(row.Cells["hire"].Value.ToString(), "dd.MM.yyyy", null),
                IsActive = row.Cells["active"].Value.ToString().StartsWith("✅")
            };
        }

        private void EditSelected()
        {
            var emp = GetSelected();
            if (emp == null) return;
            ShowDialog(emp);
        }

        private void DeactivateSelected()
        {
            var emp = GetSelected();
            if (emp == null) return;
            if (!SrpskiDijalog.Pitaj($"Deaktivirati '{emp.FullName}'?", "Potvrda", "⚠️")) return;
            _repo.Delete(emp.Id);
            Load();
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

    
    public class EmployeeDialog : Form
    {
        public Employee Employee { get; private set; }
        private TextBox _txtFirst, _txtLast, _txtPhone, _txtEmail;
        private ComboBox _cmbPos;
        private DateTimePicker _dtp;

        public EmployeeDialog(Employee existing = null)
        {
            Text = existing == null ? "Novi zaposleni" : "Izmeni zaposlenog";
            Size = new Size(380, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            int y = 20;
            Label Lbl(string t) { var l = new Label { Text = t, Location = new Point(20, y + 3), AutoSize = true }; return l; }

            var lF = Lbl("Ime:");
            _txtFirst = new TextBox { Location = new Point(140, y), Width = 200 };
            y += 40;

            var lL = Lbl("Prezime:");
            _txtLast = new TextBox { Location = new Point(140, y), Width = 200 };
            y += 40;

            var lP = Lbl("Pozicija:");
            _cmbPos = new ComboBox { Location = new Point(140, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDown };
            _cmbPos.Items.AddRange(new[] { "Konobar", "Barista", "Menadžer", "Kuvar", "Čistač" });
            y += 40;

            var lPh = Lbl("Telefon:");
            _txtPhone = new TextBox { Location = new Point(140, y), Width = 200 };
            y += 40;

            var lE = Lbl("Email:");
            _txtEmail = new TextBox { Location = new Point(140, y), Width = 200 };
            y += 40;

            var lD = Lbl("Datum zap.:");
            _dtp = new DateTimePicker { Location = new Point(140, y), Width = 200, Format = DateTimePickerFormat.Short };
            y += 40;

            if (existing != null)
            {
                _txtFirst.Text = existing.FirstName;
                _txtLast.Text = existing.LastName;
                _cmbPos.Text = existing.Position;
                _txtPhone.Text = existing.Phone;
                _txtEmail.Text = existing.Email;
                _dtp.Value = existing.HireDate;
            }

            y += 10;
            var btnSave = new Button
            {
                Text = "Sačuvaj", Location = new Point(140, y), Width = 110,
                BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtFirst.Text) || string.IsNullOrWhiteSpace(_txtLast.Text))
                { MessageBox.Show("Unesite ime i prezime!"); return; }
                Employee = new Employee
                {
                    FirstName = _txtFirst.Text.Trim(),
                    LastName = _txtLast.Text.Trim(),
                    Position = _cmbPos.Text.Trim(),
                    Phone = _txtPhone.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    HireDate = _dtp.Value,
                    IsActive = true
                };
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { lF, _txtFirst, lL, _txtLast, lP, _cmbPos, lPh, _txtPhone, lE, _txtEmail, lD, _dtp, btnSave });
        }
    }
}
