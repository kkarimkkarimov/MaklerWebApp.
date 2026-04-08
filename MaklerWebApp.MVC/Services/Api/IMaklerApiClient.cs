using MaklerWebApp.MVC.Services.Api.Contracts;

namespace MaklerWebApp.MVC.Services.Api;

public interface IMaklerApiClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task<ApiPagedResult<ApiListingSummary>> SearchListingsAsync(ApiListingSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiTokenResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiTokenResponse?> RegisterAsync(ApiRegisterRequest request, CancellationToken cancellationToken = default);
    Task<bool> RequestOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(ApiVerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task<int?> GetPublicListingCountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiPaymentHistoryItem>> GetPaymentHistoryAsync(string accessToken, CancellationToken cancellationToken = default);
}
