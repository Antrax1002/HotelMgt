using System;

namespace HotelMgt.Models
{
    public class CheckIn
    {
        public int CheckInId { get; set; }
        public int? ReservationId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime ExpectedCheckOutDate { get; set; }
        public DateTime? ActualCheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int CheckedInByEmployeeId { get; set; }
        public int? CheckedOutByEmployeeId { get; set; }

        // Navigation properties
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public bool IsCheckedOut => ActualCheckOutDate.HasValue;
        public int NightsStayed
        {
            get
            {
                var endDate = ActualCheckOutDate ?? DateTime.Today;
                return Math.Max(1, (endDate - CheckInDate).Days);
            }
        }
    }
}
