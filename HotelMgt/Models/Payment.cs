using System;

namespace HotelMgt.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int CheckInId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public int ProcessedByEmployeeId { get; set; }
        public string TransactionReference { get; set; } = string.Empty;

        // Navigation properties
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }
}
