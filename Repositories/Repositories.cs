using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CaffeShop.Database;
using CaffeShop.Models;

namespace CaffeShop.Repositories
{
    // ─── MENU REPOSITORY ───────────────────────────────────────────────────────
    public class MenuRepository
    {
        public List<MenuItem> GetAll()
        {
            var list = new List<MenuItem>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM MenuItems ORDER BY Category, Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public List<MenuItem> GetAvailable()
        {
            var list = new List<MenuItem>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM MenuItems WHERE IsAvailable=1 ORDER BY Category, Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public void Add(MenuItem item)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO MenuItems (Name, Category, Price, IsAvailable, Description)
                                VALUES (@n,@c,@p,@a,@d)";
            cmd.Parameters.AddWithValue("@n", item.Name);
            cmd.Parameters.AddWithValue("@c", item.Category);
            cmd.Parameters.AddWithValue("@p", item.Price);
            cmd.Parameters.AddWithValue("@a", item.IsAvailable ? 1 : 0);
            cmd.Parameters.AddWithValue("@d", item.Description ?? "");
            cmd.ExecuteNonQuery();
        }

        public void Update(MenuItem item)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE MenuItems SET Name=@n, Category=@c, Price=@p,
                                IsAvailable=@a, Description=@d WHERE Id=@id";
            cmd.Parameters.AddWithValue("@n", item.Name);
            cmd.Parameters.AddWithValue("@c", item.Category);
            cmd.Parameters.AddWithValue("@p", item.Price);
            cmd.Parameters.AddWithValue("@a", item.IsAvailable ? 1 : 0);
            cmd.Parameters.AddWithValue("@d", item.Description ?? "");
            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MenuItems WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private MenuItem Map(SQLiteDataReader r) => new MenuItem
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Category = r.GetString(2),
            Price = (decimal)r.GetDouble(3),
            IsAvailable = r.GetInt32(4) == 1,
            Description = r.IsDBNull(5) ? "" : r.GetString(5)
        };
    }

    
    public class EmployeeRepository
    {
        public List<Employee> GetAll()
        {
            var list = new List<Employee>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Employees ORDER BY LastName";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public List<Employee> GetActive()
        {
            var list = new List<Employee>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Employees WHERE IsActive=1 ORDER BY LastName";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public void Add(Employee emp)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Employees (FirstName,LastName,Position,Phone,Email,HireDate,IsActive)
                                VALUES (@fn,@ln,@pos,@ph,@em,@hd,@act)";
            cmd.Parameters.AddWithValue("@fn", emp.FirstName);
            cmd.Parameters.AddWithValue("@ln", emp.LastName);
            cmd.Parameters.AddWithValue("@pos", emp.Position);
            cmd.Parameters.AddWithValue("@ph", emp.Phone ?? "");
            cmd.Parameters.AddWithValue("@em", emp.Email ?? "");
            cmd.Parameters.AddWithValue("@hd", emp.HireDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@act", emp.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public void Update(Employee emp)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Employees SET FirstName=@fn,LastName=@ln,Position=@pos,
                                Phone=@ph,Email=@em,HireDate=@hd,IsActive=@act WHERE Id=@id";
            cmd.Parameters.AddWithValue("@fn", emp.FirstName);
            cmd.Parameters.AddWithValue("@ln", emp.LastName);
            cmd.Parameters.AddWithValue("@pos", emp.Position);
            cmd.Parameters.AddWithValue("@ph", emp.Phone ?? "");
            cmd.Parameters.AddWithValue("@em", emp.Email ?? "");
            cmd.Parameters.AddWithValue("@hd", emp.HireDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@act", emp.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", emp.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Employees SET IsActive=0 WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private Employee Map(SQLiteDataReader r) => new Employee
        {
            Id = r.GetInt32(0),
            FirstName = r.GetString(1),
            LastName = r.GetString(2),
            Position = r.GetString(3),
            Phone = r.IsDBNull(4) ? "" : r.GetString(4),
            Email = r.IsDBNull(5) ? "" : r.GetString(5),
            HireDate = DateTime.Parse(r.GetString(6)),
            IsActive = r.GetInt32(7) == 1
        };
    }

    
    public class OrderRepository
    {
        public List<Order> GetOpen()
        {
            var list = new List<Order>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
    SELECT o.Id, o.TableNumber, o.EmployeeId, o.Status,
  o.CreatedAt, o.ClosedAt, o.TotalAmount,
  e.FirstName || ' ' || e.LastName AS EmployeeName
FROM Orders o
JOIN Employees e ON o.EmployeeId = e.Id
WHERE DATE(o.CreatedAt) = DATE('now')
ORDER BY o.CreatedAt DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapOrder(reader));
            return list;
        }

        public List<Order> GetToday()
        {
            var list = new List<Order>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.*, e.FirstName || ' ' || e.LastName
                FROM Orders o
                JOIN Employees e ON o.EmployeeId = e.Id
                WHERE DATE(o.CreatedAt) = DATE('now')
                ORDER BY o.CreatedAt DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapOrder(reader));
            return list;
        }

        public int CreateOrder(int tableNumber, int employeeId)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Orders (TableNumber, EmployeeId, Status, CreatedAt, TotalAmount)
                                VALUES (@t, @e, 'Open', @ca, 0);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@t", tableNumber);
            cmd.Parameters.AddWithValue("@e", employeeId);
            cmd.Parameters.AddWithValue("@ca", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void AddItem(OrderItem item)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, UnitPrice)
                                VALUES (@oid, @mid, @qty, @price)";
            cmd.Parameters.AddWithValue("@oid", item.OrderId);
            cmd.Parameters.AddWithValue("@mid", item.MenuItemId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@price", item.UnitPrice);
            cmd.ExecuteNonQuery();

            
            var totalCmd = conn.CreateCommand();
            totalCmd.CommandText = @"UPDATE Orders SET TotalAmount =
                (SELECT COALESCE(SUM(Quantity * UnitPrice),0) FROM OrderItems WHERE OrderId=@id)
                WHERE Id=@id";
            totalCmd.Parameters.AddWithValue("@id", item.OrderId);
            totalCmd.ExecuteNonQuery();
        }

        public void RemoveItem(int orderItemId, int orderId)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM OrderItems WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", orderItemId);
            cmd.ExecuteNonQuery();

            var totalCmd = conn.CreateCommand();
            totalCmd.CommandText = @"UPDATE Orders SET TotalAmount =
                (SELECT COALESCE(SUM(Quantity * UnitPrice),0) FROM OrderItems WHERE OrderId=@id)
                WHERE Id=@id";
            totalCmd.Parameters.AddWithValue("@id", orderId);
            totalCmd.ExecuteNonQuery();
        }

        public List<OrderItem> GetItems(int orderId)
        {
            var list = new List<OrderItem>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT oi.*, m.Name FROM OrderItems oi
                                JOIN MenuItems m ON oi.MenuItemId = m.Id
                                WHERE oi.OrderId = @id";
            cmd.Parameters.AddWithValue("@id", orderId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OrderItem
                {
                    Id = reader.GetInt32(0),
                    OrderId = reader.GetInt32(1),
                    MenuItemId = reader.GetInt32(2),
                    Quantity = reader.GetInt32(3),
                    UnitPrice = (decimal)reader.GetDouble(4),
                    MenuItemName = reader.GetString(5)
                });
            }
            return list;
        }

