using MaklerWebApp.BLL.Models;
using MaklerWebApp.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return result is null ? Unauthorized(new { message = "Email or password is invalid." }) : Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return result is null ? Unauthorized(new { message = "Refresh token is invalid or expired." }) : Ok(result);
    }

    [HttpPost("request-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken cancellationToken)
    {
        await _authService.RequestOtpAsync(request, cancellationToken);
        return Ok(new { message = "If account exists, OTP has been sent." });
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var ok = await _authService.VerifyOtpAsync(request, cancellationToken);
        return ok ? Ok(new { verified = true }) : BadRequest(new { verified = false, message = "Invalid OTP or user not found." });
    }
}
