using System;
using System.Drawing;
using System.Windows.Forms;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class MenuPanel : UserControl
    {
        private readonly MenuRepository _repo = new();
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnToggle;

        public MenuPanel() { Build(); Load(); }

        private void Build()
        {
            BackColor = Color.FromArgb(245, 240, 235);

            var title = MainForm.MakeLabel("☕ Upravljanje menijem", 18, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(10, 10);

            _btnAdd = Btn("➕ Dodaj", Color.FromArgb(46, 125, 50), new Point(10, 55));
            _btnEdit = Btn("✏️ Izmeni", Color.FromArgb(21, 101, 192), new Point(175, 55));
            _btnDelete = Btn("🗑️ Obriši", Color.FromArgb(183, 28, 28), new Point(340, 55));
            _btnToggle = Btn("🔄 Dostupnost", Color.FromArgb(130, 85, 20), new Point(505, 55));

            _btnAdd.Click += (s, e) => ShowDialog(null);
            _btnEdit.Click += (s, e) => EditSelected();
            _btnDelete.Click += (s, e) => DeleteSelected();
            _btnToggle.Click += (s, e) => ToggleSelected();

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
            _grid.Columns.Add("name", "Naziv");
            _grid.Columns.Add("cat", "Kategorija");
            _grid.Columns.Add("price", "Cena (RSD)");
            _grid.Columns.Add("avail", "Dostupno");
            _grid.Columns.Add("desc", "Opis");
            _grid.Columns["id"].Visible = false;
            _grid.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            Controls.AddRange(new Control[] { title, _btnAdd, _btnEdit, _btnDelete, _btnToggle, _grid });
        }

        private void Load()
        {
            _grid.Rows.Clear();
            foreach (var m in _repo.GetAll())
                _grid.Rows.Add(m.Id, m.Name, m.Category, $"{m.Price:N0}", m.IsAvailable ? "✅" : "❌", m.Description);
        }

        private void ShowDialog(MenuItem existing)
        {
            using var dlg = new MenuItemDialog(existing);
            if (dlg.ShowDialog() != DialogResult.OK) return;

            if (existing == null)
                _repo.Add(dlg.Item);
            else
            {
                dlg.Item.Id = existing.Id;
                _repo.Update(dlg.Item);
            }
            Load();
        }

        private MenuItem GetSelected()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            var row = _grid.SelectedRows[0];
            return new MenuItem
            {
                Id = Convert.ToInt32(row.Cells["id"].Value),
                Name = row.Cells["name"].Value.ToString(),
                Category = row.Cells["cat"].Value.ToString(),
                Price = decimal.Parse(row.Cells["price"].Value.ToString().Replace(" ", "").Replace(",", "")),
                IsAvailable = row.Cells["avail"].Value.ToString() == "✅",
                Description = row.Cells["desc"].Value.ToString()
            };
        }

        private void EditSelected()
        {
            var item = GetSelected();
            if (item == null) return;
            ShowDialog(item);
        }

        private void DeleteSelected()
        {
            var item = GetSelected();
            if (item == null) return;
            if (!SrpskiDijalog.Pitaj($"Obrisati '{item.Name}'?", "Potvrda", "⚠️")) return;
            _repo.Delete(item.Id);
            Load();
        }

        private void ToggleSelected()
        {
            var item = GetSelected();
            if (item == null) return;
            item.IsAvailable = !item.IsAvailable;
            _repo.Update(item);
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

    
    public class MenuItemDialog : Form
    {
        public MenuItem Item { get; private set; }
        private TextBox _txtName, _txtDesc;
        private ComboBox _cmbCat;
        private NumericUpDown _nudPrice;
        private CheckBox _chkAvail;

        public MenuItemDialog(MenuItem existing = null)
        {
            Text = existing == null ? "Novi artikal" : "Izmeni artikal";
            Size = new Size(380, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            int y = 20;
            Label Lbl(string t) { var l = new Label { Text = t, Location = new Point(20, y + 3), AutoSize = true }; return l; }

            var lName = Lbl("Naziv:"); y += 0;
            _txtName = new TextBox { Location = new Point(130, y), Width = 210 };

            y += 40;
            var lCat = Lbl("Kategorija:"); y += 0;
            _cmbCat = new ComboBox { Location = new Point(130, y), Width = 210, DropDownStyle = ComboBoxStyle.DropDown };
            _cmbCat.Items.AddRange(new[] { "Kafa", "Topli napitci", "Sokovi", "Bezalkoholno", "Alkohol", "Hrana", "Deserti" });

            y += 40;
            var lPrice = Lbl("Cena (RSD):"); y += 0;
            _nudPrice = new NumericUpDown { Location = new Point(130, y), Width = 120, Minimum = 0, Maximum = 99999, DecimalPlaces = 0 };

            y += 40;
            var lDesc = Lbl("Opis:"); y += 0;
            _txtDesc = new TextBox { Location = new Point(130, y), Width = 210 };

            y += 40;
            _chkAvail = new CheckBox { Text = "Dostupno na meniju", Location = new Point(130, y), Checked = true, AutoSize = true };

            if (existing != null)
            {
                _txtName.Text = existing.Name;
                _cmbCat.Text = existing.Category;
                _nudPrice.Value = existing.Price;
                _txtDesc.Text = existing.Description;
                _chkAvail.Checked = existing.IsAvailable;
            }

            y += 50;
            var btnSave = new Button
            {
                Text = "Sačuvaj", Location = new Point(130, y), Width = 110,
                BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Unesite naziv!"); return; }
                Item = new MenuItem
                {
                    Name = _txtName.Text.Trim(),
                    Category = _cmbCat.Text.Trim(),
                    Price = _nudPrice.Value,
                    Description = _txtDesc.Text.Trim(),
                    IsAvailable = _chkAvail.Checked
                };
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                lName, _txtName, lCat, _cmbCat, lPrice, _nudPrice,
                lDesc, _txtDesc, _chkAvail, btnSave
            });
        }
    }
}
