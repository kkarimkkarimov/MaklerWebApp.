namespace MaklerWebApp.MVC.Services.Api;

public interface IMaklerApiClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
