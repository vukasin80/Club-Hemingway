using System;
using System.Windows.Forms;
using CaffeShop.Database;
using CaffeShop.Forms;

namespace CaffeShop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseManager.Initialize();

            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
                return;

            Application.Run(new MainForm());
        }
    }
}
