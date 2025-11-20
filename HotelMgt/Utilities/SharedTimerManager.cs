using System;
using System.Windows.Forms;

namespace HotelMgt.Utilities
{
    /// <summary>
    /// Provides a single shared timer for periodic refresh events across the application.
    /// Controls can subscribe to SharedTick to perform refresh logic.
    /// </summary>
    public static class SharedTimerManager
    {
        private static readonly System.Windows.Forms.Timer _timer = new() { Interval = 2000 };
        public static event EventHandler? SharedTick;

        static SharedTimerManager()
        {
            _timer.Tick += (s, e) => SharedTick?.Invoke(null, EventArgs.Empty);
            _timer.Start();
        }
    }
}