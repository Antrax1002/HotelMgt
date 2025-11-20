using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelMgt.otherUI;
using HotelMgt.Custom; // Add this for RoundedButton

namespace HotelMgt.UIStyles
{
    public static class ReservationViewBuilder
    {
        public static void Build(
            Control parent,
            CultureInfo currencyCulture,
            out TextBox txtFirstName, out TextBox txtMiddleName, out TextBox txtLastName, out TextBox txtEmail, out TextBox txtPhone,
            out ComboBox cboIDType, out TextBox txtIDNumber,
            out DateTimePicker dtpCheckIn, out DateTimePicker dtpCheckOut,
            out ComboBox cboRoom,
            out ComboBox cboAmenities, // For signature only, will be set to null
            out NumericUpDown numGuests,
            out TextBox txtSpecialRequests,
            out Label lblDownpayment,
            out Label lblTotalAmount,
            out ComboBox cboPaymentMethod,
            out TextBox txtTransactionRef,
            out Button btnCreateReservation,
            out AmenitiesPanel amenitiesPanel // <-- Add this line
        )
        {
            parent.SuspendLayout();
            parent.Controls.Clear();
            parent.BackColor = Color.White;
            parent.Dock = DockStyle.Fill;

            // --- Main horizontal split ---
            var mainSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0),
            };
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f)); // Details
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f)); // Amenities

            // --- LEFT: Details panel ---
            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            int y = 20;

            var lblTitle = new Label
            {
                Text = "Create Reservation",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            detailsPanel.Controls.Add(lblTitle);

            y += 40;
            var lblSubtitle = new Label
            {
                Text = "Enter guest info, pick dates and room, then generate a code.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                AutoSize = true
            };
            detailsPanel.Controls.Add(lblSubtitle);

            y += 30;
            var lblGuestInfo = new Label
            {
                Text = "Guest Information",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            detailsPanel.Controls.Add(lblGuestInfo);

            y += 35;
            detailsPanel.Controls.Add(new Label { Text = "First Name *", Location = new Point(20, y), Size = new Size(100, 20), Font = new Font("Segoe UI", 9) });
            txtFirstName = new TextBox { Location = new Point(20, y + 22), Size = new Size(160, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "John", BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtFirstName);

            detailsPanel.Controls.Add(new Label { Text = "Middle Name", Location = new Point(190, y), Size = new Size(100, 20), Font = new Font("Segoe UI", 9) });
            txtMiddleName = new TextBox { Location = new Point(190, y + 22), Size = new Size(160, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "A.", BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtMiddleName);

            detailsPanel.Controls.Add(new Label { Text = "Last Name *", Location = new Point(360, y), Size = new Size(100, 20), Font = new Font("Segoe UI", 9) });
            txtLastName = new TextBox { Location = new Point(360, y + 22), Size = new Size(180, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "Doe", BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtLastName);

            y += 60;
            detailsPanel.Controls.Add(new Label { Text = "Email *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            txtEmail = new TextBox { Location = new Point(20, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "john@example.com", BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtEmail);

            detailsPanel.Controls.Add(new Label { Text = "Phone Number *", Location = new Point(290, y), Size = new Size(140, 20), Font = new Font("Segoe UI", 9) });
            txtPhone = new TextBox { Location = new Point(290, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), PlaceholderText = "9xxxxxxxxx", BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtPhone);

            y += 60;
            detailsPanel.Controls.Add(new Label { Text = "ID Type *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            cboIDType = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboIDType.Items.AddRange(new object[] { "Passport", "Driver License", "National ID", "Others" });
            detailsPanel.Controls.Add(cboIDType);

            detailsPanel.Controls.Add(new Label { Text = "ID Number *", Location = new Point(290, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            txtIDNumber = new TextBox { Location = new Point(290, y + 22), Size = new Size(250, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            detailsPanel.Controls.Add(txtIDNumber);

            y += 60;
            var lblStayInfo = new Label
            {
                Text = "Stay & Room",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true
            };
            detailsPanel.Controls.Add(lblStayInfo);

            y += 35;
            detailsPanel.Controls.Add(new Label { Text = "Check-In *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            dtpCheckIn = new DateTimePicker
            {
                Location = new Point(20, y + 22),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };
            detailsPanel.Controls.Add(dtpCheckIn);

            detailsPanel.Controls.Add(new Label { Text = "Check-Out *", Location = new Point(290, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            dtpCheckOut = new DateTimePicker
            {
                Location = new Point(290, y + 22),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today.AddDays(1),
                Value = DateTime.Today.AddDays(1)
            };
            detailsPanel.Controls.Add(dtpCheckOut);

            y += 60;
            detailsPanel.Controls.Add(new Label { Text = "Room *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            cboRoom = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(520, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "DisplayText",
                ValueMember = "RoomId"
            };
            detailsPanel.Controls.Add(cboRoom);

            y += 60;
            detailsPanel.Controls.Add(new Label { Text = "Guests *", Location = new Point(20, y), Size = new Size(120, 20), Font = new Font("Segoe UI", 9) });
            numGuests = new NumericUpDown
            {
                Location = new Point(20, y + 22),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };
            detailsPanel.Controls.Add(numGuests);

            detailsPanel.Controls.Add(new Label { Text = "Special Requests", Location = new Point(160, y), Size = new Size(140, 20), Font = new Font("Segoe UI", 9) });
            txtSpecialRequests = new TextBox
            {
                Location = new Point(160, y + 22),
                Size = new Size(380, 60),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            detailsPanel.Controls.Add(txtSpecialRequests);

            y += 95;
            lblDownpayment = new Label
            {
                Text = $"Downpayment: {0m.ToString("C2", currencyCulture)}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            detailsPanel.Controls.Add(lblDownpayment);

            y += 35;
            detailsPanel.Controls.Add(new Label
            {
                Text = "Payment Method *",
                Location = new Point(20, y),
                Size = new Size(140, 20),
                Font = new Font("Segoe UI", 9)
            });
            cboPaymentMethod = new ComboBox
            {
                Location = new Point(160, y),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboPaymentMethod.Items.AddRange(new object[] { "Cash", "Credit Card", "Debit Card" });
            cboPaymentMethod.SelectedIndex = 0;
            detailsPanel.Controls.Add(cboPaymentMethod);

            // Move Transaction Reference BELOW Payment Method
            y += 35;
            detailsPanel.Controls.Add(new Label
            {
                Text = "Transaction Reference",
                Location = new Point(20, y),
                Size = new Size(140, 20),
                Font = new Font("Segoe UI", 9)
            });
            txtTransactionRef = new TextBox
            {
                Location = new Point(160, y),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            detailsPanel.Controls.Add(txtTransactionRef);

            y += 35;
            lblTotalAmount = new Label
            {
                Text = $"Total: {0m.ToString("C2", currencyCulture)}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            detailsPanel.Controls.Add(lblTotalAmount);

            // Use RoundedButton for the main action button
            btnCreateReservation = new RoundedButton
            {
                Text = "Generate Code",
                Location = new Point(20, y + 35),
                Size = new Size(520, 45),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BorderRadius = 10,
                BorderSize = 0
            };
            ((RoundedButton)btnCreateReservation).FlatAppearance.BorderSize = 0;
            detailsPanel.Controls.Add(btnCreateReservation);

            // --- RIGHT: AmenitiesPanel ---
            amenitiesPanel = new AmenitiesPanel
            {
                Name = "amenitiesPanel",
                Dock = DockStyle.Top,
                Margin = new Padding(24, 32, 24, 0),
                Height = 400 // Adjust as needed for your UI
            };

            // Add to main split
            mainSplit.Controls.Add(detailsPanel, 0, 0);
            mainSplit.Controls.Add(amenitiesPanel, 1, 0);

            // For signature compatibility, set cboAmenities to null
            cboAmenities = null!;

            parent.Controls.Add(mainSplit);

            parent.ResumeLayout(false);
            parent.PerformLayout();
        }
    }
}
