using System;

namespace HotelMgt.Models
{
    public class Employee
    {
        public int EmployeeId   { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Username  { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role      { get; set; } = string.Empty;
        public bool IsActive    { get; set; }
        public DateTime HireDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
