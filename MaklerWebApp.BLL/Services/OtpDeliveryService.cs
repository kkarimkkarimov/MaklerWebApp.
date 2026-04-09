using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MaklerWebApp.BLL.Services;

public class OtpDeliveryService : IOtpDeliveryService
{
    private readonly ILogger<OtpDeliveryService> _logger;
    private readonly OtpEmailOptions _options;

    public OtpDeliveryService(ILogger<OtpDeliveryService> logger, IOptions<OtpEmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(string destination, string code, CancellationToken cancellationToken = default)
    {
        return SendCoreAsync(destination, code, cancellationToken);
    }

    private async Task SendCoreAsync(string destination, string code, CancellationToken cancellationToken)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogWarning("OTP provider is disabled or not configured. Destination: {Destination}", destination);
            if (_options.LogCodeInPlainText)
            {
                _logger.LogInformation("DEV OTP code for {Destination}: {Code}", destination, code);
            }

            throw new InvalidOperationException("OTP email service is not configured. Set OtpEmail:Enabled=true and provide SMTP settings.");
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = _options.Subject,
                Body = $"Your OTP code is: {code}. It expires in 5 minutes.",
                IsBodyHtml = false
            };

            message.To.Add(destination);

            using var smtp = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                smtp.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            await smtp.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("OTP email sent to {Destination}", destination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Destination}", destination);
            if (_options.LogCodeInPlainText)
            {
                _logger.LogInformation("Fallback DEV OTP code for {Destination}: {Code}", destination, code);
            }

            throw new InvalidOperationException("OTP code could not be sent. Check SMTP credentials and try again.");
        }
    }
}
