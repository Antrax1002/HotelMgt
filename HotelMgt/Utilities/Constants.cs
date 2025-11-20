using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelMgt.Utilities
{
    public static class Constants
    {
        // Database connection string - UPDATE THIS
        public const string ConnectionString =
                "Data Source=ANTRAX;Initial Catalog=HotelManagementDB;Integrated Security=True;Trust Server Certificate=True";

        // Room Status
        public const string RoomStatusAvailable = "Available";
        public const string RoomStatusOccupied = "Occupied";
        public const string RoomStatusCleaning = "Cleaning";
        public const string RoomStatusMaintenance = "Maintenance";

        // Room Types
        public const string RoomTypeSingle = "Single";
        public const string RoomTypeDouble = "Double";
        public const string RoomTypeSuite = "Suite";
        public const string RoomTypeDeluxe = "Deluxe";

        // Payment Methods
        public const string PaymentCash = "Cash";
        public const string PaymentCard = "Credit Card";
        public const string PaymentDebit = "Debit Card";

        // Reservation Status
        public const string ReservationPending = "Pending";
        public const string ReservationConfirmed = "Confirmed";
        public const string ReservationCheckedIn = "Checked-In";
        public const string ReservationCompleted = "Completed";
        public const string ReservationCancelled = "Cancelled";

        // Employee Roles
        public const string RoleEmployee = "Employee";
        public const string RoleAdmin = "Admin";
    }
}
