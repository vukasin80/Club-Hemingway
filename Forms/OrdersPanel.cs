using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using CaffeShop.Models;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class OrdersPanel : UserControl
    {
        private readonly OrderRepository _orderRepo;
        private readonly EmployeeRepository _empRepo = new();
        private readonly MenuRepository _menuRepo = new();

        private DataGridView _gridOrders, _gridItems;
        private Button _btnNew, _btnAddItem, _btnRemoveItem, _btnClose, _btnPrint;
        private Label _lblTotal;
        private Order _selectedOrder;

       
        private List<OrderItem> _printItems;
        private Order _printOrder;

        public OrdersPanel(OrderRepository repo)
        {
            _orderRepo = repo;
            Build();
            LoadOrders();
        }

        private void Build()
        {
            BackColor = Color.FromArgb(245, 240, 235);
            Padding = new Padding(10);

            var title = MainForm.MakeLabel("📋 Narudžbine", 18, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(10, 10);

            _btnNew        = MakeBtn("➕ Nova narudžbina", Color.FromArgb(46, 125, 50),   new Point(10, 50));
            _btnClose      = MakeBtn("✅ Naplati",          Color.FromArgb(130, 85, 20),   new Point(185, 50));
            _btnPrint      = MakeBtn("🖨️ Štampaj račun",   Color.FromArgb(60, 60, 120),    new Point(360, 50));
            _btnAddItem    = MakeBtn("+ Dodaj stavku",     Color.FromArgb(21, 101, 192),  new Point(635, 50));
            _btnRemoveItem = MakeBtn("− Ukloni stavku",    Color.FromArgb(183, 28, 28),   new Point(810, 50));

            _btnNew.Click        += NewOrder;
            _btnClose.Click      += CloseOrder;
            _btnPrint.Click      += PrintReceipt;
            _btnAddItem.Click    += AddItem;
            _btnRemoveItem.Click += RemoveItem;

            _btnClose.Enabled      = false;
            _btnPrint.Enabled      = false;
            _btnAddItem.Enabled    = false;
            _btnRemoveItem.Enabled = false;

           
            _gridOrders = new DataGridView
            {
                Location = new Point(10, 100),
                Size = new Size(380, 450),
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false
            };
            MainForm.StyleGrid(_gridOrders);
            _gridOrders.Columns.Add("id",    "ID");
            _gridOrders.Columns.Add("table", "Sto");
            _gridOrders.Columns.Add("emp",   "Konobar");
            _gridOrders.Columns.Add("time",  "Vreme");
            _gridOrders.Columns.Add("total", "Ukupno");
            _gridOrders.Columns["id"].Visible = false;
            _gridOrders.SelectionChanged += OrderSelected;

            var lbl = MainForm.MakeLabel("Stavke narudžbine:", 12, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            lbl.Location = new Point(410, 100);

           
            _gridItems = new DataGridView
            {
                Location = new Point(410, 125),
                Size = new Size(565, 380),
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false
            };
            MainForm.StyleGrid(_gridItems);
            _gridItems.Columns.Add("id",    "ID");
            _gridItems.Columns.Add("item",  "Artikal");
            _gridItems.Columns.Add("qty",   "Kol.");
            _gridItems.Columns.Add("price", "Cena");
            _gridItems.Columns.Add("sub",   "Ukupno");
            _gridItems.Columns["id"].Visible = false;
            _gridItems.Columns["qty"].DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            _gridItems.Columns["price"].DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleRight;
            _gridItems.Columns["sub"].DefaultCellStyle.Alignment    = DataGridViewContentAlignment.MiddleRight;

            _lblTotal = MainForm.MakeLabel("Ukupno: 0 RSD", 14, FontStyle.Bold, Color.FromArgb(46, 125, 50));
            _lblTotal.Location = new Point(410, 515);

            Controls.AddRange(new Control[]
            {
                title, _btnNew, _btnClose, _btnPrint, _btnAddItem, _btnRemoveItem,
                _gridOrders, lbl, _gridItems, _lblTotal
            });
        }

       
        private void LoadOrders()
        {
            _gridOrders.Rows.Clear();
            foreach (var o in _orderRepo.GetOpen())
                _gridOrders.Rows.Add(o.Id, $"Sto #{o.TableNumber}", o.EmployeeName,
                    o.CreatedAt.ToString("HH:mm"), $"{o.TotalAmount:N0} RSD");
        }

        private void LoadItems(int orderId)
        {
            _gridItems.Rows.Clear();
            decimal total = 0;
            foreach (var item in _orderRepo.GetItems(orderId))
            {
                _gridItems.Rows.Add(item.Id, item.MenuItemName, item.Quantity,
                    $"{item.UnitPrice:N0}", $"{item.Subtotal:N0} RSD");
                total += item.Subtotal;
            }
            _lblTotal.Text = $"Ukupno: {total:N0} RSD";
        }

        private void OrderSelected(object sender, EventArgs e)
        {
            if (_gridOrders.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(_gridOrders.SelectedRows[0].Cells["id"].Value);
            _selectedOrder = _orderRepo.GetOpen().FirstOrDefault(o => o.Id == id);
            if (_selectedOrder == null) return;

            LoadItems(id);
            bool hasItems = _orderRepo.GetItems(id).Count > 0;
            _btnAddItem.Enabled    = true;
            _btnRemoveItem.Enabled = true;
            _btnClose.Enabled      = true;
            _btnPrint.Enabled      = true;
        }

       
        private void NewOrder(object sender, EventArgs e)
        {
            using var dlg = new NewOrderDialog(_empRepo.GetActive());
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _orderRepo.CreateOrder(dlg.TableNumber, dlg.EmployeeId);
            LoadOrders();
        }

        private void AddItem(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;
            using var dlg = new AddItemDialog(_menuRepo.GetAvailable());
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _orderRepo.AddItem(new OrderItem
            {
                OrderId    = _selectedOrder.Id,
                MenuItemId = dlg.SelectedItem.Id,
                Quantity   = dlg.Quantity,
                UnitPrice  = dlg.SelectedItem.Price
            });
            LoadItems(_selectedOrder.Id);
            LoadOrders();
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            if (_selectedOrder == null || _gridItems.SelectedRows.Count == 0) return;
            int itemId = Convert.ToInt32(_gridItems.SelectedRows[0].Cells["id"].Value);
            if (!SrpskiDijalog.Pitaj("Ukloniti stavku?", "Potvrda", "🗑️")) return;
            _orderRepo.RemoveItem(itemId, _selectedOrder.Id);
            LoadItems(_selectedOrder.Id);
            LoadOrders();
        }

        private void CloseOrder(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;
            var items = _orderRepo.GetItems(_selectedOrder.Id);
            if (items.Count == 0)
            {
                MessageBox.Show("Narudžbina je prazna!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = items.Sum(x => x.Subtotal);
            if (!SrpskiDijalog.Pitaj(
                    $"Naplatiti sto #{_selectedOrder.TableNumber}?\nUkupno: {total:N0} RSD",
                    "Naplata", "💰")) return;

            
            bool doPrint = SrpskiDijalog.Pitaj("Odštampati račun?", "Štampanje", "🖨️");

            if (doPrint)
                DoPrint(_selectedOrder, items);

            _orderRepo.CloseOrder(_selectedOrder.Id);

            ResetSelection();
            LoadOrders();
            MessageBox.Show("Narudžbina naplaćena! ✅", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       
        private void PrintReceipt(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;
            var items = _orderRepo.GetItems(_selectedOrder.Id);
            if (items.Count == 0)
            {
                MessageBox.Show("Nema stavki za štampanje.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DoPrint(_selectedOrder, items);
        }

        private void DoPrint(Order order, List<OrderItem> items)
        {
            _printOrder = order;
            _printItems = items;

            var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 900); // ~80mm roll
            pd.PrintPage += PrintPage;

            using var preview = new PrintPreviewDialog
            {
                Document = pd,
                Text = $"Pregled računa — Sto #{order.TableNumber}",
                Width = 500,
                Height = 700
            };
            preview.ShowDialog();
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            var g       = e.Graphics;
            var order   = _printOrder;
            var items   = _printItems;
            float x     = 10f;
            float y     = 10f;
            float width = 295f;

            
            var fTitle  = new Font("Courier New", 13f, FontStyle.Bold);
            var fBold   = new Font("Courier New", 9f, FontStyle.Bold);
            var fNormal = new Font("Courier New", 9f);
            var fSmall  = new Font("Courier New", 7.5f);
            var black   = Brushes.Black;
            var sfRight = new StringFormat { Alignment = StringAlignment.Far };
            var sfCenter= new StringFormat { Alignment = StringAlignment.Center };

           
            g.DrawString("☕  CLUB HEMINGWAY", fTitle, black, new RectangleF(x, y, width, 22), sfCenter);
            y += 24;
            g.DrawString("Bata Bulic 25, Petrovac na Mlavi", fSmall, black, new RectangleF(x, y, width, 16), sfCenter);
            y += 16;
            g.DrawString("Tel: 011/123-456", fSmall, black, new RectangleF(x, y, width, 16), sfCenter);
            y += 20;

            DrawLine(g, x, y, width); y += 6;

            g.DrawString($"Račun br: #{order.Id}", fNormal, black, x, y); y += 14;
            g.DrawString($"Sto:      #{order.TableNumber}", fNormal, black, x, y); y += 14;
            g.DrawString($"Konobar:  {order.EmployeeName}", fNormal, black, x, y); y += 14;
            g.DrawString($"Datum:    {DateTime.Now:dd.MM.yyyy  HH:mm}", fNormal, black, x, y); y += 14;

            DrawLine(g, x, y, width); y += 6;

            
            g.DrawString("Artikal", fBold, black, x, y);
            g.DrawString("Ukupno", fBold, black, new RectangleF(x, y, width, 14), sfRight);
            y += 16;
            DrawLine(g, x, y, width); y += 4;

            
            foreach (var item in items)
            {
                string line1 = $"{item.MenuItemName}";
                string line2 = $"  {item.Quantity} x {item.UnitPrice:N0} RSD";
                string subtotal = $"{item.Subtotal:N0} RSD";

                g.DrawString(line1, fBold, black, x, y); y += 13;
                g.DrawString(line2, fNormal, black, x, y);
                g.DrawString(subtotal, fNormal, black, new RectangleF(x, y, width, 13), sfRight);
                y += 16;
            }

            DrawLine(g, x, y, width); y += 6;

           
            decimal total = items.Sum(i => i.Subtotal);
            g.DrawString("UKUPNO ZA PLAĆANJE:", fBold, black, x, y);
            g.DrawString($"{total:N0} RSD", fBold, black, new RectangleF(x, y, width, 16), sfRight);
            y += 22;

            DrawLine(g, x, y, width); y += 10;

            
            g.DrawString("Hvala na poseti!", fBold, black, new RectangleF(x, y, width, 16), sfCenter);
            y += 16;
            g.DrawString("Dobrodošli ponovo!", fSmall, black, new RectangleF(x, y, width, 14), sfCenter);
            y += 14;
            g.DrawString($"Fiskalni br: {Guid.NewGuid().ToString()[..8].ToUpper()}", fSmall, black,
                new RectangleF(x, y, width, 14), sfCenter);

            e.HasMorePages = false;

            fTitle.Dispose(); fBold.Dispose(); fNormal.Dispose(); fSmall.Dispose();
        }

        private static void DrawLine(Graphics g, float x, float y, float width)
        {
            g.DrawLine(Pens.Black, x, y, x + width, y);
        }

        private void ResetSelection()
        {
            _selectedOrder = null;
            _gridItems.Rows.Clear();
            _lblTotal.Text         = "Ukupno: 0 RSD";
            _btnClose.Enabled      = false;
            _btnPrint.Enabled      = false;
            _btnAddItem.Enabled    = false;
            _btnRemoveItem.Enabled = false;
        }

        private Button MakeBtn(string text, Color color, Point loc)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = new Size(165, 38),
                FlatStyle = FlatStyle.Flat, BackColor = color,
                ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }

    
    public class NewOrderDialog : Form
    {
        public int TableNumber { get; private set; }
        public int EmployeeId { get; private set; }

        private NumericUpDown _nudTable;
        private ComboBox _cmbEmployee;

        public NewOrderDialog(List<Employee> employees)
        {
            Text = "Nova narudžbina";
            Size = new Size(320, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            var lblTable = new Label { Text = "Broj stola:", Location = new Point(20, 22), AutoSize = true };
            _nudTable = new NumericUpDown { Location = new Point(140, 19), Width = 120, Minimum = 1, Maximum = 50, Value = 1 };

            var lblEmp = new Label { Text = "Konobar:", Location = new Point(20, 62), AutoSize = true };
            _cmbEmployee = new ComboBox { Location = new Point(140, 59), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var emp in employees) _cmbEmployee.Items.Add(emp);
            if (_cmbEmployee.Items.Count > 0) _cmbEmployee.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "Kreiraj", Location = new Point(140, 150), Width = 100,
                BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += (s, e) =>
            {
                if (_cmbEmployee.SelectedItem == null) { MessageBox.Show("Izaberite konobara!"); return; }
                TableNumber = (int)_nudTable.Value;
                EmployeeId  = ((Employee)_cmbEmployee.SelectedItem).Id;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { lblTable, _nudTable, lblEmp, _cmbEmployee, btnOk });
        }
    }

    
    public class AddItemDialog : Form
    {
        public MenuItem SelectedItem { get; private set; }
        public int Quantity { get; private set; }

        private ComboBox _cmbItem;
        private NumericUpDown _nudQty;

        public AddItemDialog(List<MenuItem> items)
        {
            Text = "Dodaj stavku";
            Size = new Size(340, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            var lblItem = new Label { Text = "Artikal:", Location = new Point(20, 22), AutoSize = true };
            _cmbItem = new ComboBox { Location = new Point(110, 19), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var i in items) _cmbItem.Items.Add(i);
            if (_cmbItem.Items.Count > 0) _cmbItem.SelectedIndex = 0;

            var lblQty = new Label { Text = "Količina:", Location = new Point(20, 62), AutoSize = true };
            _nudQty = new NumericUpDown { Location = new Point(110, 59), Width = 80, Minimum = 1, Maximum = 50, Value = 1 };

            var btnOk = new Button
            {
                Text = "Dodaj", Location = new Point(150, 150), Width = 100,
                BackColor = Color.FromArgb(21, 101, 192), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += (s, e) =>
            {
                if (_cmbItem.SelectedItem == null) return;
                SelectedItem = (MenuItem)_cmbItem.SelectedItem;
                Quantity     = (int)_nudQty.Value;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { lblItem, _cmbItem, lblQty, _nudQty, btnOk });
        }
    }
}
