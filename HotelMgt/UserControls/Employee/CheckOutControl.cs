using System;
using System.Data;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelMgt.Services;
using HotelMgt.Utilities;
using HotelMgt.UIStyles; // ADD
using System.Linq;       // ADD
using HotelMgt.Custom;

namespace HotelMgt.UserControls.Employee
{
    public partial class CheckOutControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly ActivityLogService _logService;

        private TextBox txtSearch = null!;
        private RoundedButton btnSearch = null!;
        private DataGridView dgvActiveCheckIns = null!;
        private Panel panelCheckOutDetails = null!;
        private Label lblGuestName = null!;
        private Label lblRoomInfo = null!;
        private Label lblCheckInDate = null!;
        private Label lblCheckOutDate = null!;
        private Label lblExpectedCheckOutDate = null!;
        private Label lblNights = null!;
        private Label lblRatePerNight = null!;
        private TextBox txtExtra = null!;
        private Label lblTotalAmount = null!;
        private ComboBox cboPaymentMethod = null!;
        private TextBox txtTransactionRef = null!;
        private RoundedButton btnProcessCheckOut = null!;

        // amenities grid reference
        private DataGridView dgvAmenities = null!;
        // NEW: description box reference
        private TextBox txtCheckInDescription = null!;

        private int selectedCheckInId = 0;
        private decimal totalAmount = 0;

        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        public CheckOutControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _logService = new ActivityLogService();
            this.Load += CheckOutControl_Load;
        }

        private void CheckOutControl_Load(object? sender, EventArgs e)
        {
            InitializeControls();

            // Locate the amenities grid and description created by the builder
            dgvAmenities = panelCheckOutDetails.Controls.Find("dgvAmenities", true)
                .OfType<DataGridView>()
                .FirstOrDefault()!;

            txtCheckInDescription = panelCheckOutDetails.Controls.Find("txtCheckInDescription", true)
                .OfType<TextBox>()
                .FirstOrDefault()!;

            if (dgvAmenities != null)
                GridTheme.ApplyStandard(dgvAmenities);

            txtSearch.KeyDown -= TxtSearch_KeyDown;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            txtExtra.TextChanged += TxtExtra_TextChanged;

            cboPaymentMethod.SelectedIndexChanged -= CboPaymentMethod_SelectedIndexChanged;
            cboPaymentMethod.SelectedIndexChanged += CboPaymentMethod_SelectedIndexChanged;
            CboPaymentMethod_SelectedIndexChanged(this, EventArgs.Empty);

            LoadActiveCheckIns();

            this.VisibleChanged -= CheckOutControl_VisibleChanged;
            this.VisibleChanged += CheckOutControl_VisibleChanged;
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadActiveCheckIns(txtSearch.Text.Trim());
            }
        }

        private void CboPaymentMethod_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var method = cboPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            bool needsRef = !string.Equals(method, "Cash", StringComparison.OrdinalIgnoreCase);
            txtTransactionRef.Enabled = needsRef;
            if (!needsRef) txtTransactionRef.Clear();
        }

        private void CheckOutControl_VisibleChanged(object? sender, EventArgs e)
        {
            if (this.Visible)
            {
                LoadActiveCheckIns(txtSearch?.Text.Trim() ?? string.Empty);
            }
        }

        // Build UI via UIStyles builder
        private void InitializeControls()
        {
            CheckOutViewBuilder.Build(
                this,
                out var lblTitle,
                out var lblSubtitle,
                out txtSearch,
                out btnSearch,
                out dgvActiveCheckIns,
                out panelCheckOutDetails,
                out lblGuestName,
                out lblRoomInfo,
                out lblCheckInDate,
                out lblCheckOutDate,
                out lblExpectedCheckOutDate,
                out lblNights,
                out lblRatePerNight,
                out txtExtra,
                out lblTotalAmount,
                out cboPaymentMethod,
                out txtTransactionRef,
                out btnProcessCheckOut
            );

            btnSearch.Click += BtnSearch_Click;
            dgvActiveCheckIns.SelectionChanged += DgvActiveCheckIns_SelectionChanged;
            btnProcessCheckOut.Click += BtnProcessCheckOut_Click;
        }

        private void TxtExtra_TextChanged(object? sender, EventArgs e)
        {
            // Parse the extra value
            decimal extra = 0;
            if (!string.IsNullOrWhiteSpace(txtExtra.Text) && decimal.TryParse(txtExtra.Text, out var val))
                extra = val;

            // Recompute and update the total amount label
            decimal displayTotal = totalAmount + extra;
            lblTotalAmount.Text = displayTotal.ToString("C2", _currencyCulture);
        }

        private void LoadActiveCheckIns(string searchTerm = "")
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                string query = @"
                    SELECT 
                        ci.CheckInID AS CheckInId,
                        rm.RoomNumber,
                        g.FirstName + ' ' + g.LastName AS GuestName,
                        CAST(ci.CheckInDateTime AS DATE) AS CheckInDate,
                        ci.ExpectedCheckOutDate,
                        ci.NumberOfGuests,
                        rm.PricePerNight
                    FROM CheckIns ci
                    INNER JOIN Rooms rm ON ci.RoomID = rm.RoomID
                    INNER JOIN Guests g ON ci.GuestID = g.GuestID
                    WHERE ci.ActualCheckOutDateTime IS NULL";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query += @" AND (rm.RoomNumber LIKE @Search OR g.FirstName + ' ' + g.LastName LIKE @Search)";
                }

                query += " ORDER BY ci.CheckInDateTime DESC";

                using var cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgvActiveCheckIns.DataSource = dt;

                var cols = dgvActiveCheckIns.Columns;

                if (cols["CheckInId"] is { } colId)
                    colId.Visible = false;

                if (cols["RoomNumber"] is { } colRoom)
                    colRoom.HeaderText = "Room";

                if (cols["GuestName"] is { } colGuest)
                    colGuest.HeaderText = "Guest Name";

                if (cols["CheckInDate"] is { } colIn)
                    colIn.HeaderText = "Check-In";

                if (cols["ExpectedCheckOutDate"] is { } colExpected)
                    colExpected.HeaderText = "Expected Out";

                if (cols["NumberOfGuests"] is { } colGuests)
                    colGuests.HeaderText = "Guests";

                if (cols["PricePerNight"] is { } colPrice)
                {
                    colPrice.HeaderText = "Rate/Night";
                    colPrice.DefaultCellStyle.Format = "C2";
                    colPrice.DefaultCellStyle.FormatProvider = _currencyCulture;
                }

                dgvActiveCheckIns.DefaultCellStyle.FormatProvider = _currencyCulture;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading check-ins: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            LoadActiveCheckIns(txtSearch.Text.Trim());
        }

        private void DgvActiveCheckIns_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvActiveCheckIns.SelectedRows.Count == 0) return;

            var row = dgvActiveCheckIns.SelectedRows[0];
            if (row.Cells["CheckInId"].Value is null) return;

            selectedCheckInId = Convert.ToInt32(row.Cells["CheckInId"].Value);
            string guestName = row.Cells["GuestName"].Value?.ToString() ?? "";
            string roomNumber = row.Cells["RoomNumber"].Value?.ToString() ?? "";

            DateTime checkInDate = Convert.ToDateTime(row.Cells["CheckInDate"].Value).Date;

            DateTime? expectedCheckOutDate = null;
            var expectedOutCell = row.Cells["ExpectedCheckOutDate"]?.Value;
            if (expectedOutCell is not null && expectedOutCell != DBNull.Value)
            {
                expectedCheckOutDate = Convert.ToDateTime(expectedOutCell).Date;
            }

            DateTime checkOutDate = DateTime.Today;

            decimal ratePerNight = Convert.ToDecimal(row.Cells["PricePerNight"].Value);

            int nightsStayed = Math.Max(1, (checkOutDate - checkInDate).Days);

            // Room charges
            var roomTotal = nightsStayed * ratePerNight;

            // Load amenities and compute total (also fills description)
            var amenitiesTotal = LoadAmenitiesForCheckIn(selectedCheckInId);

            totalAmount = roomTotal + amenitiesTotal;

            lblGuestName.Text = guestName;
            lblRoomInfo.Text = $"Room {roomNumber}";
            lblCheckInDate.Text = checkInDate.ToString("yyyy-MM-dd");
            lblCheckOutDate.Text = checkOutDate.ToString("yyyy-MM-dd");
            lblExpectedCheckOutDate.Text = expectedCheckOutDate?.ToString("yyyy-MM-dd") ?? "-";
            lblNights.Text = nightsStayed.ToString();
            lblRatePerNight.Text = ratePerNight.ToString("C2", _currencyCulture);
            lblTotalAmount.Text = totalAmount.ToString("C2", _currencyCulture);
            TxtExtra_TextChanged(null, EventArgs.Empty);

            panelCheckOutDetails.Visible = true;
        }

        // Retrieves amenities for the given CheckIn, binds grid, sets description; returns total amenities cost
        private decimal LoadAmenitiesForCheckIn(int checkInId)
        {
            if (dgvAmenities == null) return 0m;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                // 1) Amenities
                const string sql = @"
SELECT 
    cia.AmenityID,
    a.Category,
    a.Name,
    cia.Quantity,
    cia.UnitPrice,
    (cia.Quantity * cia.UnitPrice) AS LineTotal
FROM CheckInAmenities cia
INNER JOIN Amenities a ON a.AmenityID = cia.AmenityID
WHERE cia.CheckInID = @CheckInID
ORDER BY a.Category, a.Name;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CheckInID", checkInId);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgvAmenities.DataSource = dt;

                var cols = dgvAmenities.Columns;
                if (cols["AmenityID"] is { } cId) cId.Visible = false;
                if (cols["Category"] is { } cCat) cCat.HeaderText = "Category";
                if (cols["Name"] is { } cName) cName.HeaderText = "Amenity";
                if (cols["Quantity"] is { } cQty) cQty.HeaderText = "Qty";
                if (cols["UnitPrice"] is { } cPrice)
                {
                    cPrice.HeaderText = "Unit Price";
                    cPrice.DefaultCellStyle.Format = "C2";
                    cPrice.DefaultCellStyle.FormatProvider = _currencyCulture;
                }
                if (cols["LineTotal"] is { } cTotal)
                {
                    cTotal.HeaderText = "Total";
                    cTotal.DefaultCellStyle.Format = "C2";
                    cTotal.DefaultCellStyle.FormatProvider = _currencyCulture;
                }

                decimal sum = 0m;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LineTotal"] != DBNull.Value)
                        sum += Convert.ToDecimal(dr["LineTotal"]);
                }

                // 2) Description (Notes on CheckIns)
                if (txtCheckInDescription != null)
                {
                    using var descCmd = new SqlCommand("SELECT Notes FROM CheckIns WHERE CheckInID = @Id;", conn);
                    descCmd.Parameters.AddWithValue("@Id", checkInId);
                    var notesObj = descCmd.ExecuteScalar();
                    string notes = notesObj == DBNull.Value || notesObj == null ? "" : Convert.ToString(notesObj) ?? "";
                    txtCheckInDescription.Text = string.IsNullOrWhiteSpace(notes)
                        ? "No extra(s)."
                        : notes;
                }

                return sum;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading amenities/description: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0m;
            }
        }

        private void BtnProcessCheckOut_Click(object? sender, EventArgs e)
        {
            if (selectedCheckInId == 0)
            {
                MessageBox.Show("Please select a check-in to process.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboPaymentMethod.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a payment method.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var method = cboPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            if (!string.Equals(method, "Cash", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(txtTransactionRef.Text))
            {
                MessageBox.Show("Please enter a transaction reference for non-cash payments.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTransactionRef.Focus();
                return;
            }

            // Parse extra value
            decimal extra = 0;
            if (!string.IsNullOrWhiteSpace(txtExtra.Text) && decimal.TryParse(txtExtra.Text, out var val))
                extra = val;

            decimal finalTotal = totalAmount + extra;

            var confirm = MessageBox.Show(
                $"Process check-out with payment of {finalTotal.ToString("C2", _currencyCulture)}?",
                "Confirm Check-Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    int roomId, guestId;
                    int? reservationId;
                    using (var cmd = new SqlCommand(
                        "SELECT RoomId, ReservationID, GuestID FROM CheckIns WHERE CheckInId = @Id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", selectedCheckInId);
                        using var rdr = cmd.ExecuteReader();
                        if (!rdr.Read())
                            throw new InvalidOperationException("Selected check-in not found.");

                        roomId = rdr.GetInt32(0);
                        reservationId = rdr.IsDBNull(1) ? (int?)null : rdr.GetInt32(1);
                        guestId = rdr.GetInt32(2);
                    }

                    using (var cmd = new SqlCommand(@"
                        UPDATE CheckIns 
                        SET ActualCheckOutDateTime = @Out
                        WHERE CheckInId = @Id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Out", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Id", selectedCheckInId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO Payments (ReservationID, GuestID, EmployeeID, PaymentDate, Amount, PaymentMethod, TransactionReference)
                        VALUES (@ReservationID, @GuestID, @EmployeeID, @PaymentDate, @Amount, @PaymentMethod, @TransactionReference)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@ReservationID", (object?)reservationId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GuestID", guestId);
                        cmd.Parameters.AddWithValue("@EmployeeID", CurrentUser.EmployeeId);
                        cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Amount", finalTotal); // <-- Use finalTotal here
                        cmd.Parameters.AddWithValue("@PaymentMethod", method);
                        cmd.Parameters.AddWithValue("@TransactionReference",
                            string.IsNullOrWhiteSpace(txtTransactionRef.Text) ? (object)DBNull.Value : txtTransactionRef.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"
                        UPDATE Rooms SET Status = 'Available', UpdatedAt = @Now
                        WHERE RoomId = @RoomId", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@RoomId", roomId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }

                try
                {
                    _logService.LogActivity(
                        CurrentUser.EmployeeId,
                        "CheckOut",
                        $"Guest {lblGuestName.Text} checked out from {lblRoomInfo.Text}. Payment: {totalAmount.ToString("C2", _currencyCulture)}",
                        selectedCheckInId
                    );
                }
                catch { }

                MessageBox.Show(
                    $"Check-out processed successfully!\nTotal payment: {totalAmount.ToString("C2", _currencyCulture)}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                panelCheckOutDetails.Visible = false;
                selectedCheckInId = 0;
                LoadActiveCheckIns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing check-out: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}