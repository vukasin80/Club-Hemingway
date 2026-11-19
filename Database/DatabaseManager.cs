using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CaffeShop.Database
{
    public static class DatabaseManager
    {
        private static string _dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "caffe_shop.db");

        public static string ConnectionString =>
            $"Data Source={_dbPath};Version=3;";

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "caffe_salt_2024"));
            return Convert.ToBase64String(bytes);
        }

        public static void Initialize()
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'Konobar',
                    FullName TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS MenuItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Price REAL NOT NULL,
                    IsAvailable INTEGER NOT NULL DEFAULT 1,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS Employees (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Position TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    HireDate TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TableNumber INTEGER NOT NULL,
                    EmployeeId INTEGER NOT NULL,
                    UserId INTEGER,
                    Status TEXT NOT NULL DEFAULT 'Open',
                    CreatedAt TEXT NOT NULL,
                    ClosedAt TEXT,
                    TotalAmount REAL NOT NULL DEFAULT 0,
                    FOREIGN KEY(EmployeeId) REFERENCES Employees(Id),
                    FOREIGN KEY(UserId) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    FOREIGN KEY(OrderId) REFERENCES Orders(Id),
                    FOREIGN KEY(MenuItemId) REFERENCES MenuItems(Id)
                );
            ";
            cmd.ExecuteNonQuery();

            SeedData(conn);

            
        }

        private static void SeedData(SQLiteConnection conn)
        {
            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM MenuItems";
            long count = (long)checkCmd.ExecuteScalar();
            if (count > 0) return;

            var seed = conn.CreateCommand();
            string adminHash = HashPassword("admin123");
            string konobarHash = HashPassword("konobar123");
           

            seed.CommandText = $@"
                INSERT INTO Users (Username, PasswordHash, Role, FullName, IsActive) VALUES
                ('admin', '{adminHash}', 'Admin', 'Administrator', 1),
                ('konobar1', '{konobarHash}', 'Konobar', 'Marko Petrović', 1),
                

                INSERT INTO MenuItems (Name, Category, Price, IsAvailable, Description) VALUES
                ('Espresso', 'Kafa', 150, 1, 'Klasičan espresso'),
                ('Cappuccino', 'Kafa', 200, 1, 'Espresso sa penom od mleka'),
                ('Latte', 'Kafa', 220, 1, 'Espresso sa mlekom'),
                ('Americano', 'Kafa', 170, 1, 'Espresso sa vodom'),
                ('Čaj', 'Topli napitci', 120, 1, 'Razne vrste čaja'),
                ('Sok od narandže', 'Sokovi', 180, 1, 'Svežo ceđen'),
                ('Coca Cola', 'Sokovi', 150, 1, '0.25l'),
                ('Voda', 'Bezalkoholno', 80, 1, '0.5l'),
                ('Croissant', 'Hrana', 120, 1, 'Masleni kroasan'),
                ('Cheesecake', 'Deserti', 250, 1, 'Domaći čizkejk');

                INSERT INTO Employees (FirstName, LastName, Position, Phone, HireDate, IsActive) VALUES
                ('Marko', 'Petrović', 'Konobar', '063-111-222', '2023-01-15', 1),
                ('Ana', 'Jovanović', 'Barista', '064-333-444', '2022-06-01', 1),
                ('Nikola', 'Đorđević', 'Menadžer', '065-555-666', '2021-03-10', 1);
            ";
            seed.ExecuteNonQuery();
        }
    }
}
