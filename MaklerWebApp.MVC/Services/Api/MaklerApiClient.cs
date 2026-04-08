using MaklerWebApp.MVC.Services.Api.Contracts;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

    public async Task<ApiTokenResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiTokenResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> RequestOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/request-otp", new { emailOrPhone }, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<int?> GetPublicListingCountAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/listings?page=1&pageSize=1", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(cancellationToken: cancellationToken);
        return result?.TotalCount;
    }

    public async Task<IReadOnlyList<ApiPaymentHistoryItem>> GetPaymentHistoryAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/payments/history");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ApiPaymentHistoryItem>();
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<ApiPaymentHistoryItem>>(cancellationToken: cancellationToken);
        return result ?? Array.Empty<ApiPaymentHistoryItem>();
    }
}
