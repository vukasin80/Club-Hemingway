using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CaffeShop.Repositories;

namespace CaffeShop.Forms
{
    public class ReportsPanel : UserControl
    {
        private readonly ReportsRepository _repo = new();
        private DateTimePicker _dtpFrom, _dtpTo;
        private TabControl _tabs;
        private DataGridView _gridDaily, _gridTop, _gridEmp;
        private Label _lblRevenue, _lblOrders, _lblItems, _lblAvg;

        public ReportsPanel()
        {
            Build();
            RunReport();
        }

        private void Build()
        {
            BackColor = Color.FromArgb(245, 240, 235);

            var title = MainForm.MakeLabel("📊 Izveštaji", 18, FontStyle.Bold, Color.FromArgb(50, 32, 20));
            title.Location = new Point(10, 10);

           
            var lblFrom = MainForm.MakeLabel("Od:", 10, FontStyle.Regular, Color.Black);
            lblFrom.Location = new Point(10, 55);
            _dtpFrom = new DateTimePicker { Location = new Point(40, 50), Width = 140, Format = DateTimePickerFormat.Short };
            _dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var lblTo = MainForm.MakeLabel("Do:", 10, FontStyle.Regular, Color.Black);
            lblTo.Location = new Point(195, 55);
            _dtpTo = new DateTimePicker { Location = new Point(220, 50), Width = 140, Format = DateTimePickerFormat.Short };
            _dtpTo.Value = DateTime.Today;

            var btnRun = MakeBtn("🔍 Prikaži", Color.FromArgb(50, 32, 20), new Point(375, 48));
            btnRun.Click += (s, e) => RunReport();

            var btnPdf = MakeBtn("📄 Export PDF", Color.FromArgb(183, 28, 28), new Point(530, 48));
            btnPdf.Click += (s, e) => ExportPdf();

            var btnExcel = MakeBtn("📊 Export Excel", Color.FromArgb(21, 101, 192), new Point(690, 48));
            btnExcel.Click += (s, e) => ExportExcel();

            
            _lblRevenue = SummaryCard("💰 Ukupan prihod", "0 RSD", Color.FromArgb(46, 125, 50), new Point(10, 95));
            _lblOrders = SummaryCard("📋 Narudžbine", "0", Color.FromArgb(21, 101, 192), new Point(215, 95));
            _lblItems = SummaryCard("☕ Prodato artikala", "0", Color.FromArgb(130, 85, 20), new Point(420, 95));
            _lblAvg = SummaryCard("📈 Prosek/narudžbini", "0 RSD", Color.FromArgb(100, 40, 120), new Point(625, 95));

            
            _tabs = new TabControl
            {
                Location = new Point(10, 185),
                Size = new Size(940, 400),
                Font = new Font("Segoe UI", 10f)
            };

            var tabDaily = new TabPage("📅 Dnevni pregled");
            _gridDaily = MakeGrid();
            _gridDaily.Columns.Add("date", "Datum");
            _gridDaily.Columns.Add("orders", "Narudžbine");
            _gridDaily.Columns.Add("items", "Artikala");
            _gridDaily.Columns.Add("revenue", "Prihod (RSD)");
            _gridDaily.Columns.Add("avg", "Prosek/narudžbini");
            _gridDaily.Columns["revenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridDaily.Columns["avg"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tabDaily.Controls.Add(_gridDaily);

            var tabTop = new TabPage("🏆 Top artikli");
            _gridTop = MakeGrid();
            _gridTop.Columns.Add("rank", "#");
            _gridTop.Columns.Add("name", "Artikal");
            _gridTop.Columns.Add("cat", "Kategorija");
            _gridTop.Columns.Add("qty", "Prodato kom.");
            _gridTop.Columns.Add("rev", "Prihod (RSD)");
            _gridTop.Columns["rank"].Width = 40;
            _gridTop.Columns["rev"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tabTop.Controls.Add(_gridTop);

            var tabEmp = new TabPage("👥 Po konobaru");
            _gridEmp = MakeGrid();
            _gridEmp.Columns.Add("emp", "Konobar");
            _gridEmp.Columns.Add("orders", "Narudžbine");
            _gridEmp.Columns.Add("rev", "Prihod (RSD)");
            _gridEmp.Columns.Add("avg", "Prosek");
            _gridEmp.Columns["rev"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridEmp.Columns["avg"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tabEmp.Controls.Add(_gridEmp);

            _tabs.TabPages.AddRange(new[] { tabDaily, tabTop, tabEmp });

            Controls.AddRange(new Control[]
            {
                title, lblFrom, _dtpFrom, lblTo, _dtpTo,
                btnRun, btnPdf, btnExcel,
                _lblRevenue.Parent, _lblOrders.Parent, _lblItems.Parent, _lblAvg.Parent,
                _tabs
            });
        }

        private void RunReport()
        {
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date;
            if (from > to) { MessageBox.Show("Datum 'Od' mora biti pre datuma 'Do'."); return; }

            var (rev, orders, items) = _repo.GetSummary(from, to);
            decimal avg = orders > 0 ? rev / orders : 0;

            _lblRevenue.Text = $"{rev:N0} RSD";
            _lblOrders.Text = orders.ToString("N0");
            _lblItems.Text = items.ToString("N0");
            _lblAvg.Text = $"{avg:N0} RSD";

           
            _gridDaily.Rows.Clear();
            foreach (var d in _repo.GetMonthly(from.Year, from.Month))
                _gridDaily.Rows.Add(d.Date.ToString("dd.MM.yyyy"),
                    d.OrderCount, d.ItemsSold,
                    $"{d.Revenue:N0}", $"{d.AvgOrderValue:N0}");

            
            _gridTop.Rows.Clear();
            int rank = 1;
            foreach (var t in _repo.GetTopItems(from, to))
                _gridTop.Rows.Add(rank++, t.Name, t.Category, t.TotalQuantity, $"{t.TotalRevenue:N0}");

            
            _gridEmp.Rows.Clear();
            foreach (var e in _repo.GetEmployeeReport(from, to))
            {
                decimal empAvg = e.OrderCount > 0 ? e.Revenue / e.OrderCount : 0;
                _gridEmp.Rows.Add(e.EmployeeName, e.OrderCount, $"{e.Revenue:N0}", $"{empAvg:N0}");
            }
        }

       
        private void ExportPdf()
        {

            using var dlg = new SaveFileDialog
            {
                Filter = "HTML Izveštaj (*.html)|*.html",
                FileName = $"Izvestaj_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.html",
                Title = "Sačuvaj HTML izveštaj (štampaj kao PDF iz browsera)"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var html = BuildHtmlReport();
            File.WriteAllText(dlg.FileName, html, Encoding.UTF8);

            var result = MessageBox.Show(
                "Izveštaj je sačuvan kao HTML.\nOtvoriti u browseru radi štampanja/PDF exporta?",
                "Uspeh", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }

        
        private void ExportExcel()
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Excel CSV (*.csv)|*.csv",
                FileName = $"Izvestaj_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.csv",
                Title = "Sačuvaj Excel izveštaj"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var sb = new StringBuilder();
            sb.AppendLine("sep=;"); 
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date;
            var (rev, orders, items) = _repo.GetSummary(from, to);

            sb.AppendLine($"Caffe Shop Izvestaj;{from:dd.MM.yyyy} - {to:dd.MM.yyyy}");
            sb.AppendLine();
            sb.AppendLine("SUMARNO");
            sb.AppendLine($"Ukupan prihod;{rev:N2} RSD");
            sb.AppendLine($"Broj narudzbi;{orders}");
            sb.AppendLine($"Prodato artikala;{items}");
            sb.AppendLine($"Prosek po narudzbi;{(orders > 0 ? rev / orders : 0):N2} RSD");
            sb.AppendLine();

            sb.AppendLine("DNEVNI PREGLED");
            sb.AppendLine("Datum;Narudzbi;Artikala;Prihod (RSD);Prosek");
            foreach (var d in _repo.GetMonthly(from.Year, from.Month))
                sb.AppendLine($"{d.Date:dd.MM.yyyy};{d.OrderCount};{d.ItemsSold};{d.Revenue:N2};{d.AvgOrderValue:N2}");
            sb.AppendLine();

            sb.AppendLine("TOP ARTIKLI");
            sb.AppendLine("Naziv;Kategorija;Kolicina;Prihod (RSD)");
            foreach (var t in _repo.GetTopItems(from, to))
                sb.AppendLine($"{t.Name};{t.Category};{t.TotalQuantity};{t.TotalRevenue:N2}");
            sb.AppendLine();

            sb.AppendLine("PO KONOBARU");
            sb.AppendLine("Konobar;Narudzbi;Prihod (RSD)");
            foreach (var e in _repo.GetEmployeeReport(from, to))
                sb.AppendLine($"{e.EmployeeName};{e.OrderCount};{e.Revenue:N2}");

            
            File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("Excel (CSV) izveštaj sačuvan!", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildHtmlReport()
        {
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date;
            var (rev, orders, items) = _repo.GetSummary(from, to);
            decimal avg = orders > 0 ? rev / orders : 0;

            var sb = new StringBuilder();
            sb.Append($@"<!DOCTYPE html>
<html lang='sr'><head><meta charset='UTF-8'>
<title>Caffe Shop Izveštaj</title>
<style>
  body {{ font-family: 'Segoe UI', sans-serif; margin: 30px; color: #333; }}
  h1 {{ color: #321408; border-bottom: 3px solid #d2a050; padding-bottom: 8px; }}
  h2 {{ color: #321408; margin-top: 30px; }}
  .summary {{ display: flex; gap: 20px; margin: 20px 0; }}
  .card {{ background: #f5f0eb; border-left: 5px solid #d2a050; padding: 15px 20px; border-radius: 4px; min-width: 180px; }}
  .card .val {{ font-size: 22px; font-weight: bold; color: #321408; }}
  .card .lbl {{ font-size: 12px; color: #666; }}
  table {{ border-collapse: collapse; width: 100%; margin-top: 10px; }}
  th {{ background: #321408; color: white; padding: 10px; text-align: left; }}
  td {{ padding: 8px 10px; border-bottom: 1px solid #e0d8d0; }}
  tr:nth-child(even) {{ background: #faf5f0; }}
  .footer {{ margin-top: 40px; font-size: 11px; color: #999; }}
</style></head><body>
<h1>☕ Caffe Shop — Izveštaj</h1>
<p>Period: <strong>{from:dd.MM.yyyy}</strong> — <strong>{to:dd.MM.yyyy}</strong> &nbsp;|&nbsp; Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}</p>

<div class='summary'>
  <div class='card'><div class='lbl'>Ukupan prihod</div><div class='val'>{rev:N0} RSD</div></div>
  <div class='card'><div class='lbl'>Narudžbine</div><div class='val'>{orders}</div></div>
  <div class='card'><div class='lbl'>Prodato artikala</div><div class='val'>{items}</div></div>
  <div class='card'><div class='lbl'>Prosek/narudžbini</div><div class='val'>{avg:N0} RSD</div></div>
</div>

<h2>📅 Dnevni pregled</h2>
<table><tr><th>Datum</th><th>Narudžbine</th><th>Artikala</th><th>Prihod (RSD)</th><th>Prosek</th></tr>");
            foreach (var d in _repo.GetMonthly(from.Year, from.Month))
                sb.Append($"<tr><td>{d.Date:dd.MM.yyyy}</td><td>{d.OrderCount}</td><td>{d.ItemsSold}</td><td>{d.Revenue:N0}</td><td>{d.AvgOrderValue:N0}</td></tr>");

            sb.Append(@"</table><h2>🏆 Top artikli</h2>
<table><tr><th>#</th><th>Artikal</th><th>Kategorija</th><th>Prodato kom.</th><th>Prihod (RSD)</th></tr>");
            int r = 1;
            foreach (var t in _repo.GetTopItems(from, to))
                sb.Append($"<tr><td>{r++}</td><td>{t.Name}</td><td>{t.Category}</td><td>{t.TotalQuantity}</td><td>{t.TotalRevenue:N0}</td></tr>");

            sb.Append(@"</table><h2>👥 Po konobaru</h2>
<table><tr><th>Konobar</th><th>Narudžbine</th><th>Prihod (RSD)</th><th>Prosek</th></tr>");
            foreach (var e in _repo.GetEmployeeReport(from, to))
            {
                decimal empAvg = e.OrderCount > 0 ? e.Revenue / e.OrderCount : 0;
                sb.Append($"<tr><td>{e.EmployeeName}</td><td>{e.OrderCount}</td><td>{e.Revenue:N0}</td><td>{empAvg:N0}</td></tr>");
            }

            sb.Append($"</table><div class='footer'>Generisao: {System.Environment.UserName} | Caffe Shop Manager</div></body></html>");
            return sb.ToString();
        }

        
        private Label _currentCardLabel;
        private Label SummaryCard(string labelText, string value, Color accent, Point loc)
        {
            var card = new Panel { Location = loc, Size = new Size(200, 75), BackColor = Color.White };
            var stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            var lbl = new Label
            {
                Text = labelText, Font = new Font("Segoe UI", 8f), ForeColor = Color.Gray,
                Location = new Point(12, 8), AutoSize = true
            };
            var val = new Label
            {
                Text = value, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = accent,
                Location = new Point(12, 30), AutoSize = true
            };
            card.Controls.AddRange(new Control[] { stripe, lbl, val });
            _currentCardLabel = val;
            return val;
        }

        private DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            MainForm.StyleGrid(g);
            return g;
        }

        private Button MakeBtn(string text, Color color, Point loc)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = new Size(150, 36),
                FlatStyle = FlatStyle.Flat, BackColor = color,
                ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
