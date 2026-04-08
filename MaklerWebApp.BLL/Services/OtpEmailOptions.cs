namespace MaklerWebApp.BLL.Services;

public class OtpEmailOptions
{
    public const string SectionName = "OtpEmail";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "MaklerWebApp";
    public string Subject { get; set; } = "Your OTP verification code";
    public bool LogCodeInPlainText { get; set; }
}
