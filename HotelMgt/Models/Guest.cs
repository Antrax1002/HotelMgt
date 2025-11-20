using System;

namespace HotelMgt.Models
{
    public class Guest
    {
        public int GuestId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IDType    { get; set; } = string.Empty;
        public string IDNumber  { get; set; } = string.Empty;
        public string Address   { get; set; } = string.Empty;
        public string City      { get; set; } = string.Empty;
        public string Country   { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt   { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
