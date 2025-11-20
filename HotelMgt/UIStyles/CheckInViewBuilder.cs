using System;
using System.Drawing;
using System.Windows.Forms;
using HotelMgt.otherUI; // Add this for AmenitiesPanel
using HotelMgt.Custom;  // <-- already present for RoundedButton

namespace HotelMgt.UIStyles
{
    public static class CheckInViewBuilder
    {
        public static void Build(
            Control parent,
            out TabControl tabRoot,
            // Walk-in
            out TextBox txtFirstName, out TextBox txtMiddleName, out TextBox txtLastName, out TextBox txtEmail, out TextBox txtPhone,
            out ComboBox cboIDType, out TextBox txtIDNumber, out ComboBox cboRoom, out NumericUpDown numGuests, out DateTimePicker dtpCheckOut,
            out TextBox txtNotes, out Button btnCheckIn, out Label lblNoAvailableRooms,
            // Reservation
            out ComboBox cboReservation, out Panel panelReservationDetails,
            out Label lblResGuestName, out Label lblResRoomNumber, out Label lblResCheckIn, out Label lblResCheckOut,
            out Label lblResGuests, out Label lblResTotal, out Label lblResSpecialRequests, out Button btnCheckInReservation,
            out TextBox txtReservationCodeLookup, out Button btnLookupReservation,
            out Label lblNoPendingReservations,
            // Amenities
            out AmenitiesPanel amenitiesPanel
        )
        {
            parent.SuspendLayout();
            parent.Controls.Clear();
            parent.BackColor = Color.White;
            parent.Dock = DockStyle.Fill;

            var mainSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0),
            };
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // --- LEFT: Details panel (Tabs) ---
            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            Color accent = Color.FromArgb(37, 99, 235);
            Color accentDanger = Color.FromArgb(220, 38, 38);
            Color muted = Color.Gray;
            var fontRegular = "Segoe UI";

