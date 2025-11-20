using System;

namespace HotelMgt.Utilities
{
    public static class CurrentUser
    {
        public static int EmployeeId { get; set; }
        public static string FirstName { get; set; } = string.Empty;
        public static string LastName { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;  // "Employee" or "Admin"
        public static string FullName => $"{FirstName} {LastName}";
        public static bool IsAdmin => Role == "Admin";
        public static bool IsLoggedIn => EmployeeId > 0;

        public static void Clear()
        {
            EmployeeId = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
        }
    }
}
