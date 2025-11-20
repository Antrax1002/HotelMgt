using System;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using System.Drawing;
using System.Windows.Forms;
using HotelMgt.Services;
using HotelMgt.Utilities;

namespace HotelMgt.UserControls.Employee
{
    public partial class ReservationControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly ActivityLogService _logService;

        // Controls
        private TextBox txtFirstName = null!, txtLastName = null!, txtEmail = null!, txtPhone = null!;
        private DateTimePicker dtpCheckIn = null!, dtpCheckOut = null!;
        private ComboBox cboRoom = null!;
        private NumericUpDown numGuests = null!;
        private TextBox txtSpecialRequests = null!;
        private Label lblTotalAmount = null!;
        private Button btnCreateReservation = null!;

        // Lazy init flag
        private bool _initialized;

        public ReservationControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _logService = new ActivityLogService();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        // Ensure UI is built when the control is created and when parent adds it
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            EnsureInitialized();
        }

        // Ensure the UI builds deterministically
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnsureInitialized();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            EnsureInitialized();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) EnsureInitialized();
        }

        // Expose a public trigger the dashboard can call
        public void ForceInitialize() => EnsureInitialized();

        // Make sure this exists only once and matches this signature
        private void EnsureInitialized()
        {
            if (_initialized || DesignMode || !IsHandleCreated) return;
            try
            {
                InitializeControls();
                LoadAvailableRooms();
                CalculateTotalAmount();
                _initialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Reservation tab: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeControls()
        {
            Controls.Clear(); // ensure designer placeholders don’t overlap
            BackColor = Color.White;
            Dock = DockStyle.Fill;

            int y = 20;

            var lblTitle = new Label
            {
                Text = "Create Reservation",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            y += 40;
            var lblSubtitle = new Label
            {
                Text = "Enter guest info, pick dates and room, then save.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lblSubtitle);

            y += 30;
            var lblGuestInfo = new Label
            {
                Text = "Guest Information",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lblGuestInfo);

            y += 35;
            Controls.Add(new Label { Text = "First Name *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            txtFirstName = new TextBox { Location = new Point(20, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "John" };
            Controls.Add(txtFirstName);

            Controls.Add(new Label { Text = "Last Name *", Location = new Point(290, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            txtLastName = new TextBox { Location = new Point(290, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "Doe" };
            Controls.Add(txtLastName);

            y += 60;
            Controls.Add(new Label { Text = "Email", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            txtEmail = new TextBox { Location = new Point(20, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "john@example.com" };
            Controls.Add(txtEmail);

            Controls.Add(new Label { Text = "Phone Number *", Location = new Point(290, y), Size = new Size(140, 20), Font = new Font("Segoe UI", 9) });
            txtPhone = new TextBox { Location = new Point(290, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "+1 555 123 4567" };
            Controls.Add(txtPhone);

            y += 70;
            var lblStayInfo = new Label
            {
                Text = "Stay & Room",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lblStayInfo);

            y += 35;
            Controls.Add(new Label { Text = "Check-In *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            dtpCheckIn = new DateTimePicker
            {
                Location = new Point(20, y + 22),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };
            dtpCheckIn.ValueChanged += (s, e2) =>
            {
                dtpCheckOut.MinDate = dtpCheckIn.Value.AddDays(1);
                if (dtpCheckOut.Value <= dtpCheckIn.Value) dtpCheckOut.Value = dtpCheckIn.Value.AddDays(1);
                CalculateTotalAmount();
            };
            Controls.Add(dtpCheckIn);

            Controls.Add(new Label { Text = "Check-Out *", Location = new Point(290, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            dtpCheckOut = new DateTimePicker
            {
                Location = new Point(290, y + 22),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today.AddDays(1),
                Value = DateTime.Today.AddDays(1)
            };
            dtpCheckOut.ValueChanged += (s, e2) => CalculateTotalAmount();
            Controls.Add(dtpCheckOut);

            y += 60;
            Controls.Add(new Label { Text = "Room *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            cboRoom = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(520, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "DisplayText",
                ValueMember = "RoomId"
            };
            cboRoom.SelectedIndexChanged += (s, e2) => CalculateTotalAmount();
            Controls.Add(cboRoom);

            y += 60;
            Controls.Add(new Label { Text = "Guests *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            numGuests = new NumericUpDown
            {
                Location = new Point(20, y + 22),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };
            Controls.Add(numGuests);

            Controls.Add(new Label { Text = "Special Requests", Location = new Point(160, y), Size = new Size(140, 20), Font = new Font("Segoe UI", 9) });
            txtSpecialRequests = new TextBox
            {
                Location = new Point(160, y + 22),
                Size = new Size(380, 60),
                Font = new Font("Segoe UI", 10),
                Multiline = true
            };
            Controls.Add(txtSpecialRequests);

            y += 95;
            lblTotalAmount = new Label
            {
                Text = "Total: $0.00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            Controls.Add(lblTotalAmount);

            btnCreateReservation = new Button
            {
                Text = "Create Reservation",
                Location = new Point(20, y + 35),
                Size = new Size(520, 45),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCreateReservation.FlatAppearance.BorderSize = 0;
            btnCreateReservation.Click += BtnCreateReservation_Click;
            Controls.Add(btnCreateReservation);
        }

        private void LoadAvailableRooms()
        {
            try
            {
                cboRoom.Items.Clear();

                using var conn = _dbService.GetConnection();
                conn.Open();

                string query = @"
                    SELECT RoomId, RoomNumber, RoomType, PricePerNight, MaxOccupancy
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
                        DisplayText = $"Room {reader.GetString(1)} - {reader.GetString(2)} (${reader.GetDecimal(3):F2}/night) - Max {reader.GetInt32(4)} guests"
                    };
                    cboRoom.Items.Add(room);
                }

                if (cboRoom.Items.Count == 0)
                {
                    MessageBox.Show("No rooms available for the selected period.", "No Rooms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCreateReservation_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    // Find or create guest by phone
                    int guestId;
                    using (var find = new SqlCommand("SELECT TOP 1 GuestId FROM Guests WHERE PhoneNumber = @Phone", conn, tx))
                    {
                        find.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        var found = find.ExecuteScalar();
                        if (found is int id)
                        {
                            guestId = id;
                        }
                        else
                        {
                            using var insertGuest = new SqlCommand(@"
                                INSERT INTO Guests (FirstName, LastName, Email, PhoneNumber, IDType, IDNumber, CreatedAt)
                                VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @IDType, @IDNumber, @CreatedAt);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx);
                            insertGuest.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                            insertGuest.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                            insertGuest.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                            insertGuest.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                            insertGuest.Parameters.AddWithValue("@IDType", (object)DBNull.Value);
                            insertGuest.Parameters.AddWithValue("@IDNumber", (object)DBNull.Value);
                            insertGuest.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            guestId = (int)insertGuest.ExecuteScalar();
                        }
                    }

                    dynamic room = cboRoom.SelectedItem!;
                    int nights = (dtpCheckOut.Value.Date - dtpCheckIn.Value.Date).Days;
                    decimal total = nights * (decimal)room.PricePerNight;

                    using var insertReservation = new SqlCommand(@"
                        INSERT INTO Reservations
                            (GuestId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, TotalAmount, SpecialRequests, Status, CreatedAt)
                        VALUES
                            (@GuestId, @RoomId, @CheckInDate, @CheckOutDate, @Guests, @Total, @Requests, @Status, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx);

                    insertReservation.Parameters.AddWithValue("@GuestId", guestId);
                    insertReservation.Parameters.AddWithValue("@RoomId", (int)room.RoomId);
                    insertReservation.Parameters.AddWithValue("@CheckInDate", dtpCheckIn.Value.Date);
                    insertReservation.Parameters.AddWithValue("@CheckOutDate", dtpCheckOut.Value.Date);
                    insertReservation.Parameters.AddWithValue("@Guests", (int)numGuests.Value);
                    insertReservation.Parameters.AddWithValue("@Total", total);
                    insertReservation.Parameters.AddWithValue("@Requests",
                        string.IsNullOrWhiteSpace(txtSpecialRequests.Text) ? (object)DBNull.Value : txtSpecialRequests.Text.Trim());
                    insertReservation.Parameters.AddWithValue("@Status", "Confirmed");
                    insertReservation.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    int reservationId = (int)insertReservation.ExecuteScalar();

                    _logService.LogActivity(
                        CurrentUser.EmployeeId,
                        "Reservation",
                        $"Reservation #{reservationId} created for {txtFirstName.Text} {txtLastName.Text} in Room {room.RoomNumber} ({nights} nights, {total:C2})",
                        reservationId
                    );

                    tx.Commit();

                    MessageBox.Show($"Reservation created successfully! ID: {reservationId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearForm();
                    LoadAvailableRooms();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating reservation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateTotalAmount()
        {
            if (cboRoom.SelectedItem != null && dtpCheckIn.Value.Date < dtpCheckOut.Value.Date)
            {
                dynamic room = cboRoom.SelectedItem;
                int nights = (dtpCheckOut.Value.Date - dtpCheckIn.Value.Date).Days;
                decimal total = nights * (decimal)room.PricePerNight;
                lblTotalAmount.Text = $"Total: {total:C2} ({nights} nights)";
            }
            else
            {
                lblTotalAmount.Text = "Total: $0.00";
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("First name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus(); return false;
            }
            if (dtpCheckIn.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Check-in cannot be in the past.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpCheckIn.Focus(); return false;
            }
            if (dtpCheckOut.Value.Date <= dtpCheckIn.Value.Date)
            {
                MessageBox.Show("Check-out must be after check-in.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpCheckOut.Focus(); return false;
            }
            if (cboRoom.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a room.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboRoom.Focus(); return false;
            }
            dynamic room = cboRoom.SelectedItem!;
            if ((int)numGuests.Value > (int)room.MaxOccupancy)
            {
                MessageBox.Show($"Selected room allows up to {room.MaxOccupancy} guests.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numGuests.Focus(); return false;
            }
            return true;
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            dtpCheckIn.Value = DateTime.Today;
            dtpCheckOut.Value = DateTime.Today.AddDays(1);
            cboRoom.SelectedIndex = -1;
            numGuests.Value = 1;
            txtSpecialRequests.Clear();
            lblTotalAmount.Text = "Total: $0.00";
        }
    }
}