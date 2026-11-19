using System.Collections.Generic;
using System.Data.SQLite;
using CaffeShop.Database;
using CaffeShop.Models;

namespace CaffeShop.Repositories
{
    public class UserRepository
    {
        public User Login(string username, string password)
        {
            string hash = DatabaseManager.HashPassword(password);
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Users
                                WHERE Username=@u AND PasswordHash=@p AND IsActive=1";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", hash);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return Map(r);
        }

        public List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users ORDER BY FullName";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public void Add(User user)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (Username,PasswordHash,Role,FullName,IsActive)
                                VALUES (@u,@p,@r,@fn,@a)";
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.PasswordHash);
            cmd.Parameters.AddWithValue("@r", user.Role.ToString());
            cmd.Parameters.AddWithValue("@fn", user.FullName);
            cmd.Parameters.AddWithValue("@a", user.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public void Update(User user)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Users SET Username=@u, Role=@r, FullName=@fn, IsActive=@a WHERE Id=@id";
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@r", user.Role.ToString());
            cmd.Parameters.AddWithValue("@fn", user.FullName);
            cmd.Parameters.AddWithValue("@a", user.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.ExecuteNonQuery();
        }

        public void ChangePassword(int userId, string newPassword)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET PasswordHash=@p WHERE Id=@id";
            cmd.Parameters.AddWithValue("@p", DatabaseManager.HashPassword(newPassword));
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET IsActive=0 WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private User Map(SQLiteDataReader r) => new User
        {
            Id = r.GetInt32(0),
            Username = r.GetString(1),
            PasswordHash = r.GetString(2),
            Role = r.GetString(3) == "Admin" ? UserRole.Admin : UserRole.Konobar,
            FullName = r.GetString(4),
            IsActive = r.GetInt32(5) == 1
        };
    }
}
