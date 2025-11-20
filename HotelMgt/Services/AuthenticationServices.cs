using HotelMgt.Models;
using HotelMgt.Utilities;
using Microsoft.Data.SqlClient;
using System;

namespace HotelMgt.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseService _dbService;
        private readonly ActivityLogService _logService;

        public AuthenticationService()
        {
            _dbService = new DatabaseService();
            _logService = new ActivityLogService();
        }

        public Employee? AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = _dbService.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT EmployeeId, FirstName, LastName, Email, PhoneNumber, 
                               Username, PasswordHash, Role, IsActive, HireDate
                        FROM Employees
                        WHERE Username = @Username AND IsActive = 1";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var employee = new Employee
                                {
                                    EmployeeId = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    LastName = reader.GetString(2),
                                    Email = reader.GetString(3),
                                    PhoneNumber = reader.GetString(4),
                                    Username = reader.GetString(5),
                                    PasswordHash = reader.GetString(6),
                                    Role = reader.GetString(7),
                                    IsActive = reader.GetBoolean(8),
                                    HireDate = reader.GetDateTime(9)
                                };

                                if (password == employee.PasswordHash)
                                {
                                    CurrentUser.EmployeeId = employee.EmployeeId;
                                    CurrentUser.FirstName = employee.FirstName;
                                    CurrentUser.LastName = employee.LastName;
                                    CurrentUser.Email = employee.Email;
                                    CurrentUser.Role = employee.Role;

                                    _logService.LogActivity(
                                        employee.EmployeeId,
                                        "Login",
                                        $"{employee.FullName} logged in"
                                    );

                                    return employee;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication error: {ex.Message}");
            }

            return null;
        }

        public void Logout()
        {
            if (CurrentUser.IsLoggedIn)
            {
                _logService.LogActivity(
                    CurrentUser.EmployeeId,
                    "Logout",
                    $"{CurrentUser.FullName} logged out"
                );

                CurrentUser.Clear();
            }
        }
    }
}
