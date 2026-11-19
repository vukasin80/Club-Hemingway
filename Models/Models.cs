using System;
using System.Collections.Generic;

namespace CaffeShop.Models
{
    public enum UserRole { Admin, Konobar }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }

        public bool IsAdmin => Role == UserRole.Admin;
        public override string ToString() => $"{FullName} ({Role})";
    }

   
    public static class Session
    {
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.IsAdmin == true;
    }


    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{Name} - {Price:N0} RSD";
    }

    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public override string ToString() => $"{FullName} ({Position})";
    }

    public class Order
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public override string ToString() =>
            $"Sto #{TableNumber} | {Status} | {TotalAmount:N0} RSD";
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;

        public override string ToString() =>
            $"{MenuItemName} x{Quantity} = {Subtotal:N0} RSD";
    }
}
