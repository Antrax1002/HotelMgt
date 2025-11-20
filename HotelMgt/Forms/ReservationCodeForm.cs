using HotelMgt.Documents;
using HotelMgt.Services;
using System;
using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelMgt.Forms
{
    public sealed class ReservationCodeForm : Form
    {
        private readonly EmailService _emailService;
        private readonly ActivityLogService _logService;
        private readonly CultureInfo _culture;
        private readonly ReservationReceipt _receipt;


        private readonly int _employeeId;
        private readonly int _reservationId;
        private readonly decimal _total;
        private readonly DateTime _checkIn;
        private readonly DateTime _checkOut;
        private readonly string _roomNumber;
        private readonly string _guestFullName;

        private TextBox txtEmail = null!;
        private TextBox txtCode = null!;
        private TextBox txtMessage = null!;
        private Button btnSend = null!;
        private Button btnCancel = null!;
        private Label lblStatus = null!;

        public ReservationCodeForm(
            EmailService emailService,
            ActivityLogService logService,
            CultureInfo culture,
            int employeeId,
            int reservationId,
            string reservationCode,
            string guestFullName,
            string initialEmail,
            string roomNumber,
            DateTime checkIn,
            DateTime checkOut,
            decimal total,
            ReservationReceipt receipt)
        {
            _emailService = emailService;
            _logService = logService;
            _culture = culture;
            _employeeId = employeeId;
            _reservationId = reservationId;
            _guestFullName = guestFullName;
            _roomNumber = roomNumber;
            _checkIn = checkIn;
            _checkOut = checkOut;
            _total = total;
            _receipt = receipt;


            Text = "Reservation Code";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(560, 480);

            BuildUi(reservationCode, initialEmail);
        }

        private void BuildUi(string code, string email)
        {
            var lblTitle = new Label
            {
                Text = "Reservation Code Generated",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            var lblInstructions = new Label
            {
                Text = "Edit email/message if required, then Send.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Location = new Point(20, 45),
                AutoSize = true
            };
            Controls.Add(lblInstructions);

            Controls.Add(new Label { Text = "Reservation Code", Location = new Point(20, 80), Size = new Size(140, 18) });
            txtCode = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(240, 26),
                ReadOnly = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = code
            };
            Controls.Add(txtCode);

            Controls.Add(new Label { Text = "Customer Email", Location = new Point(280, 80), Size = new Size(140, 18) });
            txtEmail = new TextBox
            {
                Location = new Point(280, 100),
                Size = new Size(250, 26),
                Font = new Font("Segoe UI", 10),
                Text = email
            };
            Controls.Add(txtEmail);

            Controls.Add(new Label { Text = "Message (Plain Text Preview)", Location = new Point(20, 140), Size = new Size(200, 18) });
            txtMessage = new TextBox
            {
                Location = new Point(20, 160),
                Size = new Size(510, 220),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                Text = BuildDefaultMessage(code)
            };
            Controls.Add(txtMessage);

            btnSend = new Button
            {
                Text = "Send Email (Async)",
                Location = new Point(20, 395),
                Size = new Size(180, 38),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += async (s, e) => await BtnSendAsync();
            Controls.Add(btnSend);

            btnCancel = new Button
            {
                Text = "Close",
                Location = new Point(220, 395),
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(20, 440),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };
            Controls.Add(lblStatus);
        }

        private string BuildDefaultMessage(string code) =>
            $"Hello {_guestFullName},\r\n\r\n" +
            $"Thank you for your reservation.\r\n" +
            $"Reservation Code: {code}\r\n" +
            $"Room: {_roomNumber}\r\n" +
            $"Check-in: {_checkIn:d}\r\n" +
            $"Check-out: {_checkOut:d}\r\n" +
            $"Total: {_total.ToString("C2", _culture)}\r\n\r\n" +
            "Please keep this code for check-in.\r\n\r\nRegards,\r\nGolden Courtyard";

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

        private async Task BtnSendAsync()
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = "";

            var email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                lblStatus.Text = "Email required.";
                txtEmail.Focus();
                return;
            }
            if (!IsValidEmail(email))
            {
                lblStatus.Text = "Invalid email format.";
                txtEmail.Focus();
                return;
            }

            btnSend.Enabled = false;
            lblStatus.Text = "Sending...";
            await Task.Delay(50); // brief UI flush

            // --- PDF generation ---
            var doc = new ConfirmationLetterDocument(_receipt);
            var pdfBytes = doc.GeneratePdfBytes();

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileName = $"{_receipt.ReservationCode}_Confirmation.pdf";
            var filePath = Path.Combine(desktop, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // --- EMAIL with PDF attachment ---
            var (success, error) = await _emailService.TrySendReservationCodeWithAttachmentAsync(
                email,
                _guestFullName,
                _receipt.ReservationCode,
                _checkIn,
                _checkOut,
                _roomNumber,
                _total,
                _culture,
                pdfBytes,
                fileName
            );

            if (success)
            {
                lblStatus.ForeColor = Color.DarkGreen;
                lblStatus.Text = "Email sent successfully. PDF saved to Desktop.";
                LogEmailActivity(true, email, null);
            }
            else
            {
                lblStatus.Text = $"Failed: {error}";
                btnSend.Enabled = true;
                LogEmailActivity(false, email, error);
            }
        }

        private void LogEmailActivity(bool success, string email, string? error)
        {
            try
            {
                _logService.LogActivity(
                    _employeeId,
                    success ? "EmailSent" : "EmailFailed",
                    success
                        ? $"Reservation email sent to {email} (Reservation #{_reservationId})."
                        : $"Reservation email FAILED to {email} (Reservation #{_reservationId}). Error: {error}",
                    _reservationId);
            }
            catch
            {
                // swallow logging issues
            }
        }
    }
}