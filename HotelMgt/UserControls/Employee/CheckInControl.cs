using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using HotelMgt.Models;
using HotelMgt.Services;
using HotelMgt.Utilities;
using HotelMgt.UIStyles;
using HotelMgt.otherUI; // Add for AmenitiesPanel

namespace HotelMgt.UserControls.Employee
{
    public partial class CheckInControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly ActivityLogService _logService;
        private readonly GuestService _guestService = new GuestService();

        private TabControl tabCheckInRoot = null!;

        // Walk-In Controls
        private TextBox txtFirstName = null!;
        private TextBox txtMiddleName = null!;
        private TextBox txtLastName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private ComboBox cboIDType = null!;
        private TextBox txtIDNumber = null!;
        private ComboBox cboRoom = null!;
        private NumericUpDown numGuests = null!;
        private DateTimePicker dtpCheckOut = null!;
        private TextBox txtNotes = null!;
        private Button btnCheckIn = null!;
        private Label? lblNoAvailableRooms;

        // Reservation Controls
        private ComboBox cboReservation = null!; // legacy (hidden)
        private Panel panelReservationDetails = null!;
        private Label lblResGuestName = null!;
        private Label lblResRoomNumber = null!;
        private Label lblResCheckIn = null!;
        private Label lblResCheckOut = null!;
        private Label lblResGuests = null!;
        private Label lblResTotal = null!;
        private Label lblResSpecialRequests = null!;
        private Button btnCheckInReservation = null!;
        private Label? lblNoPendingReservations;
        private Button? btnCancelReservation;

        // Code lookup
        private TextBox txtReservationCodeLookup = null!;
        private Button btnLookupReservation = null!;

        // AmenitiesPanel (new, replaces all local amenities logic)
        private AmenitiesPanel amenitiesPanel = null!;

        // Current reservation
        private SelectedReservation? _currentReservation;

        private sealed class SelectedReservation
        {
            public int ReservationId { get; init; }
            public int RoomId { get; init; }
            public int GuestId { get; init; }
            public string GuestName { get; init; } = "";
            public string RoomNumber { get; init; } = "";
            public DateTime CheckInDate { get; init; }
            public DateTime CheckOutDate { get; init; }
            public int NumberOfGuests { get; init; }
            public decimal TotalAmount { get; init; }
            public string SpecialRequests { get; init; } = "";
        }

        public CheckInControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _logService = new ActivityLogService();
            Load += CheckInControl_Load;
        }

        private void CheckInControl_Load(object? sender, EventArgs e)
        {
            CheckInViewBuilder.Build(
                this,
                out var builtTab,
                out txtFirstName, out txtMiddleName, out txtLastName, out txtEmail, out txtPhone,
                out cboIDType, out txtIDNumber, out cboRoom, out numGuests, out dtpCheckOut,
                out txtNotes, out btnCheckIn, out var noRoomsLbl,
                out cboReservation, out panelReservationDetails,
                out lblResGuestName, out lblResRoomNumber, out lblResCheckIn, out lblResCheckOut,
                out lblResGuests, out lblResTotal, out lblResSpecialRequests, out btnCheckInReservation,
                out txtReservationCodeLookup, out btnLookupReservation,
                out var noResLbl,
                out amenitiesPanel // NEW: get AmenitiesPanel from builder
            );

            tabCheckInRoot = builtTab;
            lblNoAvailableRooms = noRoomsLbl;
            lblNoPendingReservations = noResLbl;

            // Walk-In
            btnCheckIn.Click += BtnCheckIn_Click;

            // Reservation lookup
            btnLookupReservation.Click += (_, __) => LookupReservationByCode();
            txtReservationCodeLookup.KeyDown += (_, ke) =>
            {
                if (ke.KeyCode == Keys.Enter)
                {
                    ke.SuppressKeyPress = true;
                    LookupReservationByCode();
                }
            };

            // Reservation actions
            btnCheckInReservation.Click += BtnCheckInReservation_Click;
            var cancelBtn = Controls.Find("btnCancelReservation", true).OfType<Button>().FirstOrDefault();
            if (cancelBtn != null)
            {
                btnCancelReservation = cancelBtn;
                btnCancelReservation.Click += BtnCancelReservation_Click;
            }

            LoadAvailableRooms();
            UpdateReservationActionState();
        }

        #region Reservation Lookup / Actions

        private void LookupReservationByCode()
        {
            var code = txtReservationCodeLookup.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter a reservation code.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReservationCodeLookup.Focus();
                return;
            }

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                const string sql = @"
SELECT TOP 1 
    r.ReservationID,
    r.RoomID,
    r.GuestID,
    g.FirstName + ' ' + g.LastName AS GuestName,
    rm.RoomNumber,
    r.CheckInDate,
    r.CheckOutDate,
    r.NumberOfGuests,
    r.TotalAmount,
    r.SpecialRequests
