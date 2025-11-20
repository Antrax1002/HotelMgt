using System;

namespace HotelMgt.Core.Events
{
    public static class RoomEvents
    {
        public static event EventHandler<RoomsChangedEventArgs>? RoomsChanged;

        public static void Publish(RoomChangeType changeType, int? roomId = null)
            => RoomsChanged?.Invoke(null, new RoomsChangedEventArgs(changeType, roomId));
    }

    public enum RoomChangeType { Added, Updated, Deleted, BulkRefreshed }

    public sealed class RoomsChangedEventArgs : EventArgs
    {
        public RoomsChangedEventArgs(RoomChangeType changeType, int? roomId)
        {
            ChangeType = changeType;
            RoomId = roomId;
        }
        public RoomChangeType ChangeType { get; }
        public int? RoomId { get; }
    }
}