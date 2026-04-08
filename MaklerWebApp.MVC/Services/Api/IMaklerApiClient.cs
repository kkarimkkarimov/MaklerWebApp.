using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.Http;

namespace MaklerWebApp.MVC.Services.Api;

public interface IMaklerApiClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task<ApiPagedResult<ApiListingSummary>> SearchListingsAsync(ApiListingSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiPagedResult<ApiListingSummary>> GetMyListingsAsync(string accessToken, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ApiListingSummary?> CreateListingAsync(string accessToken, ApiCreateListingRequest request, CancellationToken cancellationToken = default);
    Task<ApiListingDetails?> GetListingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddListingViewAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiTokenResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiTokenResponse?> RegisterAsync(ApiRegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RequestOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(ApiVerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task<int?> GetPublicListingCountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiPaymentHistoryItem>> GetPaymentHistoryAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> UploadListingImagesAsync(string accessToken, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default);
}
