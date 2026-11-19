using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CaffeShop.Database;

namespace CaffeShop.Repositories
{
    public class DailyReport
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int ItemsSold { get; set; }
        public decimal AvgOrderValue => OrderCount > 0 ? Revenue / OrderCount : 0;
    }

    public class TopItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class EmployeeReport
    {
        public string EmployeeName { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportsRepository
    {
        
        public List<DailyReport> GetMonthly(int year, int month)
        {
            var list = new List<DailyReport>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
    SELECT
        DATE(o.ClosedAt) AS Day,
        COUNT(*) AS Orders,
        SUM(o.TotalAmount) AS Revenue,
        COALESCE((
            SELECT SUM(oi.Quantity)
            FROM OrderItems oi
            JOIN Orders o2 ON oi.OrderId = o2.Id
            WHERE DATE(o2.ClosedAt) = DATE(o.ClosedAt) AND o2.Status = 'Closed'
        ), 0) AS ItemsSold
    FROM Orders o
    WHERE o.Status = 'Closed'
      AND strftime('%Y', o.ClosedAt) = @year
      AND strftime('%m', o.ClosedAt) = @month
    GROUP BY DATE(o.ClosedAt)
    ORDER BY DATE(o.ClosedAt)";
            cmd.Parameters.AddWithValue("@year", year.ToString("D4"));
            cmd.Parameters.AddWithValue("@month", month.ToString("D2"));
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new DailyReport
                {
                    Date = DateTime.Parse(r.GetString(0)),
                    OrderCount = r.GetInt32(1),
                    Revenue = (decimal)r.GetDouble(2),
                    ItemsSold = r.GetInt32(3)
                });
            }
            return list;
        }

        
        public List<TopItem> GetTopItems(DateTime from, DateTime to, int limit = 10)
        {
            var list = new List<TopItem>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.Name, m.Category, SUM(oi.Quantity), SUM(oi.Quantity * oi.UnitPrice)
                FROM OrderItems oi
                JOIN Orders o ON oi.OrderId = o.Id
                JOIN MenuItems m ON oi.MenuItemId = m.Id
                WHERE o.Status = 'Closed'
                  AND DATE(o.ClosedAt) BETWEEN @from AND @to
                GROUP BY m.Id
                ORDER BY SUM(oi.Quantity) DESC
                LIMIT @lim";
            cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@lim", limit);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new TopItem
                {
                    Name = r.GetString(0),
                    Category = r.GetString(1),
                    TotalQuantity = r.GetInt32(2),
                    TotalRevenue = (decimal)r.GetDouble(3)
                });
            return list;
        }

      
        public List<EmployeeReport> GetEmployeeReport(DateTime from, DateTime to)
        {
            var list = new List<EmployeeReport>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.FirstName || ' ' || e.LastName, COUNT(o.Id), SUM(o.TotalAmount)
                FROM Orders o
                JOIN Employees e ON o.EmployeeId = e.Id
                WHERE o.Status = 'Closed'
                  AND DATE(o.ClosedAt) BETWEEN @from AND @to
                GROUP BY o.EmployeeId
                ORDER BY SUM(o.TotalAmount) DESC";
            cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new EmployeeReport
                {
                    EmployeeName = r.GetString(0),
                    OrderCount = r.GetInt32(1),
                    Revenue = (decimal)r.GetDouble(2)
                });
            return list;
        }

       
        public (decimal revenue, int orders, int items) GetSummary(DateTime from, DateTime to)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COALESCE(SUM(o.TotalAmount),0), COUNT(o.Id),
                       COALESCE(SUM(oi.Quantity),0)
                FROM Orders o
                LEFT JOIN OrderItems oi ON oi.OrderId = o.Id
                WHERE o.Status = 'Closed'
                  AND DATE(o.ClosedAt) BETWEEN @from AND @to";
            cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
            using var r = cmd.ExecuteReader();
            r.Read();
            return ((decimal)r.GetDouble(0), r.GetInt32(1), r.GetInt32(2));
        }
    }
}
