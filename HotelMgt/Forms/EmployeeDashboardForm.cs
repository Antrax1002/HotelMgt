using HotelMgt.Custom; // RoundedPanel
using HotelMgt.Services;
using HotelMgt.UIStyles; // ADD
using HotelMgt.UserControls.Employee;
using HotelMgt.Utilities;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis; // ADD
using System.Drawing;
using System.Drawing.Drawing2D; // ADD
using System.Drawing.Text;
using System.IO; // ADD
using System.Windows.Forms;
using HotelMgt.otherUI; // OccupancyNotesEditor

namespace HotelMgt.Forms
{
    public partial class EmployeeDashboardForm : Form
    {
        private readonly AuthenticationService _authService;
        private readonly DatabaseService _dbService;

        // Overview tab UI
        private Label lblAvailableRooms = null!;
        private Label lblOccupiedRooms = null!;
        private Label lblReservedRooms = null!;
        private Label lblActiveCheckIns = null!;

        private DataGridView dgvCurrentOccupancy = null!;

        // Header logo
        private RoundedPanel? _headerLogoPanel; // kept; panel is built by builder

        private bool _suppressLogoutPrompt; // ADD
        private ImageList? _tabImages; // icons (optional)
        private Panel? _tabHost;

        public EmployeeDashboardForm()
        {
            InitializeComponent();

            // Ensure clicking X prompts for logout (matches AdminDashboard behavior)
            this.FormClosing += EmployeeDashboardForm_FormClosing;

            // NEW: swap in a borderless TabControl (removes left/right page lines)
            tabControl = TabControlBorderless.Replace(tabControl);

            _authService = new AuthenticationService();
            _dbService = new DatabaseService();

            panelHeader.Resize += PanelHeader_Resize;
            btnLogout.Click += BtnLogout_Click;

            // Build header UI (logo box etc.)
            EmployeeDashboardViewBuilder.BuildHeader(panelHeader, out _headerLogoPanel);
            if (_headerLogoPanel != null)
            {
                EmployeeDashboardViewBuilder.LayoutHeaderStack(panelHeader, _headerLogoPanel, lblTitle, lblWelcome);
            }

            // Build Overview UI first, then load stats
            EmployeeDashboardViewBuilder.BuildOverviewTab(
                tabOverview,
                out lblAvailableRooms, out lblOccupiedRooms, out lblReservedRooms, out lblActiveCheckIns,
                out dgvCurrentOccupancy);

            LoadUserControls();
            LoadOverviewStats();
            TrimEmployeeTabs(); // remove admin-only pages

            OccupancyNotesEditor.Enable(
                dgvCurrentOccupancy,
                () => _dbService.GetConnection()
            );

            // Left gutter for tab bar + softer pages
            EmployeeDashboardViewBuilder.EmbedTabControlInHost(tabControl, leftPadding: 20, ref _tabHost);

            // Add top spacing and unify background
            if (_tabHost != null)
            {
                var pad = _tabHost.Padding;
                _tabHost.Padding = new Padding(pad.Left > 0 ? pad.Left : 20, 16, pad.Right, 20); // Top = 16px
                _tabHost.BackColor = this.BackColor; // keep the same light page background
            }

            var iconMap = new Dictionary<TabPage, string>();
            TryAddIcon(tabOverview, "ic_home", iconMap);
            TryAddIcon(tabCheckIn, "ic_checkin", iconMap);
            TryAddIcon(tabCheckOut, "ic_checkout", iconMap);
            TryAddIcon(tabReservations, "ic_calendar", iconMap);
            TryAddIcon(tabAvailableRooms, "ic_bed", iconMap);
            TryAddIcon(tabGuestSearch, "ic_search", iconMap);

            EmployeeDashboardViewBuilder.StyleTabControl(tabControl, iconMap, out _tabImages);
            EmployeeDashboardViewBuilder.SoftenTabPages(tabControl);

            tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl.SelectedTab == tabOverview)
                    LoadOverviewStats();
            };
        }

        private static void TryAddIcon(TabPage? page, string key, IDictionary<TabPage, string> map)
        {
            if (page != null) map[page] = key;
        }

        private void EmployeeDashboardForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Hotel Management - {CurrentUser.FullName} ({CurrentUser.Role})";
            lblWelcome.Text = $"Welcome, {CurrentUser.FullName}";


            // Ensure header layout/alignment
            if (_headerLogoPanel != null)
            {
                EmployeeDashboardViewBuilder.LayoutHeaderStack(panelHeader, _headerLogoPanel, lblTitle, lblWelcome);
            }
            EmployeeDashboardViewBuilder.AlignHeaderRightControls(panelHeader, btnLogout);
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _authService.Logout();

                this.Hide();
                var login = new LoginForm { StartPosition = FormStartPosition.CenterScreen };
                login.FormClosed += (_, __) =>
                {
                    _suppressLogoutPrompt = true; // suppress prompt when closing from LoginForm close
                    this.Close();
                };
                login.Show();
            }
        }

        private void EmployeeDashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_suppressLogoutPrompt)
            {
                // Allow closing silently (triggered by LoginForm closing)
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                e.Cancel = true;
                _authService.Logout();

                this.Hide();
                var login = new LoginForm { StartPosition = FormStartPosition.CenterScreen };
                login.FormClosed += (_, __) =>
                {
                    _suppressLogoutPrompt = true; // prevent prompt when we finally close
                    this.Close();
                };
                login.Show();
            }
            else
            {
                e.Cancel = true;
            }
        }

        // Update: include stacked title/welcome layout on resize
        private void PanelHeader_Resize(object? sender, EventArgs e)
        {
            if (_headerLogoPanel != null)
            {
                EmployeeDashboardViewBuilder.LayoutHeaderStack(panelHeader, _headerLogoPanel, lblTitle, lblWelcome);
            }
            EmployeeDashboardViewBuilder.AlignHeaderRightControls(panelHeader, btnLogout);
        }

        private void TrimEmployeeTabs()
        {
            // Remove admin-only tabs if present on Employee dashboard
            // Common admin tabs: Employees, Room Mgmt, Reports
            RemoveTabIfExists("tabEmployeeManagement");
            RemoveTabIfExists("tabRoomManagement");
            RemoveTabIfExists("tabRevenueReport");
            // Optionally remove other admin tabs if they exist
            RemoveTabIfExists("tabActivityLogs");
        }

        private void RemoveTabIfExists(string fieldName)
        {
            var field = GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (field?.GetValue(this) is TabPage page && page.Parent == tabControl)
            {
                tabControl.TabPages.Remove(page);
            }
        }

        private void LoadOverviewStats()
        {
            try
            {
                // Ensure labels are ready
                if (lblAvailableRooms == null || lblOccupiedRooms == null ||
                    lblReservedRooms == null || lblActiveCheckIns == null)
                {
                    EmployeeDashboardViewBuilder.BuildOverviewTab(
                        tabOverview,
                        out lblAvailableRooms, out lblOccupiedRooms, out lblReservedRooms, out lblActiveCheckIns,
                        out dgvCurrentOccupancy);

                    if (lblAvailableRooms == null) return; // still not ready; avoid null ref
                }

                using var conn = _dbService.GetConnection();
                conn.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Rooms WHERE Status = 'Available'", conn))
                    lblAvailableRooms.Text = Convert.ToString(cmd.ExecuteScalar()) ?? "0";

                using (var cmd = new SqlCommand(
                    "SELECT COUNT(DISTINCT RoomID) FROM CheckIns WHERE ActualCheckOutDateTime IS NULL", conn))
                    lblOccupiedRooms.Text = Convert.ToString(cmd.ExecuteScalar()) ?? "0";

                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(DISTINCT RoomID)
                    FROM Reservations
                    WHERE ReservationStatus IN ('Confirmed', 'Pending')
                      AND CheckInDate >= CAST(GETDATE() AS DATE)", conn))
                    lblReservedRooms.Text = Convert.ToString(cmd.ExecuteScalar()) ?? "0";

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM CheckIns WHERE ActualCheckOutDateTime IS NULL", conn))
                    lblActiveCheckIns.Text = Convert.ToString(cmd.ExecuteScalar()) ?? "0";

                if (dgvCurrentOccupancy != null)
                    LoadCurrentOccupancy(conn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCurrentOccupancy(SqlConnection existingConn)
        {
            try
            {
                const string query = @"
                    SELECT
                        rm.RoomNumber AS [Room],
                        (g.FirstName + ' ' + g.LastName) AS [GuestName],
                        g.PhoneNumber AS [Contact],
                        CAST(c.CheckInDateTime AS DATE) AS [Check In],
                        c.ExpectedCheckOutDate AS [Expected Check Out],
                        c.NumberOfGuests AS [Guests],
                        COALESCE(STRING_AGG(a.Name, ', '), '') AS [Amenities],
                        ISNULL(c.Notes, '') AS [Notes]
                    FROM CheckIns c
                    INNER JOIN Rooms rm ON c.RoomID = rm.RoomID
                    INNER JOIN Guests g ON c.GuestID = g.GuestID
                    LEFT JOIN CheckInAmenities cia ON cia.CheckInID = c.CheckInID
                    LEFT JOIN Amenities a ON a.AmenityID = cia.AmenityID
                    WHERE c.ActualCheckOutDateTime IS NULL
                    GROUP BY
                        rm.RoomNumber,
                        (g.FirstName + ' ' + g.LastName),
                        g.PhoneNumber,
                        CAST(c.CheckInDateTime AS DATE),
                        c.ExpectedCheckOutDate,
                        c.NumberOfGuests,
                        ISNULL(c.Notes, '')
                    ORDER BY MAX(c.CheckInDateTime) DESC";

                using var adapter = new SqlDataAdapter(query, existingConn);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgvCurrentOccupancy.DataSource = dt;

                var cols = dgvCurrentOccupancy.Columns;

                if (cols["GuestName"] is { } guestCol)
                    guestCol.HeaderText = "Guest";

                if (cols["Check In"] is { } checkInCol)
                    checkInCol.DefaultCellStyle.Format = "yyyy-MM-dd";

                if (cols["Expected Check Out"] is { } expectedCol)
                    expectedCol.DefaultCellStyle.Format = "yyyy-MM-dd";

                if (cols["Room"] is { } roomCol)
                    roomCol.FillWeight = 70;

                if (cols["Contact"] is { } contactCol)
                    contactCol.FillWeight = 120;

                if (cols["Guests"] is { } guestsCol)
                    guestsCol.FillWeight = 60;

                if (cols["Amenities"] is { } amenitiesCol)
                {
                    amenitiesCol.FillWeight = 220;
                    amenitiesCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                if (cols["Notes"] is { } notesCol)
                {
                    notesCol.FillWeight = 200;
                    notesCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                dgvCurrentOccupancy.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading current occupancy: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUserControls()
        {
            DashboardTabLoader.LoadStandardTabs(
                tabCheckIn,
                tabCheckOut,
                tabReservations,
                tabAvailableRooms,
                tabGuestSearch,
                useScrollHost: false // Employee does NOT use scroll host
            );
        }

        private static void SafeSetActivityGridColumns(DataGridView grid)
        {
            var cols = grid.Columns;

            if (cols["ActivityDateTime"] is { } timeCol)
            {
                timeCol.HeaderText = "Time";
                timeCol.DefaultCellStyle.Format = "HH:mm";
                timeCol.FillWeight = 70;
            }
            if (cols["Employee"] is { } empCol)
            {
                empCol.HeaderText = "Employee";
                empCol.FillWeight = 160;
            }
            if (cols["ActivityType"] is { } typeCol)
            {
                typeCol.HeaderText = "Activity Type";
                typeCol.FillWeight = 120;
            }
            if (cols["ActivityDescription"] is { } descCol)
            {
                descCol.HeaderText = "Description";
                descCol.FillWeight = 450;
                descCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
        }
    }
}