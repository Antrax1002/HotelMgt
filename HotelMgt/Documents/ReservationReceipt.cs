using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelMgt.Documents
{
    public class ReservationReceipt
    {
        public string ReservationCode { get; set; } = "";
        public string GuestFullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int NumberOfGuests { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string RoomNumber { get; set; } = "";
        public string RoomType { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public int Nights { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> Inclusions { get; set; } = new List<string>();
        public List<(string Title, string Body)> BankAccounts { get; set; } = new();
    }

    
}
