using HotelMgt.Documents;
using HotelMgt.Forms;
using HotelMgt.Models;
using HotelMgt.otherUI; // At the top if not already
using HotelMgt.Services;
using HotelMgt.UIStyles;
using HotelMgt.Utilities;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using System;
using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Windows.Forms;
using HotelMgt.Core.Events;

namespace HotelMgt.UserControls.Employee
{
    public partial class ReservationControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly ActivityLogService _logService;
        private readonly EmailService _emailService;
        private readonly GuestService _guestService = new GuestService(); // Add this field if not present

        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        // Controls (assigned by builder)
        private TextBox txtFirstName = null!, txtMiddleName = null!, txtLastName = null!, txtEmail = null!, txtPhone = null!;
        private ComboBox cboIDType = null!;
        private TextBox txtIDNumber = null!;
        private DateTimePicker dtpCheckIn = null!, dtpCheckOut = null!;
        private ComboBox cboRoom = null!;
        private ComboBox cboAmenities = null!;
        private NumericUpDown numGuests = null!;
        private TextBox txtSpecialRequests = null!;
        private Label lblTotalAmount = null!;
        private Button btnCreateReservation = null!; // "Generate Code"
        private Label lblDownpayment = null!;
        private ComboBox cboPaymentMethod = null!;
        private TextBox txtTransactionRef = null!;

        private AmenitiesPanel amenitiesPanel = null!; // Add this field to your ReservationControl

        // To avoid recursion while sanitizing phone text
        private bool _suppressPhoneTextChanged;

