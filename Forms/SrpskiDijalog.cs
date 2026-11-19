using System;
using System.Drawing;
using System.Windows.Forms;

namespace CaffeShop.Forms
{
  
    public class SrpskiDijalog : Form
    {
        public bool Potvrdjeno { get; private set; } = false;

        public SrpskiDijalog(string poruka, string naslov = "Potvrda", string ikonica = "❓")
        {
            Text = naslov;
            Size = new Size(420, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            
            var lblIcon = new Label
            {
                Text = ikonica,
                Font = new Font("Segoe UI Emoji", 24f),
                Location = new Point(20, 30),
                Size = new Size(50, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblPoruka = new Label
            {
                Text = poruka,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(80, 30),
                Size = new Size(310, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };

        
            var separator = new Panel
            {
                Location = new Point(0, 120),
                Size = new Size(420, 1),
                BackColor = Color.FromArgb(220, 215, 210)
            };

            
            var btnDa = new Button
            {
                Text = "✔  Da",
                Location = new Point(210, 135),
                Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDa.FlatAppearance.BorderSize = 0;
            btnDa.Click += (s, e) => { Potvrdjeno = true; Close(); };

            var btnNe = new Button
            {
                Text = "✘  Ne",
                Location = new Point(310, 135),
                Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(183, 28, 28),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNe.FlatAppearance.BorderSize = 0;
            btnNe.Click += (s, e) => { Potvrdjeno = false; Close(); };

          
            AcceptButton = btnDa;
            CancelButton = btnNe;

            Controls.AddRange(new Control[] { lblIcon, lblPoruka, separator, btnDa, btnNe });
        }

        public static bool Pitaj(string poruka, string naslov = "Potvrda", string ikonica = "❓")
        {
            using var dlg = new SrpskiDijalog(poruka, naslov, ikonica);
            dlg.ShowDialog();
            return dlg.Potvrdjeno;
        }
    }
}
