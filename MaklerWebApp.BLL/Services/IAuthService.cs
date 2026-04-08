using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
}
