using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace HotelMgt.Services
{
    public sealed class EmailService
    {
        private readonly EmailSettings _settings;
        public EmailService(EmailSettings settings) => _settings = settings;

        // Existing sync method retained (now delegates to async)
        public bool TrySendReservationCode(
            string recipientEmail,
            string guestFullName,
            string reservationCode,
            DateTime checkIn,
            DateTime checkOut,
            string roomNumber,
            decimal totalAmount,
            CultureInfo currencyCulture,
            out string? error)
        {
            var task = TrySendReservationCodeAsync(
                recipientEmail, guestFullName, reservationCode,
                checkIn, checkOut, roomNumber, totalAmount,
                currencyCulture);
            task.Wait(); // simple blocking for legacy callers
            (bool success, string? err) = task.Result;
            error = err;
            return success;
        }

        // New async HTML + plain text sending
        public async Task<(bool success, string? error)> TrySendReservationCodeAsync(
            string recipientEmail,
            string guestFullName,
            string reservationCode,
            DateTime checkIn,
            DateTime checkOut,
            string roomNumber,
            decimal totalAmount,
            CultureInfo currencyCulture)
        {
            if (!_settings.IsConfigured())
                return (false, "Email not configured.");

            try
            {
                using var client = new SmtpClient
                {
                    Host = _settings.Host,
                    Port = _settings.Port,
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                    Timeout = 15000
                };

                var plainText = BuildPlainText(
                    guestFullName, reservationCode, roomNumber,
                    checkIn, checkOut, totalAmount, currencyCulture);

                var htmlBody = BuildHtmlBody(
                    guestFullName, reservationCode, roomNumber,
                    checkIn, checkOut, totalAmount, currencyCulture);

                using var msg = new MailMessage
                {
                    From = new MailAddress(_settings.FromAddress, _settings.FromName),
                    Subject = $"{reservationCode} is your hotel reservation code",
                    Body = plainText,
                    IsBodyHtml = false
                };
                msg.To.Add(recipientEmail.Trim());

                // Alternate views (HTML + Plain Text)
                var plainView = AlternateView.CreateAlternateViewFromString(plainText, Encoding.UTF8, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                msg.AlternateViews.Add(plainView);
                msg.AlternateViews.Add(htmlView);

                await client.SendMailAsync(msg);
                return (true, null);
            }
            catch (SmtpException smtpEx)
            {
                return (false, $"SMTP error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string? error)> TrySendReservationCodeWithAttachmentAsync(
            string recipientEmail,
            string guestFullName,
            string reservationCode,
            DateTime checkIn,
            DateTime checkOut,
            string roomNumber,
            decimal totalAmount,
            CultureInfo currencyCulture,
            byte[] pdfBytes,
            string pdfFileName = "ReservationConfirmation.pdf")
        {
            if (!_settings.IsConfigured())
                return (false, "Email not configured.");

            try
            {
                using var client = new SmtpClient
                {
                    Host = _settings.Host,
                    Port = _settings.Port,
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                    Timeout = 15000
                };

                var plainText = BuildPlainText(
                    guestFullName, reservationCode, roomNumber,
                    checkIn, checkOut, totalAmount, currencyCulture);

                var htmlBody = BuildHtmlBody(
                    guestFullName, reservationCode, roomNumber,
                    checkIn, checkOut, totalAmount, currencyCulture);

                using var msg = new MailMessage
                {
                    From = new MailAddress(_settings.FromAddress, _settings.FromName),
                    Subject = $"{reservationCode} is your hotel reservation code",
                    Body = plainText,
                    IsBodyHtml = false
                };
                msg.To.Add(recipientEmail.Trim());

                // Alternate views (HTML + Plain Text)
                var plainView = AlternateView.CreateAlternateViewFromString(plainText, Encoding.UTF8, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                msg.AlternateViews.Add(plainView);
                msg.AlternateViews.Add(htmlView);

                // Attach PDF
                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    var pdfStream = new System.IO.MemoryStream(pdfBytes);
                    var attachment = new Attachment(pdfStream, pdfFileName, "application/pdf");
                    msg.Attachments.Add(attachment);
                }

                await client.SendMailAsync(msg);
                return (true, null);
            }
            catch (SmtpException smtpEx)
            {
                return (false, $"SMTP error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static string BuildPlainText(
            string guestFullName,
            string code,
            string room,
            DateTime checkIn,
            DateTime checkOut,
            decimal total,
            CultureInfo culture) =>
            new StringBuilder()
                .AppendLine($"Hello {guestFullName},")
                .AppendLine()
                .AppendLine("Thank you for your reservation. Here are your details:")
                .AppendLine($"Reservation Code: {code}")
                .AppendLine($"Room: {room}")
                .AppendLine($"Check-in: {checkIn:d}")
                .AppendLine($"Check-out: {checkOut:d}")
                .AppendLine($"Total: {total.ToString("C2", culture)}")
                .AppendLine()
                .AppendLine("Please keep this code for check-in.")
                .AppendLine()
                .AppendLine($"Generated at {DateTime.Now:G}")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("Golden Courtyard")
                .ToString();

        private static string BuildHtmlBody(
            string guestFullName,
            string code,
            string room,
            DateTime checkIn,
            DateTime checkOut,
            decimal total,
            CultureInfo culture) =>
            $@"<!DOCTYPE html>
                <html>
                <head>
                <meta charset=""utf-8""/>
                <title>Reservation Confirmation</title>
                <style>
                 body {{ 
                    font-family:Segoe UI,Arial,sans-serif; 
                    background:#f8fafc; color:#1e293b; }}
                 .wrapper {{ 
                    max-width:560px; 
                    margin:0 auto; 
                    background:#ffffff; 
                    padding:18px 24px; 
                    border:1px solid #e2e8f0; 
                    border-radius:8px; }}
                 h1 {{ 
                    font-size:20px; 
                    margin:0 0 12px; }}
                 .code {{ 
                    font-size:24px; 
                    font-weight:700; 
                    letter-spacing:2px; 
                    color:#0f172a; 
                    background:#eef2ff; 
                    padding:8px 14px; 
                    border-radius:6px; 
                    display:inline-block; }}
                 table {{ 
                    width:100%; 
                    border-collapse:collapse; 
                    margin-top:16px; }}
                 td {{ 
                    padding:6px 4px; 
                    vertical-align:top; }}
                 .label {{ 
                    font-weight:600; 
                    width:140px; }}
                 .footer {{ 
                    font-size:12px; 
                    margin-top:22px; 
                    color:#64748b; }}
                </style>
                </head>
                <body>
                  <div class=""wrapper"">
                    <h1>Your Reservation Details</h1>
                    <p>Hello {System.Web.HttpUtility.HtmlEncode(guestFullName)},</p>
                    <p>Thank you for your reservation. Keep the code below safe:</p>
                    <div class=""code"">{System.Web.HttpUtility.HtmlEncode(code)}</div>
                    <table>
                      <tr><td class=""label"">Room</td><td>{System.Web.HttpUtility.HtmlEncode(room)}</td></tr>
                      <tr><td class=""label"">Check-in</td><td>{checkIn:d}</td></tr>
                      <tr><td class=""label"">Check-out</td><td>{checkOut:d}</td></tr>
                      <tr><td class=""label"">Total</td><td>{total.ToString("C2", culture)}</td></tr>
                      <tr><td class=""label"">Generated</td><td>{DateTime.Now:G}</td></tr>
                    </table>
                    <p style=""margin-top:18px;"">Present this code at check-in.</p>
                    <div class=""footer"">
                      Regards,<br/>Golden Courtyard
                    </div>
                  </div>
                </body>
                </html>";
    }
}