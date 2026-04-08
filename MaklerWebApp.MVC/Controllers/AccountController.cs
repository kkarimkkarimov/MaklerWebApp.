using MaklerWebApp.MVC.Infrastructure;
using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Services.Api;
using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace MaklerWebApp.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IMaklerApiClient _maklerApiClient;

    public AccountController(IMaklerApiClient maklerApiClient)
    {
        _maklerApiClient = maklerApiClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Cabinet));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tokenResponse = await _maklerApiClient.LoginAsync(new ApiLoginRequest
        {
            Email = model.Email,
            Password = model.Password
        }, cancellationToken);

        if (tokenResponse is null)
        {
            ModelState.AddModelError(string.Empty, "Email və ya şifrə yanlışdır.");
            return View(model);
        }

        await SignInAsync(tokenResponse, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(Cabinet));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Cabinet));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tokenResponse = await _maklerApiClient.RegisterAsync(new ApiRegisterRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Password = model.Password
        }, cancellationToken);

        if (tokenResponse is null)
        {
            ModelState.AddModelError(string.Empty, "Qeydiyyat uğursuz oldu. Yenidən cəhd edin.");
            return View(model);
        }

        await SignInAsync(tokenResponse, isPersistent: false);
        await _maklerApiClient.RequestOtpAsync(model.Email, cancellationToken);

        TempData["SuccessMessage"] = "OTP kodu göndərildi. Hesabı təsdiqləyin.";
        return RedirectToAction(nameof(VerifyOtp), new { emailOrPhone = model.Email });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyOtp(string? emailOrPhone = null)
    {
        return View(new VerifyOtpViewModel
        {
            EmailOrPhone = emailOrPhone ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var verified = await _maklerApiClient.VerifyOtpAsync(new ApiVerifyOtpRequest
        {
            EmailOrPhone = model.EmailOrPhone,
            Code = model.Code
        }, cancellationToken);

        if (!verified)
        {
            ModelState.AddModelError(nameof(model.Code), "Kod yanlışdır və ya vaxtı bitib.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Hesab uğurla təsdiqləndi.";
        return RedirectToAction(nameof(Cabinet));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Cabinet(CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        IReadOnlyList<ApiPaymentHistoryItem> paymentHistory;
        try
        {
            paymentHistory = await _maklerApiClient.GetPaymentHistoryAsync(accessToken, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            var refreshed = await TryRefreshSessionAsync(cancellationToken);
            if (!refreshed)
            {
                await SignOutAsync(cancellationToken);
                return RedirectToAction(nameof(Login));
            }

            accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await SignOutAsync(cancellationToken);
                return RedirectToAction(nameof(Login));
            }

            paymentHistory = await _maklerApiClient.GetPaymentHistoryAsync(accessToken, cancellationToken);
        }

        return View(new CabinetViewModel
        {
            PaymentHistory = paymentHistory
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyListings(int page = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0 || pageSize > 24)
        {
            pageSize = 12;
        }

        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var apiResult = await ExecuteWithRefreshAsync(
            (token, ct) => _maklerApiClient.GetMyListingsAsync(token, page, pageSize, ct),
            accessToken,
            cancellationToken);

        if (apiResult is null)
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var items = apiResult.Items
            .Select(x => ToDashboardItem(x, Url.Action("Details", "Listings", new { id = x.Id }) ?? "#"))
            .ToList();

        var vm = new MyListingsViewModel
        {
            Items = items,
            TotalCount = apiResult.TotalCount,
            Page = apiResult.Page,
            PageSize = apiResult.PageSize
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await SignOutAsync(cancellationToken);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(ApiTokenResponse tokenResponse, bool isPersistent)
    {
        var claims = ReadClaims(tokenResponse.AccessToken);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = tokenResponse.AccessTokenExpiresAt
            });

        HttpContext.Session.SetString(AuthSessionKeys.AccessToken, tokenResponse.AccessToken);
        HttpContext.Session.SetString(AuthSessionKeys.RefreshToken, tokenResponse.RefreshToken);
    }

    private async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = HttpContext.Session.GetString(AuthSessionKeys.RefreshToken);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _maklerApiClient.LogoutAsync(refreshToken, cancellationToken);
        }

        HttpContext.Session.Remove(AuthSessionKeys.AccessToken);
        HttpContext.Session.Remove(AuthSessionKeys.RefreshToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private async Task<string?> GetValidAccessTokenAsync(CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return accessToken;
        }

        var refreshed = await TryRefreshSessionAsync(cancellationToken);
        if (!refreshed)
        {
            return null;
        }

        return HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
    }

    private async Task<bool> TryRefreshSessionAsync(CancellationToken cancellationToken)
    {
        var refreshToken = HttpContext.Session.GetString(AuthSessionKeys.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var refreshed = await _maklerApiClient.RefreshAsync(refreshToken, cancellationToken);
        if (refreshed is null)
        {
            return false;
        }

        await SignInAsync(refreshed, isPersistent: true);
        return true;
    }

    private async Task<T?> ExecuteWithRefreshAsync<T>(
        Func<string, CancellationToken, Task<T>> operation,
        string accessToken,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            return await operation(accessToken, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            var refreshed = await TryRefreshSessionAsync(cancellationToken);
            if (!refreshed)
            {
                return null;
            }

            var newAccessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
            if (string.IsNullOrWhiteSpace(newAccessToken))
            {
                return null;
            }

            return await operation(newAccessToken, cancellationToken);
        }
    }

    private static DashboardListingItemViewModel ToDashboardItem(ApiListingSummary item, string detailsUrl)
    {
        return new DashboardListingItemViewModel
        {
            Id = item.Id,
            Title = string.IsNullOrWhiteSpace(item.DisplayTitle) ? "Elan" : item.DisplayTitle,
            Location = string.IsNullOrWhiteSpace(item.District) ? item.City : $"{item.City}, {item.District}",
            Price = item.Price,
            Currency = GetCurrencyCode(item.CurrencyType),
            StatusLabel = GetStatusLabel(item.Status),
            StatusClass = GetStatusClass(item.Status),
            IsFeatured = item.IsFeatured,
            PublishedAt = item.PublishedAt,
            DetailsUrl = detailsUrl
        };
    }

    private static string GetStatusLabel(int status)
    {
        return status switch
        {
            1 => "Pending",
            2 => "Approved",
            3 => "Rejected",
            _ => "Unknown"
        };
    }

    private static string GetStatusClass(int status)
    {
        return status switch
        {
            1 => "badge-soft-warning",
            2 => "badge-soft-success",
            3 => "badge-soft-danger",
            _ => "badge-soft-secondary"
        };
    }

    private static string GetCurrencyCode(int currencyType)
    {
        return currencyType switch
        {
            1 => "AZN",
            2 => "USD",
            3 => "EUR",
            _ => "AZN"
        };
    }

    private static IReadOnlyList<Claim> ReadClaims(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return Array.Empty<Claim>();
        }

        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return Array.Empty<Claim>();
        }

        var payloadJson = Base64UrlDecode(parts[1]);
        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;

        var sub = TryGetString(root, ClaimTypes.NameIdentifier) ?? TryGetString(root, "sub");
        var email = TryGetString(root, ClaimTypes.Email) ?? TryGetString(root, "email");
        var name = TryGetString(root, ClaimTypes.Name) ?? email ?? "User";
        var role = TryGetString(root, ClaimTypes.Role) ?? TryGetString(root, "role");

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(sub))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static string Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        output = output.PadRight(output.Length + (4 - output.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(output);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