FROM Reservations r
INNER JOIN Guests g ON r.GuestID = g.GuestID
INNER JOIN Rooms rm ON r.RoomID = rm.RoomID
WHERE r.ReservationCode = @Code
  AND r.ReservationStatus = 'Confirmed'
  AND NOT EXISTS (
        SELECT 1 FROM CheckIns ci
        WHERE ci.ReservationID = r.ReservationID
          AND ci.ActualCheckOutDateTime IS NULL
  );";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", code);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    _currentReservation = new SelectedReservation
                    {
                        ReservationId = reader.GetInt32(0),
                        RoomId = reader.GetInt32(1),
                        GuestId = reader.GetInt32(2),
                        GuestName = reader.GetString(3),
                        RoomNumber = reader.GetString(4),
                        CheckInDate = reader.GetDateTime(5),
                        CheckOutDate = reader.GetDateTime(6),
                        NumberOfGuests = reader.GetInt32(7),
                        TotalAmount = reader.GetDecimal(8),
                        SpecialRequests = reader.IsDBNull(9) ? "" : reader.GetString(9)
                    };

                    lblResGuestName.Text = $"Guest: {_currentReservation.GuestName}";
                    lblResRoomNumber.Text = $"Room: {_currentReservation.RoomNumber}";
                    lblResCheckIn.Text = $"Check-In: {_currentReservation.CheckInDate:yyyy-MM-dd}";
                    lblResCheckOut.Text = $"Check-Out: {_currentReservation.CheckOutDate:yyyy-MM-dd}";
                    lblResGuests.Text = $"Guests: {_currentReservation.NumberOfGuests}";
                    lblResTotal.Text = $"Total: PHP {_currentReservation.TotalAmount:#,0.00}";
                    lblResSpecialRequests.Text = $"Special: {(string.IsNullOrWhiteSpace(_currentReservation.SpecialRequests) ? "None" : _currentReservation.SpecialRequests)}";

                    panelReservationDetails.Visible = true;
                    if (lblNoPendingReservations != null) lblNoPendingReservations.Visible = false;
                }
                else
                {
                    _currentReservation = null;
                    panelReservationDetails.Visible = false;
                    if (lblNoPendingReservations != null)
                    {
                        lblNoPendingReservations.Text = "No confirmed reservation found for that code (or already checked in).";
                        lblNoPendingReservations.Visible = true;
                    }
                    MessageBox.Show("No confirmed reservation found for that code (or already checked in).",
                        "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                UpdateReservationActionState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error looking up reservation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCheckInReservation_Click(object? sender, EventArgs e)
        {
            if (_currentReservation is null)
            {
                MessageBox.Show("Lookup a confirmed reservation first.", "No Reservation",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Use AmenitiesPanel for selected amenities
            var selectedAmenities = amenitiesPanel.GetSelectedAmenities();

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // Update reservation status
                    const string updateQuery = @"
UPDATE Reservations
SET ReservationStatus = 'CheckedIn', EmployeeID = @EmployeeId, UpdatedAt = @Now
WHERE ReservationID = @ReservationId";
                    using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", CurrentUser.EmployeeId);
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ReservationId", _currentReservation.ReservationId);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert CheckIn
                    const string insertCheckIn = @"
INSERT INTO CheckIns
(ReservationID, GuestID, RoomID, EmployeeID, CheckInDateTime, ExpectedCheckOutDate, NumberOfGuests, Notes, CreatedAt)
VALUES
(@ReservationID, @GuestID, @RoomID, @EmployeeID, @CheckInDateTime, @ExpectedCheckOutDate, @NumberOfGuests, @Notes, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int checkInId;
                    using (var cmd = new SqlCommand(insertCheckIn, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ReservationID", _currentReservation.ReservationId);
                        cmd.Parameters.AddWithValue("@GuestID", _currentReservation.GuestId);
                        cmd.Parameters.AddWithValue("@RoomID", _currentReservation.RoomId);
                        cmd.Parameters.AddWithValue("@EmployeeID", CurrentUser.EmployeeId);
                        cmd.Parameters.AddWithValue("@CheckInDateTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ExpectedCheckOutDate", _currentReservation.CheckOutDate);
                        cmd.Parameters.AddWithValue("@NumberOfGuests", _currentReservation.NumberOfGuests);
                        cmd.Parameters.AddWithValue("@Notes",
                            string.IsNullOrWhiteSpace(_currentReservation.SpecialRequests)
                                ? (object)DBNull.Value
                                : _currentReservation.SpecialRequests);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        checkInId = (int)cmd.ExecuteScalar()!;
                    }

                    if (selectedAmenities.Count > 0)
                        InsertCheckInAmenities(conn, transaction, checkInId, selectedAmenities);

                    UpdateRoomStatus(conn, transaction, _currentReservation.RoomId, "Occupied");

                    transaction.Commit();

                    try
                    {
                        _logService.LogActivity(
                            CurrentUser.EmployeeId,
                            "CheckIn",
                            $"Guest {_currentReservation.GuestName} checked into Room {_currentReservation.RoomNumber} (Reservation)",
                            checkInId);
                    }
                    catch { /* non-blocking */ }

                    MessageBox.Show(
                        $"Guest {_currentReservation.GuestName} successfully checked in to Room {_currentReservation.RoomNumber}!",
                        "Check-In Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _currentReservation = null;
                    panelReservationDetails.Visible = false;
                    txtReservationCodeLookup.Clear();
                    UpdateReservationActionState();
                    LoadAvailableRooms();
                }
                catch (Exception exInner)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show($"Error during reservation check-in: {exInner.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception exOuter)
            {
                MessageBox.Show($"Error: {exOuter.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelReservation_Click(object? sender, EventArgs e)
        {
            if (_currentReservation is null)
            {
                MessageBox.Show("Lookup a confirmed reservation first.", "No Reservation",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Cancel reservation for {_currentReservation.GuestName} - Room {_currentReservation.RoomNumber} on {_currentReservation.CheckInDate:yyyy-MM-dd}?",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    const string sql = @"
UPDATE Reservations
SET ReservationStatus = 'Cancelled', EmployeeID = @EmployeeId, UpdatedAt = @Now
WHERE ReservationID = @ReservationId;";
                    using (var cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", CurrentUser.EmployeeId);
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ReservationId", _currentReservation.ReservationId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();

                    try
                    {
                        _logService.LogActivity(
                            CurrentUser.EmployeeId,
                            "CancelReservation",
                            $"Cancelled reservation {_currentReservation.ReservationId} for {_currentReservation.GuestName} (Room {_currentReservation.RoomNumber})",
                            _currentReservation.ReservationId);
                    }
                    catch { }

                    MessageBox.Show("Reservation cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _currentReservation = null;
                    panelReservationDetails.Visible = false;
                    txtReservationCodeLookup.Clear();
                    UpdateReservationActionState();
                    LoadAvailableRooms();
                }
                catch (Exception exInner)
                {
                    try { tx.Rollback(); } catch { }
                    MessageBox.Show($"Error cancelling reservation: {exInner.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateReservationActionState()
        {
            bool hasSelection = _currentReservation != null;

            var activeBlue = Color.FromArgb(37, 99, 235);
            var activeRed = Color.FromArgb(220, 38, 38);
            var inactive = Color.FromArgb(148, 163, 184);

            btnCheckInReservation.BackColor = hasSelection ? activeBlue : inactive;
            btnCheckInReservation.ForeColor = Color.White;
            btnCheckInReservation.Cursor = hasSelection ? Cursors.Hand : Cursors.No;
            btnCheckInReservation.Enabled = hasSelection;

            if (btnCancelReservation != null)
            {
                btnCancelReservation.BackColor = hasSelection ? activeRed : inactive;
                btnCancelReservation.ForeColor = Color.White;
                btnCancelReservation.Cursor = hasSelection ? Cursors.Hand : Cursors.No;
                btnCancelReservation.Enabled = hasSelection;
            }
        }

        #endregion

        #region Load Data

        private void LoadAvailableRooms()
        {
            try
            {
                cboRoom.Items.Clear();

                using var conn = _dbService.GetConnection();
                conn.Open();

                const string query = @"
SELECT RoomID, RoomNumber, RoomType, PricePerNight, MaxOccupancy
FROM Rooms
WHERE Status = 'Available'
ORDER BY RoomNumber";

                using var cmd = new SqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var room = new
                    {
                        RoomId = reader.GetInt32(0),
                        RoomNumber = reader.GetString(1),
                        RoomType = reader.GetString(2),
                        PricePerNight = reader.GetDecimal(3),
                        MaxOccupancy = reader.GetInt32(4),
                        DisplayText =
                            $"Room {reader.GetString(1)} - {reader.GetString(2)} (PHP {reader.GetDecimal(3):#,0.00}/night) - Max {reader.GetInt32(4)} guests"
                    };
                    cboRoom.Items.Add(room);
                }

                bool hasRooms = cboRoom.Items.Count > 0;
                if (lblNoAvailableRooms != null) lblNoAvailableRooms.Visible = !hasRooms;

                cboRoom.Visible = hasRooms;
                cboRoom.Enabled = hasRooms;
                if (!hasRooms) cboRoom.SelectedIndex = -1;

                numGuests.Enabled = hasRooms;
                dtpCheckOut.Enabled = hasRooms;
                txtNotes.Enabled = hasRooms;
                btnCheckIn.Enabled = hasRooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available rooms: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event Handlers (Misc)

        private void CboReservation_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboReservation.SelectedItem != null)
            {
                dynamic reservation = cboReservation.SelectedItem;
                lblResGuestName.Text = reservation.GuestName;
                lblResRoomNumber.Text = $"Room {reservation.RoomNumber}";
                lblResCheckIn.Text = ((DateTime)reservation.CheckInDate).ToString("yyyy-MM-dd");
                lblResCheckOut.Text = ((DateTime)reservation.CheckOutDate).ToString("yyyy-MM-dd");
                lblResGuests.Text = reservation.NumberOfGuests.ToString();
                decimal total = (decimal)reservation.TotalAmount;
                lblResTotal.Text = $"PHP {total:#,0.00}";
                lblResSpecialRequests.Text = string.IsNullOrEmpty(reservation.SpecialRequests) ? "None" : reservation.SpecialRequests;
                panelReservationDetails.Visible = true;
            }
            else
            {
                panelReservationDetails.Visible = false;
            }
            UpdateReservationActionState();
        }

        private void BtnCheckIn_Click(object? sender, EventArgs e)
        {
            SqlTransaction transaction = null!;
            try
            {
                if (!ValidateWalkInInputs())
                    return;

                // Use AmenitiesPanel for selected amenities
                var selectedAmenities = amenitiesPanel.GetSelectedAmenities();

                using var conn = _dbService.GetConnection();
                conn.Open();
                transaction = conn.BeginTransaction();

                string firstName = txtFirstName.Text.Trim();
                string middleName = txtMiddleName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text?.Trim() ?? string.Empty;
                string idType = cboIDType.SelectedItem!.ToString()!;
                string idNumber = txtIDNumber.Text.Trim();

                // Use the helper, which will only rollback after the reader is disposed
                var lookup = GuestLookupHelper.LookupOrPromptGuest(
                    this, conn, transaction,
                    firstName, middleName, lastName,
                    (fn, mn, ln, em, ph, idt, idn) => {
                        txtFirstName.Text = fn;
                        txtMiddleName.Text = mn;
                        txtLastName.Text = ln;
                        txtEmail.Text = em;
                        txtPhone.Text = ph;
                        cboIDType.SelectedItem = idt;
                        txtIDNumber.Text = idn;
                    });

                if (lookup.AbortCheckIn)
                    return;

                int guestId = lookup.GuestId;
                if (!lookup.IsExistingGuest)
                {
                    guestId = _guestService.EnsureGuest(
                        conn, transaction,
                        firstName, middleName, lastName, phone, email, idType, idNumber
                    );
                }

                int roomId = (int)((dynamic)cboRoom.SelectedItem).RoomId;

                const string insertCheckIn = @"
INSERT INTO CheckIns
(ReservationID, GuestID, RoomID, EmployeeID, CheckInDateTime, ExpectedCheckOutDate, NumberOfGuests, Notes, CreatedAt)
VALUES
(NULL, @GuestID, @RoomID, @EmployeeID, @CheckInDateTime, @ExpectedCheckOutDate, @NumberOfGuests, @Notes, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int checkInId;
                using (var cmd = new SqlCommand(insertCheckIn, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@GuestID", guestId);
                    cmd.Parameters.AddWithValue("@RoomID", roomId);
                    cmd.Parameters.AddWithValue("@EmployeeID", CurrentUser.EmployeeId);
                    cmd.Parameters.AddWithValue("@CheckInDateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ExpectedCheckOutDate", dtpCheckOut.Value.Date);
                    cmd.Parameters.AddWithValue("@NumberOfGuests", (int)numGuests.Value);
                    cmd.Parameters.AddWithValue("@Notes",
                        string.IsNullOrWhiteSpace(txtNotes.Text) ? (object)DBNull.Value : txtNotes.Text.Trim());
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    checkInId = (int)cmd.ExecuteScalar()!;
                }

                if (selectedAmenities.Count > 0)
                    InsertCheckInAmenities(conn, transaction, checkInId, selectedAmenities);

                UpdateRoomStatus(conn, transaction, roomId, "Occupied");
                transaction.Commit();

                try
                {
                    _logService.LogActivity(
                        CurrentUser.EmployeeId,
                        "CheckIn",
                        $"Walk-in guest {txtFirstName.Text} {txtLastName.Text} checked into Room {((dynamic)cboRoom.SelectedItem).RoomNumber}",
                        checkInId);
                }
                catch { }

                MessageBox.Show("Check-in successful!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearWalkInFields();
                LoadAvailableRooms();
            }
            catch (Exception exInner)
            {
                try { transaction?.Rollback(); } catch { }
                MessageBox.Show($"Error during check-in: {exInner.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW: Validation similar to ReservationControl (adaptable if ReservationControl uses other rules)
        private bool ValidateWalkInInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                return Fail("First Name is required.", txtFirstName);
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
                return Fail("Last Name is required.", txtLastName);
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
                return Fail("Phone is required.", txtPhone);
            if (cboIDType.SelectedItem == null)
                return Fail("ID Type is required.", cboIDType);
            if (string.IsNullOrWhiteSpace(txtIDNumber.Text))
                return Fail("ID Number is required.", txtIDNumber);
            if (cboRoom.SelectedItem == null)
                return Fail("Select a room.", cboRoom);
            if (numGuests.Value <= 0)
                return Fail("Guests must be at least 1.", numGuests);

            var phone = txtPhone.Text.Trim();
            if (!Regex.IsMatch(phone, @"^[\+\d\-\s]+$") || Regex.Matches(phone, @"\d").Count < 7 || phone.Length > 20)
                return Fail("Enter a valid phone number (7+ digits, allowed: digits + - space).", txtPhone);

            var email = txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                return Fail("Enter a valid email address.", txtEmail);

            if (txtIDNumber.Text.Trim().Length < 3)
                return Fail("ID Number must be at least 3 characters.", txtIDNumber);

            if (dtpCheckOut.Value.Date <= DateTime.Today)
                return Fail("Expected Check-Out must be after today.", dtpCheckOut);

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private bool Fail(string message, Control focusTarget)
        {
            MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focusTarget.Focus();
            return false;
        }

        #endregion

        #region Database Helpers

        private void InsertCheckInAmenities(SqlConnection conn, SqlTransaction tx, int checkInId,
            List<HotelMgt.otherUI.AmenitiesPanel.AmenitySelection> amenities)
        {
            const string sql = @"
INSERT INTO CheckInAmenities (CheckInID, AmenityID, Quantity, UnitPrice, CreatedAt)
VALUES (@CheckInID, @AmenityID, @Quantity, @UnitPrice, GETDATE());";

            foreach (var amenity in amenities)
            {
                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("@CheckInID", checkInId);
                cmd.Parameters.AddWithValue("@AmenityID", amenity.AmenityID);
                cmd.Parameters.AddWithValue("@Quantity", amenity.Quantity);
                cmd.Parameters.AddWithValue("@UnitPrice", amenity.Price);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateRoomStatus(SqlConnection conn, SqlTransaction transaction, int roomId, string status)
        {
            const string updateRoomQuery = "UPDATE Rooms SET Status = @Status WHERE RoomID = @RoomID";
            using var cmd = new SqlCommand(updateRoomQuery, conn, transaction);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@RoomID", roomId);
            cmd.ExecuteNonQuery();
        }

        private void ClearWalkInFields()
        {
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cboIDType.SelectedIndex = -1;
            txtIDNumber.Clear();
            cboRoom.SelectedIndex = -1;
            numGuests.Value = 1;
            dtpCheckOut.Value = DateTime.Today.AddDays(1);
            txtNotes.Clear();
        }

        #endregion
    }
}