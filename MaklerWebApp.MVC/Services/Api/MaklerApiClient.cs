namespace MaklerWebApp.MVC.Services.Api;

public class MaklerApiClient : IMaklerApiClient
{
    private readonly HttpClient _httpClient;

    public MaklerApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
