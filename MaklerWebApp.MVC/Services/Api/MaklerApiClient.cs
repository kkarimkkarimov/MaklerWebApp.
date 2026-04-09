using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaklerWebApp.MVC.Services.Api;

public class MaklerApiClient : IMaklerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private sealed class UploadImagesResponse
    {
        public List<string> ImageUrls { get; set; } = new();
    }

    public async Task<IReadOnlyList<ApiMapListingMarker>> SearchMapListingsAsync(ApiListingSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["keyword"] = request.Keyword,
                ["city"] = request.City,
                ["district"] = request.District,
                ["listingType"] = request.ListingType?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["propertyType"] = request.PropertyType?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minPrice"] = request.MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxPrice"] = request.MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minArea"] = request.MinArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxArea"] = request.MaxArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minRooms"] = request.MinRooms?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxRooms"] = request.MaxRooms?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isNewBuilding"] = request.IsNewBuilding?.ToString().ToLowerInvariant(),
                ["hasMortgage"] = request.HasMortgage?.ToString().ToLowerInvariant(),
                ["isMortgageEligible"] = request.IsMortgageEligible?.ToString().ToLowerInvariant(),
                ["isFurnished"] = request.IsFurnished?.ToString().ToLowerInvariant(),
                ["repairStatus"] = request.RepairStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["documentStatus"] = request.DocumentStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isFeatured"] = request.IsFeatured?.ToString().ToLowerInvariant(),
                ["adStatus"] = request.AdStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["onlyWithImages"] = request.OnlyWithImages?.ToString().ToLowerInvariant(),
                ["sortBy"] = request.SortBy,
                ["descending"] = request.Descending.ToString().ToLowerInvariant()
            };

            var url = QueryHelpers.AddQueryString("api/listings/map", queryParams!);
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<ApiMapListingMarker>();
            }

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<ApiMapListingMarker>>(JsonOptions, cancellationToken);
            return result ?? Array.Empty<ApiMapListingMarker>();
        }
        catch
        {
            return Array.Empty<ApiMapListingMarker>();
        }
    }

    public async Task<IReadOnlyList<ApiAzerbaijanLocation>> GetAzerbaijanLocationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/locations/az", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<ApiAzerbaijanLocation>();
            }

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<ApiAzerbaijanLocation>>(JsonOptions, cancellationToken);
            return result ?? Array.Empty<ApiAzerbaijanLocation>();
        }
        catch
        {
            return Array.Empty<ApiAzerbaijanLocation>();
        }
    }

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
        try
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["keyword"] = request.Keyword,
                ["city"] = request.City,
                ["district"] = request.District,
                ["listingType"] = request.ListingType?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["propertyType"] = request.PropertyType?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minPrice"] = request.MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxPrice"] = request.MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minArea"] = request.MinArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxArea"] = request.MaxArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minRooms"] = request.MinRooms?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxRooms"] = request.MaxRooms?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isNewBuilding"] = request.IsNewBuilding?.ToString().ToLowerInvariant(),
                ["hasMortgage"] = request.HasMortgage?.ToString().ToLowerInvariant(),
                ["isMortgageEligible"] = request.IsMortgageEligible?.ToString().ToLowerInvariant(),
                ["isFurnished"] = request.IsFurnished?.ToString().ToLowerInvariant(),
                ["repairStatus"] = request.RepairStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["documentStatus"] = request.DocumentStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isFeatured"] = request.IsFeatured?.ToString().ToLowerInvariant(),
                ["adStatus"] = request.AdStatus?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["onlyWithImages"] = request.OnlyWithImages?.ToString().ToLowerInvariant(),
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

            return await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(JsonOptions, cancellationToken)
                   ?? new ApiPagedResult<ApiListingSummary>();
        }
        catch
        {
            return new ApiPagedResult<ApiListingSummary>();
        }
    }

    public async Task<ApiPagedResult<ApiListingSummary>> GetMyListingsAsync(string accessToken, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/listings/me?page={page}&pageSize={pageSize}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return new ApiPagedResult<ApiListingSummary>();
        }

        return await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(JsonOptions, cancellationToken)
               ?? new ApiPagedResult<ApiListingSummary>();
    }

    public async Task<ApiListingSummary?> CreateListingAsync(string accessToken, ApiCreateListingRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/listings")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiListingSummary>(JsonOptions, cancellationToken);
    }

    public async Task<bool> UpdateListingAsync(string accessToken, int id, ApiCreateListingRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/listings/{id}")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteListingAsync(string accessToken, int id, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/listings/{id}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetListingAdStatusAsync(string accessToken, int id, int adStatus, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"api/listings/{id}/ad-status")
        {
            Content = JsonContent.Create(new ApiPatchListingAdStatusRequest
            {
                AdStatus = adStatus
            })
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<ApiListingDetails?> GetListingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/listings/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ApiListingDetails>(JsonOptions, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task AddListingViewAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync($"api/listings/{id}/views", content: null, cancellationToken);
            _ = response.IsSuccessStatusCode;
        }
        catch
        {
        }
    }

    public async Task<ApiTokenResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(await ReadErrorMessageAsync(response, "Email və ya şifrə yanlışdır."));
        }

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

    public async Task<ApiRegisterResponse?> RegisterAsync(ApiRegisterRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorMessageAsync(response, "Qeydiyyat tamamlanmadı."));
        }

        return await response.Content.ReadFromJsonAsync<ApiRegisterResponse>(JsonOptions, cancellationToken);
    }

    public async Task<bool> RequestOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/request-otp", new { emailOrPhone }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorMessageAsync(response, "OTP kodu göndərilə bilmədi."));
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<ApiTokenResponse?> VerifyOtpAsync(ApiVerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiVerifyOtpResponse>(JsonOptions, cancellationToken);
        return payload?.Verified == true ? payload.Token : null;
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallback)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch
        {
        }

        return fallback;
    }

    public async Task<int?> GetPublicListingCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/listings?page=1&pageSize=1", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiListingSummary>>(JsonOptions, cancellationToken);
            return result?.TotalCount;
        }
        catch
        {
            return null;
        }
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

    public async Task<ApiPaymentHistoryItem?> StartBoostPaymentAsync(string accessToken, ApiBoostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/payments/boost")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiPaymentHistoryItem>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiFavoriteItem>> GetFavoritesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/favorites");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ApiFavoriteItem>();
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<ApiFavoriteItem>>(JsonOptions, cancellationToken);
        return result ?? Array.Empty<ApiFavoriteItem>();
    }

    public async Task<bool> AddFavoriteAsync(string accessToken, int listingId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/favorites/{listingId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFavoriteAsync(string accessToken, int listingId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/favorites/{listingId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<ApiUserProfile?> GetMyProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiUserProfile>(JsonOptions, cancellationToken);
    }

    public async Task<ApiUserProfile?> UpdateMyProfileAsync(string accessToken, ApiUpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "api/users/me")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiUserProfile>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> UploadListingImagesAsync(string accessToken, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return Array.Empty<string>();
        }

        using var content = new MultipartFormDataContent();
        foreach (var file in files.Where(x => x is { Length: > 0 }))
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            }

            content.Add(streamContent, "files", file.FileName);
        }

        if (!content.Any())
        {
            return Array.Empty<string>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/listings/images/upload")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Access token is unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<string>();
        }

        var payload = await response.Content.ReadFromJsonAsync<UploadImagesResponse>(JsonOptions, cancellationToken);
        if (payload is null || payload.ImageUrls.Count == 0)
        {
            return Array.Empty<string>();
        }

        return payload.ImageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => Uri.TryCreate(url, UriKind.Absolute, out _)
                ? url
                : new Uri(_httpClient.BaseAddress!, url).ToString())
            .ToArray();
    }
}
