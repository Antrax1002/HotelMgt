using System.Windows.Forms;
using HotelMgt.UserControls.Employee;
using HotelMgt.UserControls.Admin;

namespace HotelMgt.Utilities
{
    public static class DashboardTabLoader
    {
        public static void LoadStandardTabs(
            TabPage tabCheckIn,
            TabPage tabCheckOut,
            TabPage tabReservations,
            TabPage tabAvailableRooms,
            TabPage tabGuestSearch,
            bool useScrollHost = false)
        {
            void AddControl(TabPage tab, Control ctrl)
            {
                tab.Controls.Clear();
                ctrl.Dock = DockStyle.Fill;
                tab.Controls.Add(ctrl);
            }

            void AddWithScrollHost(TabPage tab, Control ctrl)
            {
                tab.Controls.Clear();
                var host = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = tab.BackColor
                };
                ctrl.Dock = DockStyle.Fill;
                host.Controls.Add(ctrl);
                tab.Controls.Add(host);
            }

            var checkIn = new CheckInControl();
            var checkOut = new CheckOutControl();
            var reservations = new ReservationControl();
            var availableRooms = new AvailableRoomsControl();
            var guestSearch = new GuestSearchControl();

            if (useScrollHost)
            {
                AddWithScrollHost(tabCheckIn, checkIn);
                AddWithScrollHost(tabCheckOut, checkOut);
                AddWithScrollHost(tabReservations, reservations);
                AddWithScrollHost(tabAvailableRooms, availableRooms);
                AddWithScrollHost(tabGuestSearch, guestSearch);
            }
            else
            {
                AddControl(tabCheckIn, checkIn);
                AddControl(tabCheckOut, checkOut);
                AddControl(tabReservations, reservations);
                AddControl(tabAvailableRooms, availableRooms);
                AddControl(tabGuestSearch, guestSearch);
            }
        }
    }
}