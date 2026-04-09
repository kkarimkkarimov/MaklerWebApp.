using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task RequestOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<VerifyOtpResult?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
}
