namespace MaklerWebApp.BLL.Services;

public interface IOtpDeliveryService
{
    Task SendAsync(string destination, string code, CancellationToken cancellationToken = default);
}
