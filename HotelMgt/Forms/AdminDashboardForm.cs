using HotelMgt.Custom;          // RoundedPanel
using HotelMgt.otherUI;
using HotelMgt.Services;
using HotelMgt.UIStyles;        // builder helpers
using HotelMgt.UserControls.Admin;
using HotelMgt.UserControls.Employee;
using HotelMgt.Utilities;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HotelMgt.Forms
{
    public partial class AdminDashboardForm : Form
    {
        private readonly AuthenticationService _authService;
        private readonly DatabaseService _dbService;

        // Overview stats labels
        private Label lblAvailableRooms = null!;
        private Label lblOccupiedRooms = null!;
        private Label lblReservedRooms = null!;
        private Label lblActiveCheckIns = null!;

        // Admin overview grids
        private DataGridView dgvCurrentOccupancy = null!;

        // Header logo
        private RoundedPanel? _headerLogoPanel;

        private bool _suppressLogoutPrompt;
        private ImageList? _tabImages; // icons (optional)
        private Panel? _tabHost;

        // Activity Logs controls (Overview)
        private DateTimePicker dtpLogDate = null!;
        private ComboBox cboLogEmployee = null!;
        private ComboBox cboLogType = null!;
        private Label lblLogSummary = null!;
        private DataGridView dgvActivityLogs = null!;
        private Label lblActivityLogsEmpty = null!;

        // Live updates
        private DateTime? _lastActivityMax;
        private DateTime? _lastPaymentMax;

        public AdminDashboardForm()
        {
            InitializeComponent();

            _authService = new AuthenticationService();
            _dbService = new DatabaseService();

            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            btnLogout.Click += btnLogout_Click;

            // Header design: fully delegated to builder
            AdminDashboardViewBuilder.InitializeHeader(panelHeader, lblTitle, lblWelcome, btnLogout, out _headerLogoPanel);

            // Overview UI (design from builder)
            AdminDashboardViewBuilder.BuildOverviewTab(
                tabOverview,
                out lblAvailableRooms, out lblOccupiedRooms, out lblReservedRooms, out lblActiveCheckIns,
                out dgvCurrentOccupancy,
                out dtpLogDate, out cboLogEmployee, out cboLogType,
                out lblLogSummary, out dgvActivityLogs, out lblActivityLogsEmpty);

            OccupancyNotesEditor.Enable(
                dgvCurrentOccupancy,
                () => _dbService.GetConnection()
            );
            // Populate filters BEFORE wiring
            LoadEmployeesForLogFilter();
            LoadActivityTypesForLogFilter();
            cboLogEmployee.SelectedIndex = cboLogEmployee.Items.Count > 0 ? 0 : -1;
            cboLogType.SelectedIndex = cboLogType.Items.Count > 0 ? 0 : -1;

            // Wire log filters
            dtpLogDate.ValueChanged += (_, __) => LoadActivityLogs();
            cboLogEmployee.SelectedIndexChanged += (_, __) => LoadActivityLogs();
            cboLogType.SelectedIndexChanged += (_, __) => LoadActivityLogs();

            LoadUserControls();

            // Tabs design consolidated
            var iconMap = new Dictionary<TabPage, string>();
            TryAddIcon(tabOverview, "ic_home", iconMap);
            TryAddIcon(tabCheckIn, "ic_checkin", iconMap);
            TryAddIcon(tabCheckOut, "ic_checkout", iconMap);
            TryAddIcon(tabReservations, "ic_calendar", iconMap);
            TryAddIcon(tabAvailableRooms, "ic_bed", iconMap);
            TryAddIcon(tabGuestSearch, "ic_search", iconMap);
            TryAddIcon(tabEmployeeManagement, "ic_users", iconMap);
            TryAddIcon(tabRoomManagement, "ic_settings", iconMap);
            TryAddIcon(tabRevenueReport, "ic_chart", iconMap);

            AdminDashboardViewBuilder.SetupTabs(tabControl, leftPadding: 20, iconMap, out _tabImages, out _tabHost);

            // Initial loads after filters are populated
            LoadOverviewStats();
            LoadActivityLogs();

            SharedTimerManager.SharedTick += SharedTimerManager_SharedTick;
        }

        private static void TryAddIcon(TabPage? page, string key, IDictionary<TabPage, string> map)
        {
            if (page != null) map[page] = key;
        }

        private void AdminDashboardForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Hotel Management - {CurrentUser.FullName} ({CurrentUser.Role})";
            lblWelcome.Text = $"Welcome, {CurrentUser.FullName}";

            // No layout/style code here; builder already wired resize and styling
            LoadOverviewStats();
            LoadActivityLogs();
        }

        // NEW: wrap a content control in an AutoScroll host so it remains visible on small screens
        private static void InstallInScrollHost(TabPage page, Control content)
        {
            if (page == null || content == null) return;

            page.SuspendLayout();
            page.Controls.Clear();

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = page.BackColor,
                Padding = new Padding(0)
            };

            // Keep the original layout expectations: let the content fill,
            // but provide scrollbars by setting AutoScrollMinSize from PreferredSize.
            content.Margin = Padding.Empty;
            content.Dock = DockStyle.Fill;
            content.MaximumSize = new Size(int.MaxValue, int.MaxValue);

            void UpdateScrollExtent()
            {
                // PreferredSize reflects the natural size of the control and its children
                var pref = content.PreferredSize;

                // Ensure non-negative values; width is typically managed by the host,
                // we mostly need vertical scrolling.
                var minW = Math.Max(0, pref.Width);
                var minH = Math.Max(0, pref.Height);

                // Only update if changed to avoid layout churn
                if (host.AutoScrollMinSize.Width != minW || host.AutoScrollMinSize.Height != minH)
                {
                    host.AutoScrollMinSize = new Size(minW, minH);
                }
            }

            // Recompute scroll extent on layout/size changes
            content.Layout += (_, __) => UpdateScrollExtent();
            content.SizeChanged += (_, __) => UpdateScrollExtent();
            host.Resize += (_, __) => UpdateScrollExtent();

            host.Controls.Add(content);
            page.Controls.Add(host);

            // Initial computation
            UpdateScrollExtent();

            page.ResumeLayout(performLayout: true);
        }

        // NEW: a scroll host that uses Top + AutoSize, ideal for big dashboards like Reports
        private static void InstallInScrollHostTop(TabPage page, Control content)
        {
            if (page == null || content == null) return;

            page.SuspendLayout();
            page.Controls.Clear();

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = page.BackColor,
                Padding = new Padding(0)
            };

            // Let the content define its natural height; host provides vertical scroll
            content.Margin = Padding.Empty;
            content.AutoSize = true;                 // no AutoSizeMode usage
            content.Dock = DockStyle.Top;
            content.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            content.MaximumSize = new Size(int.MaxValue, int.MaxValue);

            // Keep content width in sync with host
            void SyncWidth(object? _, EventArgs __)
            {
                content.Width = host.ClientSize.Width - content.Margin.Horizontal;
            }
            host.Resize += SyncWidth;
            SyncWidth(null, EventArgs.Empty);

            host.Controls.Add(content);
            page.Controls.Add(host);

            page.ResumeLayout(performLayout: true);
        }

        private void LoadUserControls()
        {
            DashboardTabLoader.LoadStandardTabs(
                tabCheckIn!,
                tabCheckOut!,
                tabReservations!,
                tabAvailableRooms!,
                tabGuestSearch!,
                useScrollHost: true // Admin uses scroll host
            );

            // Employees
            var employeeManagementControl = new EmployeeManagementControl();
            InstallInScrollHost(tabEmployeeManagement!, employeeManagementControl);

            // Room Management
            var roomManagementControl = new RoomManagementControl();
            InstallInScrollHost(tabRoomManagement!, roomManagementControl);

            // Reports — RESTORE original behavior (no scroll host)
            var revenueReportControl = new RevenueReportControl();
            tabRevenueReport!.Controls.Clear();
            revenueReportControl.Dock = DockStyle.Fill;
            tabRevenueReport!.Controls.Add(revenueReportControl);
        }

        private void LoadOverviewStats()
        {
            try
            {
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
            if (dgvCurrentOccupancy == null) return;

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

        private void LoadEmployeesForLogFilter()
        {
            try
            {
                var items = new List<KeyValuePair<int?, string>>
                {
                    new KeyValuePair<int?, string>(null, "All Employees")
                };

                using var conn = _dbService.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand(
                    "SELECT EmployeeID, FirstName, LastName FROM Employees WHERE Role = @Role ORDER BY FirstName, LastName",
                    conn);
                cmd.Parameters.AddWithValue("@Role", "Employee");

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var id = rdr.GetInt32(0);
                    var name = $"{rdr.GetString(1)} {rdr.GetString(2)}";
                    items.Add(new KeyValuePair<int?, string>(id, name));
                }

                cboLogEmployee.DataSource = items;
                cboLogEmployee.DisplayMember = "Value";
                cboLogEmployee.ValueMember = "Key";
                if (cboLogEmployee.Items.Count > 0) cboLogEmployee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Fallback
                cboLogEmployee.DataSource = null;
                cboLogEmployee.Items.Clear();
                cboLogEmployee.Items.Add("All Employees");
                cboLogEmployee.SelectedIndex = 0;
            }
        }

        private void LoadActivityTypesForLogFilter()
        {
            var items = new List<KeyValuePair<string?, string>>
            {
                new KeyValuePair<string?, string>(null, "All Types"),
                new KeyValuePair<string?, string>("login", "Login"),
                new KeyValuePair<string?, string>("checkin", "Check-In"),
                new KeyValuePair<string?, string>("checkout", "Check-Out"),
                new KeyValuePair<string?, string>("reservation", "Reservation"),
                new KeyValuePair<string?, string>("payment", "Payment")
            };
            cboLogType.DataSource = items;
            cboLogType.DisplayMember = "Value";
            cboLogType.ValueMember = "Key";
            if (cboLogType.Items.Count > 0) cboLogType.SelectedIndex = 0;
        }

        private void LoadActivityLogs()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                var sql = @"
WITH Combined AS (
    SELECT 
        al.ActivityDateTime,
        (e.FirstName + ' ' + e.LastName) AS Employee,
        al.ActivityType,
        al.ActivityDescription,
        LOWER(REPLACE(REPLACE(al.ActivityType,'-',''),' ','')) AS NormType
    FROM ActivityLog al
    INNER JOIN Employees e ON al.EmployeeID = e.EmployeeID
    WHERE e.Role = 'Employee'
      AND CAST(al.ActivityDateTime AS DATE) = @Date
      AND (@EmpId IS NULL OR al.EmployeeID = @EmpId)

    UNION ALL

    SELECT 
        p.PaymentDate AS ActivityDateTime,
        (e2.FirstName + ' ' + e2.LastName) AS Employee,
        'Payment' AS ActivityType,
        ('Payment ' + p.PaymentStatus 
            + ' - ' + CONVERT(nvarchar(20), p.Amount) 
            + ' via ' + p.PaymentMethod
            + CASE WHEN p.TransactionReference IS NOT NULL AND LTRIM(RTRIM(p.TransactionReference)) <> '' 
                   THEN ' (Ref: ' + p.TransactionReference + ')' ELSE '' END
            + ' - Res#' + CONVERT(nvarchar(20), p.ReservationID)
            + CASE WHEN p.Notes IS NOT NULL AND LTRIM(RTRIM(p.Notes)) <> '' 
                   THEN ' - Notes: ' + p.Notes ELSE '' END
          ) AS ActivityDescription,
        'payment' AS NormType
    FROM Payments p
    INNER JOIN Employees e2 ON p.EmployeeID = e2.EmployeeID
    WHERE e2.Role = 'Employee'
      AND CAST(p.PaymentDate AS DATE) = @Date
      AND (@EmpId IS NULL OR p.EmployeeID = @EmpId)
)
SELECT ActivityDateTime, Employee, ActivityType, ActivityDescription
FROM Combined
WHERE (@TypePattern IS NULL OR NormType LIKE @TypePattern)
ORDER BY ActivityDateTime DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Date", dtpLogDate.Value.Date);

                var empId = (cboLogEmployee.SelectedValue as int?) ??
                            (cboLogEmployee.SelectedItem is KeyValuePair<int?, string> kv ? kv.Key : null);
                cmd.Parameters.AddWithValue("@EmpId", (object?)empId ?? DBNull.Value);

                var typeGroup = (cboLogType.SelectedValue as string) ??
                                (cboLogType.SelectedItem is KeyValuePair<string?, string> kv2 ? kv2.Key : null);
                var typePattern = typeGroup is null ? null : $"{typeGroup}%";
                cmd.Parameters.AddWithValue("@TypePattern", (object?)typePattern ?? DBNull.Value);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgvActivityLogs.DataSource = dt;

                var cols = dgvActivityLogs.Columns;

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

                dgvActivityLogs.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                lblLogSummary.Text = $"Showing {dt.Rows.Count} activities for {dtpLogDate.Value:yyyy-MM-dd}";

                DateTime? newMax = null;
                foreach (DataRow row in dt.Rows)
                {
                    var ts = (DateTime)row["ActivityDateTime"];
                    if (!newMax.HasValue || ts > newMax.Value) newMax = ts;
                }
                _lastActivityMax = newMax;
                _lastPaymentMax = newMax;

                bool empty = dt.Rows.Count == 0;
                lblActivityLogsEmpty.Visible = empty;
                if (empty) lblActivityLogsEmpty.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activity logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabControl?.SelectedTab == tabOverview)
            {
                LoadOverviewStats();
                LoadActivityLogs();
            }
        }

        private void btnLogout_Click(object? sender, EventArgs e)
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
                    _suppressLogoutPrompt = true;
                    this.Close();
                };
                login.Show();
            }
        }

        private void AdminDashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_suppressLogoutPrompt) return;

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
                    _suppressLogoutPrompt = true;
                    this.Close();
                };
                login.Show();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void LogRefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (tabControl?.SelectedTab != tabOverview) return;

            try
            {
                var (activityMax, paymentMax) = GetLatestLogTimestamps();

                bool hasNewActivity = activityMax.HasValue && (!_lastActivityMax.HasValue || activityMax > _lastActivityMax);
                bool hasNewPayment = paymentMax.HasValue && (!_lastPaymentMax.HasValue || paymentMax > _lastPaymentMax);

                if (hasNewActivity || hasNewPayment)
                {
                    LoadActivityLogs();
                }
            }
            catch
            {
                // swallow
            }
        }

        private (DateTime? activityMax, DateTime? paymentMax) GetLatestLogTimestamps()
        {
            using var conn = _dbService.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
        SELECT MAX(ActivityDateTime)
        FROM ActivityLog
        WHERE CAST(ActivityDateTime AS DATE) = @Date
          AND (@EmpId IS NULL OR EmployeeID = @EmpId);

        SELECT MAX(PaymentDate)
        FROM Payments
        WHERE CAST(PaymentDate AS DATE) = @Date
          AND (@EmpId IS NULL OR EmployeeID = @EmpId);", conn);

            cmd.Parameters.AddWithValue("@Date", dtpLogDate.Value.Date);

            var empId = (cboLogEmployee.SelectedValue as int?) ??
                        (cboLogEmployee.SelectedItem is KeyValuePair<int?, string> kv ? kv.Key : null);
            cmd.Parameters.AddWithValue("@EmpId", (object?)empId ?? DBNull.Value);

            using var rdr = cmd.ExecuteReader();

            DateTime? aMax = null, pMax = null;

            if (rdr.Read() && !rdr.IsDBNull(0))
                aMax = rdr.GetDateTime(0);

            if (rdr.NextResult() && rdr.Read() && !rdr.IsDBNull(0))
                pMax = rdr.GetDateTime(0);

            return (aMax, pMax);
        }

        private void SharedTimerManager_SharedTick(object? sender, EventArgs e)
        {
            // Only refresh if the Overview tab is active
            if (tabControl?.SelectedTab != tabOverview) return;

            try
            {
                var (activityMax, paymentMax) = GetLatestLogTimestamps();

                bool hasNewActivity = activityMax.HasValue && (!_lastActivityMax.HasValue || activityMax > _lastActivityMax);
                bool hasNewPayment = paymentMax.HasValue && (!_lastPaymentMax.HasValue || paymentMax > _lastPaymentMax);

                if (hasNewActivity || hasNewPayment)
                {
                    LoadActivityLogs();
                }
            }
            catch
            {
                // swallow
            }
        }
    }
}