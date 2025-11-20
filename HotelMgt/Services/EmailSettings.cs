using System;

namespace HotelMgt.Services
{
    public sealed class EmailSettings
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; } = 587;
        public bool EnableSsl { get; init; } = true;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FromAddress { get; init; } = string.Empty;
        public string FromName { get; init; } = "Reservations";

        public static EmailSettings LoadFromEnvironment() => new()
        {
            Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "",
            Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587,
            EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLESSL"), out var ssl) ? ssl : true,
            UserName = Environment.GetEnvironmentVariable("SMTP_USER") ?? "",
            Password = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "",
            FromAddress = Environment.GetEnvironmentVariable("SMTP_FROM") ?? (Environment.GetEnvironmentVariable("SMTP_USER") ?? ""),
            FromName = Environment.GetEnvironmentVariable("SMTP_FROMNAME") ?? "Reservations"
        };

        public bool IsConfigured() =>
            !string.IsNullOrWhiteSpace(Host) &&
            Port > 0 &&
            !string.IsNullOrWhiteSpace(UserName) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(FromAddress);
    }
}