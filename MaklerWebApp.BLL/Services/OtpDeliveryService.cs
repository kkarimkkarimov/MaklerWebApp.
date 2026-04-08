using Microsoft.Extensions.Logging;

namespace MaklerWebApp.BLL.Services;

public class OtpDeliveryService : IOtpDeliveryService
{
    private readonly ILogger<OtpDeliveryService> _logger;

    public OtpDeliveryService(ILogger<OtpDeliveryService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string destination, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OTP code sent to {Destination}", destination);
        _logger.LogDebug("OTP code for {Destination}: {Code}", destination, code);
        return Task.CompletedTask;
    }
}
