namespace HotelMgt.Utilities
{
    /// <summary>
    /// Result of a guest lookup operation, including guest ID, existence, and abort flag.
    /// </summary>
    public class GuestLookupResult
    {
        public bool IsExistingGuest { get; set; }
        public int GuestId { get; set; }
        public bool AbortCheckIn { get; set; }
    }
}