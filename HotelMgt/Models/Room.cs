using System;

namespace HotelMgt.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; }
        public string Status { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public string Amenities { get; set; } = string.Empty;
        public DateTime? LastMaintenanceDate { get; set; }
    }
}