        public void CloseOrder(int orderId)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Orders SET Status='Closed', ClosedAt=@ca WHERE Id=@id";
            cmd.Parameters.AddWithValue("@ca", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@id", orderId);
            cmd.ExecuteNonQuery();
        }

        public (decimal revenue, int count) GetTodayStats()
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COALESCE(SUM(TotalAmount),0), COUNT(*)
                                FROM Orders
                                WHERE Status='Closed' AND DATE(ClosedAt) = DATE('now')";
            using var r = cmd.ExecuteReader();
            r.Read();
            return ((decimal)r.GetDouble(0), r.GetInt32(1));
        }

        private Order MapOrder(SQLiteDataReader r)
        {
            
            int idIdx = r.GetOrdinal("Id");
            int tableIdx = r.GetOrdinal("TableNumber");
            int empIdIdx = r.GetOrdinal("EmployeeId");
            int statusIdx = r.GetOrdinal("Status");
            int createdIdx = r.GetOrdinal("CreatedAt");
            int closedIdx = r.GetOrdinal("ClosedAt");
            int totalIdx = r.GetOrdinal("TotalAmount");
            int empNameIdx = r.GetOrdinal("EmployeeName"); 

            DateTime createdAt = DateTime.MinValue;
            var createdObj = r.GetValue(createdIdx);
            if (createdObj != DBNull.Value)
                DateTime.TryParse(createdObj.ToString(), out createdAt);

            DateTime? closedAt = null;
            if (!r.IsDBNull(closedIdx))
            {
                var closedObj = r.GetValue(closedIdx);
                if (DateTime.TryParse(closedObj.ToString(), out var cd))
                    closedAt = cd;
            }

            return new Order
            {
                Id = Convert.ToInt32(r.GetValue(idIdx)),
                TableNumber = Convert.ToInt32(r.GetValue(tableIdx)),
                EmployeeId = Convert.ToInt32(r.GetValue(empIdIdx)),
                Status = r.IsDBNull(statusIdx) ? null : r.GetString(statusIdx),
                CreatedAt = createdAt,
                ClosedAt = closedAt,
                TotalAmount = r.IsDBNull(totalIdx) ? 0m : Convert.ToDecimal(r.GetValue(totalIdx)),
                EmployeeName = r.IsDBNull(empNameIdx) ? string.Empty : r.GetValue(empNameIdx).ToString()
            };
        }
    }
}
