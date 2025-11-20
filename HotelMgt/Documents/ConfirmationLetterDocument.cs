using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HotelMgt.Documents
{
    public class ConfirmationLetterDocument : IDocument
    {
        private readonly ReservationReceipt _data;
        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        public ConfirmationLetterDocument(ReservationReceipt data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeFirstPageContent);

                    page.Footer().AlignCenter().Element(c =>
                    {
                        c.Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                         .FontSize(9)
                         .FontColor(Colors.Grey.Darken1);
                    });
                })
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposePoliciesSection);

                    page.Footer().AlignCenter().Element(c =>
                    {
                        c.Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                         .FontSize(9)
                         .FontColor(Colors.Grey.Darken1);
                    });
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(8).Row(row =>
            {
                // Left: logo placeholder (constant width)
                row.ConstantItem(120).Height(80).AlignMiddle().AlignLeft().Element(logo =>
                {
                    logo.AlignCenter()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(6)
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("LOGO").SemiBold().FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                });

                // Right: hotel name and date block
                row.RelativeItem().PaddingLeft(8).Column(col =>
                {
                    col.Item().AlignRight().Text("Golden Courtyard").FontSize(20).SemiBold();
                    col.Item().AlignRight().Text("LETTER OF CONFIRMATION").FontSize(11).Underline().SemiBold();
                    col.Item().AlignRight().PaddingTop(6).Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(9);
                });
            });
        }

        private void ComposeFirstPageContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(6).Text($"Dear {_data.GuestFullName},").FontSize(11);

                column.Item().PaddingTop(6).Text("Thank you for making Golden Courtyard your first choice when staying with us. We are pleased to send you this confirmation letter based on the following details:").FontSize(10);

                // Details table
                column.Item().PaddingVertical(8).Element(ComposeDetailsTable);

                // Room table
                column.Item().PaddingTop(6).Element(ComposeRoomTable);

                // All requests are subject...
                column.Item().PaddingTop(10).Text("All requests are subject to availability upon arrival at the Front Desk.").FontSize(10).Bold();

                // Check-in/out times
                column.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Check-in time: 03:00 PM (GMT +08:00)").FontSize(10).Bold();
                    row.RelativeItem().AlignRight().Text("Check-out time: 11:00 AM (GMT +08:00)").FontSize(10).Bold();
                });

                // Cancellation and No-Show Policy
                column.Item().PaddingTop(12).Text("Cancellations and No-Show Policy:").Bold().FontSize(10);
                column.Item().Text("This booking is non-refundable, non-cancellable but rebookable and transferrable between Month/ Date/ Year to Month/ Date/ Year. No-show: Guests who do not arrive within 24 hours on check-in date will be charged the whole amount of the reservation. Same-day cancellation shall be charged the full amount equivalent to the whole duration of your reservation and/or forfeiture of all payments made.").FontSize(9);

                // Billing Arrangement
                column.Item().PaddingTop(12).Text("Billing Arrangement:").Bold().FontSize(10);
                column.Item().Text("Pre-payment for room charges may be settled in cash or credit card at Golden Courtyard. For BDO bills payment made through Direct Bank Deposit, please use your confirmation number(s) as your Reference Number.").FontSize(9);

                // Bank accounts
                column.Item().PaddingTop(10).Element(ComposeBankAccounts);
            });
        }

        private void ComposeDetailsTable(IContainer c)
        {
            c.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(140);
                    columns.RelativeColumn();
                });

                void AddRow(string label, string value)
                {
                    table.Cell().Padding(6).Background(Colors.Grey.Lighten4).Text(label).FontSize(10).SemiBold();
                    table.Cell().Padding(6).Text(value).FontSize(10);
                }

                AddRow("Name of the Guest(s)", _data.GuestFullName);
                AddRow("Number of Guest(s)", $"Adult: {_data.NumberOfGuests}");
                AddRow("Arrival Date", _data.CheckIn.ToString("yyyy-MM-dd"));
                AddRow("Check-out Date", _data.CheckOut.ToString("yyyy-MM-dd"));
                AddRow("Confirmation Number", _data.ReservationCode);

                table.Cell().Padding(6).Background(Colors.Grey.Lighten4).Text("Inclusions").FontSize(10).SemiBold();
                table.Cell().Padding(6).Element(x =>
                {
                    x.Column(col =>
                    {
                        if (_data.Inclusions.Count == 0)
                        {
                            col.Item().Text("-").FontSize(10);
                        }
                        else
                        {
                            foreach (var inc in _data.Inclusions)
                                col.Item().Text($"- {inc}").FontSize(10);
                        }
                    });
                });
            });
        }

        private void ComposeRoomTable(IContainer c)
        {
            c.PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn();
                    columns.ConstantColumn(140);
                });

                // Header row
                table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Number of Room(s)").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Room Type").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Room Rate").Bold();

                // Data row
                table.Cell().Padding(6).Text("1");
                table.Cell().Padding(6).Text($"{_data.RoomType} (Room {_data.RoomNumber})");
                table.Cell().Padding(6).AlignRight().Text($"{_data.PricePerNight.ToString("C2", _currencyCulture)} net room / night");
            });
        }

        private void ComposeBankAccounts(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(bankCol =>
                    {
                        bankCol.Item().Text("BANCO DE ORO - Peso Account").Bold().FontSize(9);
                        bankCol.Item().Text("Branch: Makati Main").FontSize(9);
                        bankCol.Item().Text("Account Name: Golden Courtyard Hotel, Inc.").FontSize(9);
                        bankCol.Item().Text("Account No.: 00456-801-2345").FontSize(9);
                    });
                    row.RelativeItem().Column(bankCol =>
                    {
                        bankCol.Item().Text("CHINA BANK - Peso Account").Bold().FontSize(9);
                        bankCol.Item().Text("Branch: Ortigas Center").FontSize(9);
                        bankCol.Item().Text("Account Name: Golden Courtyard Hotel, Inc.").FontSize(9);
                        bankCol.Item().Text("Account No.: 1592-0000-8888").FontSize(9);
                    });
                });
                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(bankCol =>
                    {
                        bankCol.Item().Text("LAND BANK - Peso Account").Bold().FontSize(9);
                        bankCol.Item().Text("Branch: Mandaluyong").FontSize(9);
                        bankCol.Item().Text("Account Name: Golden Courtyard Hotel, Inc.").FontSize(9);
                        bankCol.Item().Text("Account No.: 002932-1007-73").FontSize(9);
                    });
                    row.RelativeItem();
                });
            });
        }

        private void ComposePoliciesSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Early Check-in and late Check-out Policy:").Bold().FontSize(10);
                col.Item().Text("Requests for early check-in will be noted on your reservation but will be subject to availability on check-in date. Late Check-out request will be arranged at the Front Desk and request is subject to availability as well.").FontSize(9);

                col.Item().PaddingTop(8).Text("Incidental Deposit Policy:").Bold().FontSize(10);
                col.Item().Text("An incidental deposit of PHP 2,000 per room, per night is required upon check-in. Deposit will be refunded upon check-out if no costs are incurred.").FontSize(9);

                col.Item().PaddingTop(8).Text("Children Policy:").Bold().FontSize(10);
                col.Item().Text("1. Infant 0 - 1 year old: Stay for free if using existing bedding.\n2. Children 2 - 11 year(s): Stay for free if using existing bedding.\n3. A maximum of 2 children can stay free of charge when sharing beds with adults.\n4. Children 12 years old and above are considered adults. Request for extra beds will be charged.").FontSize(9);

                col.Item().PaddingTop(8).Text("No Smoking Policy:").Bold().FontSize(10);
                col.Item().Text("All rooms are non-smoking rooms, including bathrooms. A penalty will be imposed for violation. Tampering with, disabling or destroying the room smoke detectors is also prohibited by the law.").FontSize(9);

                col.Item().PaddingTop(8).Text("Party Policy:").Bold().FontSize(10);
                col.Item().Text("1. All rooms can accommodate 2 adults and 2 children at any given time.\n2. We kindly request that you'll be considerate to other guests and reduce the volume of radios, televisions, and voices after 10pm.\n3. Guests who will extend beyond 10pm must register at the Guest Services Counter at the lobby.\n4. Only registered guests are allowed at the Pool area and use the swimming pool.\n5. We enforce a no in-room party policy to ensure the comfort of all our guests.").FontSize(9);

                col.Item().PaddingTop(8).Text("Other Policies:").Bold().FontSize(10);
                col.Item().Text("No pets are allowed inside the hotel. Durian, Marang and other fruits with strong smell are not allowed to be brought inside the hotel. Firearms are to be deposited at the security area. All Food and drinks brought shall be subject to a corkage fee.").FontSize(9);

                col.Item().PaddingTop(8).Text("Airport Pick-up:").Bold().FontSize(10);
                col.Item().Text("Guests are required to confirm their pick-up request via email at reservations@goldencourtyard.com.ph. If you have booked an airport pick up, please proceed to the hotel's airport counter at the arrival area and look for our Airport Representative with Golden Courtyard signage. In the event our Airport Representative number (63) 968 899 4795 is not available, please contact the Concierge on (63) 977 807 4422. Our transportation rates from airport to hotel are as follows:\n• Car: good for 3 passengers with light luggage - PHP 950 per way\n• Urvan: good for 6 passengers with light luggage - PHP 1,800 per way\nA pre-payment is required for this request which may be made through a Credit Card Payment link or Bank Deposit.").FontSize(9);

                col.Item().PaddingTop(8).Text("Payment:").Bold().FontSize(10);
                col.Item().Text("Reminding our valued guests that payment will be in Philippine Currency. The hotel's prevailing conversion rate shall apply.").FontSize(9);

                col.Item().PaddingTop(8).Text("Credit Card Fraud Protection:").Bold().FontSize(10);
                col.Item().Text("If a booking is suspected to be made using a fraudulent credit card, the hotel may cancel the confirmed reservation. We will require necessary documents for credit card prepayments.").FontSize(9);

                // Final message and regards
                col.Item().PaddingTop(16).Text("Should you have further inquiries or concerns with your booking, please do not hesitate to contact us. Thank you and we are looking forward to serving you soon!").FontSize(9);

                col.Item().PaddingTop(16).Text("Best regards,").FontSize(10);
                col.Item().Text("Reservations Department").FontSize(10).SemiBold();
            });
        }

        // Helpers to create PDF bytes
        public byte[] GeneratePdfBytes()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }

        public void GeneratePdfFile(string filePath)
        {
            var bytes = GeneratePdfBytes();
            System.IO.File.WriteAllBytes(filePath, bytes);
        }
    }
}
