using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaklerWebApp.MVC.Services.Api;

public class MaklerApiClient : IMaklerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    public async Task<ApiPagedResult<ApiListingSummary>> SearchListingsAsync(ApiListingSearchRequest request, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["keyword"] = request.Keyword,
            ["city"] = request.City,
            ["minPrice"] = request.MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["maxPrice"] = request.MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["page"] = request.Page.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["pageSize"] = request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString().ToLowerInvariant()
        };

        var url = QueryHelpers.AddQueryString("api/listings", queryParams!);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new ApiPagedResult<ApiListingSummary>();
        }

        return await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(cancellationToken: cancellationToken)
               ?? new ApiPagedResult<ApiListingSummary>();
    }

    public async Task<ApiListingDetails?> GetListingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/listings/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiListingDetails>(JsonOptions, cancellationToken);
    }

    public async Task AddListingViewAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/listings/{id}/views", content: null, cancellationToken);
        _ = response.IsSuccessStatusCode;
    }

    public async Task<ApiTokenResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiTokenResponse>(JsonOptions, cancellationToken);
    }

    public async Task<ApiTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new ApiRefreshTokenRequest
        {
            RefreshToken = refreshToken
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiTokenResponse>(JsonOptions, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/logout", new ApiLogoutRequest
        {
            RefreshToken = refreshToken
        }, cancellationToken);

        _ = response.IsSuccessStatusCode;
    }

    public async Task<ApiTokenResponse?> RegisterAsync(ApiRegisterRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiTokenResponse>(JsonOptions, cancellationToken);
    }

    public async Task<bool> RequestOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/request-otp", new { emailOrPhone }, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyOtpAsync(ApiVerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiVerifyOtpResponse>(JsonOptions, cancellationToken);
        return payload?.Verified == true;
    }

    public async Task<int?> GetPublicListingCountAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/listings?page=1&pageSize=1", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(JsonOptions, cancellationToken);
        return result?.TotalCount;
    }

    public async Task<IReadOnlyList<ApiPaymentHistoryItem>> GetPaymentHistoryAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/payments/history");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ApiPaymentHistoryItem>();
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<ApiPaymentHistoryItem>>(JsonOptions, cancellationToken);
        return result ?? Array.Empty<ApiPaymentHistoryItem>();
    }
}