            tabRoot = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font(fontRegular, 10f),
                Padding = new Point(12, 6)
            };

            // --- Make Walk-In Tab scrollable ---
            var tabWalkIn = new TabPage("Walk-In") { BackColor = Color.White };
            var walkInScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            tabWalkIn.Controls.Add(walkInScrollPanel);

            // --- Make Reservation Tab scrollable ---
            var tabReservation = new TabPage("With Reservation") { BackColor = Color.White };
            var reservationScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            tabReservation.Controls.Add(reservationScrollPanel);

            tabRoot.TabPages.Add(tabWalkIn);
            tabRoot.TabPages.Add(tabReservation);
            detailsPanel.Controls.Add(tabRoot);

            // ------------------- WALK-IN TAB -------------------
            int y = 20;

            var lblTitleWalk = new Label
            {
                Text = "Guest Walk-In Check-In",
                Font = new Font(fontRegular, 18, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            walkInScrollPanel.Controls.Add(lblTitleWalk);

            y += 40;
            var lblSubtitleWalk = new Label
            {
                Text = "Enter guest details, select a room and expected check-out date, then complete check-in.",
                Font = new Font(fontRegular, 10, FontStyle.Regular),
                ForeColor = muted,
                Location = new Point(20, y),
                AutoSize = true
            };
            walkInScrollPanel.Controls.Add(lblSubtitleWalk);

            y += 34;
            var lblSectionGuest = new Label
            {
                Text = "Guest Information",
                Font = new Font(fontRegular, 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            walkInScrollPanel.Controls.Add(lblSectionGuest);

            y += 35;
            walkInScrollPanel.Controls.Add(MakeLabel("First Name *", 20, y));
            txtFirstName = MakeTextBox(20, y + 22, 120, "John");
            walkInScrollPanel.Controls.Add(txtFirstName);

            walkInScrollPanel.Controls.Add(MakeLabel("Middle Name", 150, y));
            txtMiddleName = MakeTextBox(150, y + 22, 120, "A.");
            walkInScrollPanel.Controls.Add(txtMiddleName);

            walkInScrollPanel.Controls.Add(MakeLabel("Last Name *", 290, y));
            txtLastName = MakeTextBox(290, y + 22, 180, "Doe");
            walkInScrollPanel.Controls.Add(txtLastName);

            y += 60;
            walkInScrollPanel.Controls.Add(MakeLabel("Email", 20, y));
            txtEmail = MakeTextBox(20, y + 22, 250, "john@example.com");
            walkInScrollPanel.Controls.Add(txtEmail);

            walkInScrollPanel.Controls.Add(MakeLabel("Phone *", 290, y));
            txtPhone = MakeTextBox(290, y + 22, 180, "9xxxxxxxxx");
            walkInScrollPanel.Controls.Add(txtPhone);

            y += 60;
            walkInScrollPanel.Controls.Add(MakeLabel("ID Type *", 20, y));
            cboIDType = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(180, 25),
                Font = new Font(fontRegular, 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboIDType.Items.AddRange(new object[] { "Passport", "Driver License", "National ID", "Others" });
            walkInScrollPanel.Controls.Add(cboIDType);

            walkInScrollPanel.Controls.Add(MakeLabel("ID Number *", 220, y));
            txtIDNumber = MakeTextBox(220, y + 22, 180);
            walkInScrollPanel.Controls.Add(txtIDNumber);

            y += 60;
            var lblStay = new Label
            {
                Text = "Stay & Room",
                Font = new Font(fontRegular, 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            walkInScrollPanel.Controls.Add(lblStay);

            y += 35;
            walkInScrollPanel.Controls.Add(MakeLabel("Room *", 20, y));
            cboRoom = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(520, 25),
                Font = new Font(fontRegular, 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "DisplayText",
                ValueMember = "RoomId"
            };
            walkInScrollPanel.Controls.Add(cboRoom);

            y += 60;
            walkInScrollPanel.Controls.Add(MakeLabel("Guests *", 20, y));
            numGuests = new NumericUpDown
            {
                Location = new Point(20, y + 22),
                Size = new Size(120, 25),
                Font = new Font(fontRegular, 10),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };
            walkInScrollPanel.Controls.Add(numGuests);

            walkInScrollPanel.Controls.Add(MakeLabel("Check-Out *", 160, y));
            dtpCheckOut = new DateTimePicker
            {
                Location = new Point(160, y + 22),
                Size = new Size(180, 25),
                Font = new Font(fontRegular, 10),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today.AddDays(1),
                Value = DateTime.Today.AddDays(1)
            };
            walkInScrollPanel.Controls.Add(dtpCheckOut);

            walkInScrollPanel.Controls.Add(MakeLabel("Notes", 360, y));
            txtNotes = new TextBox
            {
                Location = new Point(360, y + 22),
                Size = new Size(180, 60),
                Font = new Font(fontRegular, 10),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            walkInScrollPanel.Controls.Add(txtNotes);

            y += 95;
            // --- Walk-In Check-In Button ---
            btnCheckIn = new RoundedButton
            {
                Text = "Check-In (Walk-In)",
                Location = new Point(20, y),
                Size = new Size(520, 45),
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font(fontRegular, 11, FontStyle.Bold),
                BorderRadius = 10,
                BorderSize = 0,
                FlatStyle = FlatStyle.Flat
            };
            ((RoundedButton)btnCheckIn).FlatAppearance.BorderSize = 0;
            walkInScrollPanel.Controls.Add(btnCheckIn);

            lblNoAvailableRooms = new Label
            {
                Text = "No available rooms.",
                Font = new Font(fontRegular, 9, FontStyle.Italic),
                ForeColor = muted,
                Location = new Point(20, y + 50),
                AutoSize = true,
                Visible = false
            };
            walkInScrollPanel.Controls.Add(lblNoAvailableRooms);

            // ------------------- RESERVATION TAB -------------------
            int ry = 20;

            var lblTitleRes = new Label
            {
                Text = "Reservation Check-In",
                Font = new Font(fontRegular, 18, FontStyle.Bold),
                Location = new Point(20, ry),
                AutoSize = true
            };
            reservationScrollPanel.Controls.Add(lblTitleRes);

            ry += 40;
            var lblSubtitleRes = new Label
            {
                Text = "Enter a confirmed reservation code or pick a pending reservation, then complete or cancel.",
                Font = new Font(fontRegular, 10),
                ForeColor = muted,
                Location = new Point(20, ry),
                AutoSize = true
            };
            reservationScrollPanel.Controls.Add(lblSubtitleRes);

            ry += 34;
            var lblLookup = new Label
            {
                Text = "Enter reservation code",
                Font = new Font(fontRegular, 12, FontStyle.Bold),
                Location = new Point(20, ry),
                AutoSize = true
            };
            reservationScrollPanel.Controls.Add(lblLookup);

            ry += 35;

            cboReservation = new ComboBox
            {
                Location = new Point(20, ry),
                Size = new Size(320, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false,
                Enabled = false
            };
            reservationScrollPanel.Controls.Add(cboReservation);

            txtReservationCodeLookup = new TextBox
            {
                Location = new Point(20, ry),
                Size = new Size(320, 25),
                Font = new Font(fontRegular, 10),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Reservation code"
            };
            reservationScrollPanel.Controls.Add(txtReservationCodeLookup);

            // --- Reservation Lookup Button ---
            btnLookupReservation = new RoundedButton
            {
                Text = "Verify",
                Location = new Point(350, ry - 1),
                Size = new Size(90, 28),
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font(fontRegular, 9, FontStyle.Bold),
                BorderRadius = 8,
                BorderSize = 0,
                FlatStyle = FlatStyle.Flat
            };
            ((RoundedButton)btnLookupReservation).FlatAppearance.BorderSize = 0;
            reservationScrollPanel.Controls.Add(btnLookupReservation);

            ry += 46;
            lblNoPendingReservations = new Label
            {
                Text = "Enter a reservation code to view details.",
                Font = new Font(fontRegular, 9, FontStyle.Italic),
                ForeColor = muted,
                Location = new Point(20, ry),
                AutoSize = true
            };
            reservationScrollPanel.Controls.Add(lblNoPendingReservations);

            ry += 26;
            panelReservationDetails = new Panel
            {
                Location = new Point(20, ry),
                Size = new Size(420, 190),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                BackColor = Color.White
            };
            reservationScrollPanel.Controls.Add(panelReservationDetails);

            int dY = 10;
            lblResGuestName = MakeDetail(panelReservationDetails, ref dY, "Guest:");
            lblResRoomNumber = MakeDetail(panelReservationDetails, ref dY, "Room:");
            lblResCheckIn = MakeDetail(panelReservationDetails, ref dY, "Check-In:");
            lblResCheckOut = MakeDetail(panelReservationDetails, ref dY, "Check-Out:");
            lblResGuests = MakeDetail(panelReservationDetails, ref dY, "Guests:");
            lblResTotal = MakeDetail(panelReservationDetails, ref dY, "Total:");
            lblResSpecialRequests = MakeDetail(panelReservationDetails, ref dY, "Special: None");

            ry += panelReservationDetails.Height + 12;

            // --- Complete Check-In Button ---
            btnCheckInReservation = new RoundedButton
            {
                Text = "Complete Check-In",
                Location = new Point(20, ry),
                Size = new Size(205, 42),
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font(fontRegular, 10, FontStyle.Bold),
                BorderRadius = 8,
                BorderSize = 0,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            ((RoundedButton)btnCheckInReservation).FlatAppearance.BorderSize = 0;
            reservationScrollPanel.Controls.Add(btnCheckInReservation);

            // --- Cancel Reservation Button ---
            var btnCancelReservation = new RoundedButton
            {
                Text = "Cancel Reservation",
                Name = "btnCancelReservation",
                Location = new Point(235, ry),
                Size = new Size(205, 42),
                BackColor = accentDanger,
                ForeColor = Color.White,
                Font = new Font(fontRegular, 10, FontStyle.Bold),
                BorderRadius = 8,
                BorderSize = 0,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            ((RoundedButton)btnCancelReservation).FlatAppearance.BorderSize = 0;
            reservationScrollPanel.Controls.Add(btnCancelReservation);

            // --- RIGHT: AmenitiesPanel ---
            amenitiesPanel = new AmenitiesPanel
            {
                Name = "amenitiesPanel",
                Dock = DockStyle.Top,
                Margin = new Padding(24, 32, 24, 0),
                Height = 400
            };

            mainSplit.Controls.Add(detailsPanel, 0, 0);
            mainSplit.Controls.Add(amenitiesPanel, 1, 0);

            parent.Controls.Add(mainSplit);

            parent.ResumeLayout(false);
            parent.PerformLayout();
        }

        // ---------------- Helper factories ----------------
        private static Label MakeLabel(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };

        private static TextBox MakeTextBox(int x, int y, int width, string placeholder = "")
        {
            var tb = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
#if NET6_0_OR_GREATER
            if (!string.IsNullOrWhiteSpace(placeholder))
                tb.PlaceholderText = placeholder;
#endif
            return tb;
        }

        private static Label MakeDetail(Panel host, ref int y, string text)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(10, y),
                AutoSize = true
            };
            host.Controls.Add(lbl);
            y += 24;
            return lbl;
        }
    }
}