        public ReservationControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _logService = new ActivityLogService();
            _emailService = new EmailService(EmailSettings.LoadFromEnvironment());
            this.Load += ReservationControl_Load;
        }

        private void ReservationControl_Load(object? sender, EventArgs e)
        {
            InitializeControls();
            LoadAvailableRooms();
            CalculateTotalAmount();
        }

        private void InitializeControls()
        {
            ReservationViewBuilder.Build(
                this,
                _currencyCulture,
                out txtFirstName, out txtMiddleName, out txtLastName, out txtEmail, out txtPhone,
                out cboIDType, out txtIDNumber,
                out dtpCheckIn, out dtpCheckOut,
                out cboRoom,
                out var _cboAmenities, // ignore, not used
                out numGuests,
                out txtSpecialRequests,
                out lblDownpayment,
                out lblTotalAmount,
                out cboPaymentMethod,
                out txtTransactionRef,
                out btnCreateReservation,
                out amenitiesPanel // <-- Use the out parameter here
            );

            // No need to search for amenitiesPanel by name anymore!

            // Subscribe to amenities selection changes
            amenitiesPanel.SelectionChanged += (s, e) => CalculateTotalAmount();

            AttachPhoneNumberValidation();

            dtpCheckIn.ValueChanged += (s, e2) =>
            {
                dtpCheckOut.MinDate = dtpCheckIn.Value.AddDays(1);
                if (dtpCheckOut.Value <= dtpCheckIn.Value)
                    dtpCheckOut.Value = dtpCheckIn.Value.AddDays(1);
                CalculateTotalAmount();
            };
            dtpCheckOut.ValueChanged += (s, e2) => CalculateTotalAmount();
            cboRoom.SelectedIndexChanged += (s, e2) => CalculateTotalAmount();
            btnCreateReservation.Click += BtnGenerateCode_Click;

            cboPaymentMethod.SelectedIndexChanged += (s, e) =>
            {
                bool isCash = cboPaymentMethod.SelectedItem?.ToString() == "Cash";
                txtTransactionRef.Enabled = !isCash;
                if (isCash) txtTransactionRef.Clear();
            };
            // Set initial state
            bool isCashInit = cboPaymentMethod.SelectedItem?.ToString() == "Cash";
            txtTransactionRef.Enabled = !isCashInit;
            if (isCashInit) txtTransactionRef.Clear();
        }

        private void AttachPhoneNumberValidation()
        {
            // Block non-digits (allow control keys)
            txtPhone.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            // Sanitize pasted text to digits only
            txtPhone.TextChanged += (s, e) =>
            {
                if (_suppressPhoneTextChanged) return;
                var original = txtPhone.Text;
                if (string.IsNullOrEmpty(original)) return;

                Span<char> buffer = stackalloc char[original.Length];
                int idx = 0;
                foreach (var ch in original)
                {
                    if (char.IsDigit(ch)) buffer[idx++] = ch;
                }

                if (idx != original.Length)
                {
                    _suppressPhoneTextChanged = true;
                    var caret = txtPhone.SelectionStart;
                    txtPhone.Text = new string(buffer[..idx]);
                    txtPhone.SelectionStart = Math.Min(caret - (original.Length - idx), txtPhone.Text.Length);
                    _suppressPhoneTextChanged = false;
                }
            };

            txtPhone.MaxLength = 15;
        }

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
                    var price = reader.GetDecimal(3).ToString("C2", _currencyCulture);
                    var room = new
                    {
                        RoomId = reader.GetInt32(0),
                        RoomNumber = reader.GetString(1),
                        RoomType = reader.GetString(2),
                        PricePerNight = reader.GetDecimal(3),
                        MaxOccupancy = reader.GetInt32(4),
                        DisplayText = $"Room {reader.GetString(1)} - {reader.GetString(2)} ({price}/night) - Max {reader.GetInt32(4)} guests"
                    };
                    cboRoom.Items.Add(room);
                }

                if (cboRoom.Items.Count == 0)
                {
                    MessageBox.Show("No rooms available.", "No Rooms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerateCode_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            int reservationId = 0;
            string reservationCode = "";
            string guestFullName = $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}";
            string roomNumber = "";
            int nights = 0;
            decimal total = 0m;
            decimal pricePerNight = 0m;

            dynamic room = cboRoom.SelectedItem!;

            // Declare selectedAmenities ONCE here
            var selectedAmenities = amenitiesPanel.GetSelectedAmenities();

            try
            {
                using var conn = _dbService.GetConnection();
                await conn.OpenAsync();
                using var tx = conn.BeginTransaction();

                try
                {
                    // --- FIND OR CREATE GUEST (robust, safe for DataReader) ---
                    string firstName = txtFirstName.Text.Trim();
                    string middleName = txtMiddleName.Text.Trim();
                    string lastName = txtLastName.Text.Trim();
                    string phone = txtPhone.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    string idType = cboIDType.SelectedItem?.ToString() ?? "Unknown";
                    string idNumber = string.IsNullOrWhiteSpace(txtIDNumber.Text) ? "N/A" : txtIDNumber.Text.Trim();

                    var lookup = GuestLookupHelper.LookupOrPromptGuest(
                        this, conn, tx,
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
                            conn, tx,
                            firstName, middleName, lastName, phone, email, idType, idNumber
                        );
                    }

                    // --- ROOM & TOTAL ---
                    roomNumber = room.RoomNumber;
                    nights = (dtpCheckOut.Value.Date - dtpCheckIn.Value.Date).Days;
                    pricePerNight = (decimal)room.PricePerNight;
                    decimal downpayment = pricePerNight; // always 1 night

                    // Calculate amenities total
                    decimal amenitiesTotal = selectedAmenities.Sum(a => a.Price * a.Quantity);

                    total = (nights * pricePerNight) + amenitiesTotal - downpayment;
                    if (total < 0) total = 0;

                    // --- INSERT RESERVATION ---
                    using var insertReservation = new SqlCommand(@"
                    DECLARE @output TABLE (ReservationID INT, ReservationCode NVARCHAR(50));
                    INSERT INTO Reservations
                        (GuestID, RoomID, EmployeeID, CheckInDate, CheckOutDate, NumberOfGuests, TotalAmount, Downpayment, SpecialRequests, ReservationStatus, CreatedAt)
                    OUTPUT Inserted.ReservationID, Inserted.ReservationCode INTO @output
                    VALUES
                        (@GuestId, @RoomId, @EmployeeId, @CheckInDate, @CheckOutDate, @Guests, @Total, @Downpayment, @Requests, @Status, @CreatedAt);
                    SELECT ReservationID, ReservationCode FROM @output;", conn, tx);

                    insertReservation.Parameters.AddWithValue("@GuestId", guestId);
                    insertReservation.Parameters.AddWithValue("@RoomId", (int)room.RoomId);
                    insertReservation.Parameters.AddWithValue("@EmployeeId", CurrentUser.EmployeeId);
                    insertReservation.Parameters.AddWithValue("@CheckInDate", dtpCheckIn.Value.Date);
                    insertReservation.Parameters.AddWithValue("@CheckOutDate", dtpCheckOut.Value.Date);
                    insertReservation.Parameters.AddWithValue("@Guests", (int)numGuests.Value);
                    insertReservation.Parameters.AddWithValue("@Total", total);
                    insertReservation.Parameters.AddWithValue("@Downpayment", downpayment);
                    insertReservation.Parameters.AddWithValue("@Requests",
                        string.IsNullOrWhiteSpace(txtSpecialRequests.Text) ? (object)DBNull.Value : txtSpecialRequests.Text.Trim());
                    insertReservation.Parameters.AddWithValue("@Status", "Confirmed");
                    insertReservation.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    using (var reader = await insertReservation.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            reservationId = reader.GetInt32(0);
                            reservationCode = reader.GetString(1);
                        }
                    }

                    // --- INSERT RESERVATION AMENITIES ---
                    if (selectedAmenities.Count > 0)
                    {
                        const string sql = @"
    INSERT INTO ReservationAmenities (ReservationID, AmenityID, Quantity, UnitPrice, CreatedAt)
    VALUES (@ReservationID, @AmenityID, @Quantity, @UnitPrice, GETDATE());";

                        foreach (var amenity in selectedAmenities)
                        {
                            using var cmd = new SqlCommand(sql, conn, tx);
                            cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                            cmd.Parameters.AddWithValue("@AmenityID", amenity.AmenityID);
                            cmd.Parameters.AddWithValue("@Quantity", amenity.Quantity);
                            cmd.Parameters.AddWithValue("@UnitPrice", amenity.Price);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // --- UPDATE ROOM STATUS TO RESERVED ---
                    using (var updateRoomCmd = new SqlCommand("UPDATE Rooms SET Status = 'Reserved' WHERE RoomID = @RoomID", conn, tx))
                    {
                        updateRoomCmd.Parameters.AddWithValue("@RoomID", (int)room.RoomId);
                        updateRoomCmd.ExecuteNonQuery();
                    }

                    await tx.CommitAsync();

                    // Notify listeners (like AvailableRoomsControl) to refresh
                    RoomEvents.Publish(RoomChangeType.Updated, (int)room.RoomId);
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }

                // --- LOG ACTIVITY ---
                try
                {
                    _logService.LogActivity(
                        CurrentUser.EmployeeId,
                        "Reservation",
                        $"Reservation #{reservationId} created for {guestFullName} in Room {roomNumber} ({nights} nights, {total.ToString("C2", _currencyCulture)}), Code: {reservationCode}",
                        reservationId
                    );
                }
                catch { }

                // --- AMENITIES ---
                var amenities = selectedAmenities.Count == 0
                    ? new List<string> { "None" }
                    : selectedAmenities.Select(a => $"{a.Name} x{a.Quantity} ({a.Price.ToString("C2", _currencyCulture)})").ToList();

                // --- RECEIPT ---
                var receipt = new ReservationReceipt
                {
                    ReservationCode = reservationCode,
                    GuestFullName = guestFullName,
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    NumberOfGuests = (int)numGuests.Value,
                    CheckIn = dtpCheckIn.Value.Date,
                    CheckOut = dtpCheckOut.Value.Date,
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomType,
                    PricePerNight = pricePerNight,
                    Nights = nights,
                    TotalAmount = total,
                    Inclusions = amenities,
                    BankAccounts = new List<(string Title, string Body)>
            {
                ("BDO (Peso)", "Account Name: Golden Courtyard Hotel; Account No.: 123-456-7890"),
                ("GCash", "0917-123-4567")
            }
                };

                using var form = new ReservationCodeForm(
                    _emailService,
                    _logService,
                    _currencyCulture,
                    CurrentUser.EmployeeId,
                    reservationId,
                    reservationCode,
                    guestFullName,
                    txtEmail.Text.Trim(),
                    roomNumber,
                    dtpCheckIn.Value.Date,
                    dtpCheckOut.Value.Date,
                    total,
                    receipt);

                form.ShowDialog(this);

                ClearForm();
                LoadAvailableRooms();
                CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating code:\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //private int CreateGuest(SqlConnection conn, SqlTransaction tx)
        //{
        //    using var insertGuest = new SqlCommand(@"
        //        INSERT INTO Guests (FirstName, LastName, Email, PhoneNumber, IDType, IDNumber, CreatedAt)
        //        VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @IDType, @IDNumber, @CreatedAt);
        //        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx);

        //    insertGuest.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
        //    insertGuest.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
        //    // Email is required; always supply trimmed value
        //    insertGuest.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
        //    insertGuest.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
        //    insertGuest.Parameters.AddWithValue("@IDType", cboIDType.SelectedItem?.ToString() ?? "Unknown");
        //    insertGuest.Parameters.AddWithValue("@IDNumber", string.IsNullOrWhiteSpace(txtIDNumber.Text) ? "N/A" : txtIDNumber.Text.Trim());
        //    insertGuest.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        //    return (int)insertGuest.ExecuteScalar();
        //}

        private int CreateGuest(SqlConnection conn, SqlTransaction tx)
        {
            // Check for existing guest by phone and ID number
            using (var find = new SqlCommand(@"
        SELECT TOP 1 GuestID FROM Guests
        WHERE PhoneNumber = @PhoneNumber AND IDNumber = @IDNumber;", conn, tx))
            {
                find.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                find.Parameters.AddWithValue("@IDNumber", string.IsNullOrWhiteSpace(txtIDNumber.Text) ? "N/A" : txtIDNumber.Text.Trim());
                var existing = find.ExecuteScalar();
                if (existing is int id) return id;
            }

            // Insert new guest if not found
            using var insertGuest = new SqlCommand(@"
        INSERT INTO Guests (FirstName, LastName, Email, PhoneNumber, IDType, IDNumber, CreatedAt)
        VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @IDType, @IDNumber, @CreatedAt);
        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx);

            insertGuest.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
            insertGuest.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
            insertGuest.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
            insertGuest.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
            insertGuest.Parameters.AddWithValue("@IDType", cboIDType.SelectedItem?.ToString() ?? "Unknown");
            insertGuest.Parameters.AddWithValue("@IDNumber", string.IsNullOrWhiteSpace(txtIDNumber.Text) ? "N/A" : txtIDNumber.Text.Trim());
            insertGuest.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            return (int)insertGuest.ExecuteScalar();
        }

        private void CalculateTotalAmount()
        {
            if (cboRoom.SelectedItem != null && dtpCheckIn.Value.Date < dtpCheckOut.Value.Date)
            {
                dynamic room = cboRoom.SelectedItem;
                int nights = (dtpCheckOut.Value.Date - dtpCheckIn.Value.Date).Days;
                decimal pricePerNight = (decimal)room.PricePerNight;
                decimal downpayment = pricePerNight;

                // Calculate amenities total
                var selectedAmenities = amenitiesPanel.GetSelectedAmenities();
                decimal amenitiesTotal = selectedAmenities.Sum(a => a.Price * a.Quantity);

                decimal total = (nights * pricePerNight) + amenitiesTotal - downpayment;
                if (total < 0) total = 0;

                lblTotalAmount.Text = $"Total: {total.ToString("C2", _currencyCulture)} (after downpayment, {nights} nights, amenities: {amenitiesTotal.ToString("C2", _currencyCulture)})";
                lblDownpayment.Text = $"Downpayment: {downpayment.ToString("C2", _currencyCulture)}";
            }
            else
            {
                lblTotalAmount.Text = $"Total: {0m.ToString("C2", _currencyCulture)}";
                lblDownpayment.Text = $"Downpayment: {0m.ToString("C2", _currencyCulture)}";
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text)) { MessageBox.Show("First name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFirstName.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtLastName.Text)) { MessageBox.Show("Last name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLastName.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Email is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtEmail.Focus(); return false; }
            if (!IsValidEmail(txtEmail.Text.Trim())) { MessageBox.Show("Invalid email format.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtEmail.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtPhone.Text)) { MessageBox.Show("Phone number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPhone.Focus(); return false; }
            if (cboIDType.SelectedIndex == -1) { MessageBox.Show("ID Type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboIDType.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtIDNumber.Text)) { MessageBox.Show("ID Number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtIDNumber.Focus(); return false; }
            if (dtpCheckIn.Value.Date < DateTime.Today) { MessageBox.Show("Check-in cannot be in the past.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); dtpCheckIn.Focus(); return false; }
            if (dtpCheckOut.Value.Date <= dtpCheckIn.Value.Date) { MessageBox.Show("Check-out must be after check-in.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); dtpCheckOut.Focus(); return false; }
            if (cboRoom.SelectedIndex == -1) { MessageBox.Show("Please select a room.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboRoom.Focus(); return false; }

            dynamic room = cboRoom.SelectedItem!;
            if ((int)numGuests.Value > (int)room.MaxOccupancy)
            {
                MessageBox.Show($"Selected room allows up to {room.MaxOccupancy} guests.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numGuests.Focus();
                return false;
            }
            // After guest count check in ValidateInputs()
            if (cboPaymentMethod.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a payment method.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPaymentMethod.Focus();
                return false;
            }
            if (cboPaymentMethod.SelectedItem?.ToString() != "Cash" && string.IsNullOrWhiteSpace(txtTransactionRef.Text))
            {
                MessageBox.Show("Transaction Reference is required for non-cash payments.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTransactionRef.Focus();
                return false;
            }
            return true;
        }

        private static bool IsValidEmail(string email)
        {
            if (!email.Contains('@')) return false;
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cboIDType.SelectedIndex = -1;
            txtIDNumber.Clear();
            dtpCheckIn.Value = DateTime.Today;
            dtpCheckOut.Value = DateTime.Today.AddDays(1);
            cboRoom.SelectedIndex = -1;
            numGuests.Value = 1;
            txtSpecialRequests.Clear();
            lblTotalAmount.Text = $"Total: {0m.ToString("C2", _currencyCulture)}";
        }

        private void LoadAmenities()
        {
            cboAmenities.Items.Clear();

            // Add the "None - PHP0" option first
            cboAmenities.Items.Add(new
            {
                AmenityID = -1,
                DisplayText = "None"
            });

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand(@"
            SELECT AmenityID, Name, Price
            FROM Amenities
            WHERE IsActive = 1
            ORDER BY Name", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    decimal price = reader.GetDecimal(2);
                    cboAmenities.Items.Add(new
                    {
                        AmenityID = id,
                        DisplayText = $"{name} - {price.ToString("C2", _currencyCulture)}"
                    });
                }
                cboAmenities.DisplayMember = "DisplayText";
                cboAmenities.ValueMember = "AmenityID";
                // Always select "None" by default
                cboAmenities.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading amenities: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}