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

        ApiTokenResponse? tokenResponse;
        try
        {
            tokenResponse = await _maklerApiClient.LoginAsync(new ApiLoginRequest
            {
                Email = model.Email,
                Password = model.Password
            }, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (RequiresOtpVerification(ex.Message))
            {
                TempData["SuccessMessage"] = "OTP təsdiqi tələb olunur. Kod e-mail ünvanınıza yenidən göndərildi.";
                return RedirectToAction(nameof(VerifyOtp), new { emailOrPhone = model.Email });
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        if (tokenResponse is null)
        {
            ModelState.AddModelError(string.Empty, "Email və ya şifrə yanlışdır.");
            ViewData["AuthError"] = "Daxilolma alınmadı. Email/şifrəni yoxlayın və OTP təsdiqi gözləyən hesabdırsa yenidən OTP təsdiq edin.";
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

        ApiRegisterResponse? registerResponse;
        try
        {
            registerResponse = await _maklerApiClient.RegisterAsync(new ApiRegisterRequest
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            }, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        if (registerResponse is null)
        {
            ModelState.AddModelError(string.Empty, "Qeydiyyat uğursuz oldu. Yenidən cəhd edin.");
            return View(model);
        }

        TempData["SuccessMessage"] = string.IsNullOrWhiteSpace(registerResponse.Message)
            ? "OTP kodu göndərildi. Hesabı təsdiqləyin."
            : registerResponse.Message;
        var verificationTarget = string.IsNullOrWhiteSpace(registerResponse.EmailOrPhone)
            ? model.Email
            : registerResponse.EmailOrPhone;

        return RedirectToAction(nameof(VerifyOtp), new { emailOrPhone = verificationTarget });
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

        var tokenResponse = await _maklerApiClient.VerifyOtpAsync(new ApiVerifyOtpRequest
        {
            EmailOrPhone = model.EmailOrPhone,
            Code = model.Code
        }, cancellationToken);

        if (tokenResponse is null)
        {
            ModelState.AddModelError(nameof(model.Code), "Kod yanlışdır və ya vaxtı bitib.");
            return View(model);
        }

        await SignInAsync(tokenResponse, isPersistent: false);
        TempData["SuccessMessage"] = "Hesab uğurla təsdiqləndi.";
        return RedirectToAction(nameof(Cabinet));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string emailOrPhone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            TempData["ErrorMessage"] = "Email ünvanını daxil edin.";
            return RedirectToAction(nameof(VerifyOtp));
        }

        try
        {
            await _maklerApiClient.RequestOtpAsync(emailOrPhone, cancellationToken);
            TempData["SuccessMessage"] = "Yeni OTP kodu göndərildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(VerifyOtp), new { emailOrPhone });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Cabinet(CancellationToken cancellationToken)
    {
        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var paymentHistory = await ExecuteWithRefreshAsync(
            async (token, ct) =>
            {
                var items = await _maklerApiClient.GetPaymentHistoryAsync(token, ct);
                return items.ToList();
            },
            accessToken,
            cancellationToken);

        var myListings = await ExecuteWithRefreshAsync(
            async (token, ct) => await _maklerApiClient.GetMyListingsAsync(token, 1, 6, ct),
            accessToken,
            cancellationToken);

        if (paymentHistory is null || myListings is null)
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var recentListings = (myListings?.Items ?? Array.Empty<ApiListingSummary>())
            .Take(5)
            .Select(x => ToDashboardItem(x, Url.Action("Details", "Listings", new { id = x.Id }) ?? "#"))
            .ToList();

        return View(new CabinetViewModel
        {
            PaymentHistory = paymentHistory,
            RecentListings = recentListings,
            TotalListingsCount = myListings?.TotalCount ?? 0,
            FeaturedListingsCount = (myListings?.Items ?? Array.Empty<ApiListingSummary>()).Count(x => x.IsFeatured),
            TotalPaymentAmount = paymentHistory.Sum(x => x.Amount)
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var profile = await ExecuteWithRefreshAsync(
            (token, ct) => _maklerApiClient.GetMyProfileAsync(token, ct),
            accessToken,
            cancellationToken);

        if (profile is null)
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        return View(new ProfileViewModel
        {
            FullName = profile.FullName,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber,
            ProfileImageUrl = profile.ProfileImageUrl,
            IsVerified = profile.IsVerified
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var updated = await ExecuteWithRefreshAsync(
            (token, ct) => _maklerApiClient.UpdateMyProfileAsync(token, new ApiUpdateProfileRequest
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber,
                ProfileImageUrl = model.ProfileImageUrl
            }, ct),
            accessToken,
            cancellationToken);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Profil yenilənmədi. Yenidən cəhd edin.");
            return View(model);
        }

        model.Email = updated.Email;
        model.IsVerified = updated.IsVerified;
        TempData["SuccessMessage"] = "Profil uğurla yeniləndi.";
        return View(model);
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
            async (token, ct) => await _maklerApiClient.GetMyListingsAsync(token, page, pageSize, ct),
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
    public async Task<IActionResult> DeleteListing(int id, CancellationToken cancellationToken)
    {
        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var deleted = await ExecuteWithRefreshAsync(
            (Func<string, CancellationToken, Task<bool>>)((token, ct) => _maklerApiClient.DeleteListingAsync(token, id, ct)),
            accessToken,
            cancellationToken);

        TempData[deleted == true ? "SuccessMessage" : "ErrorMessage"] = deleted == true
            ? "Elan silindi."
            : "Elanı silmək mümkün olmadı.";

        return RedirectToAction(nameof(MyListings));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAdStatus(int id, int adStatus, CancellationToken cancellationToken)
    {
        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var updated = await ExecuteWithRefreshAsync(
            (Func<string, CancellationToken, Task<bool>>)((token, ct) => _maklerApiClient.SetListingAdStatusAsync(token, id, adStatus, ct)),
            accessToken,
            cancellationToken);

        TempData[updated == true ? "SuccessMessage" : "ErrorMessage"] = updated == true
            ? "Elanın statusu yeniləndi."
            : "Status yenilənmədi.";

        return RedirectToAction(nameof(MyListings));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartBoost(int id, int serviceType, CancellationToken cancellationToken)
    {
        var accessToken = await GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var payment = await ExecuteWithRefreshAsync(
            (token, ct) => _maklerApiClient.StartBoostPaymentAsync(token, new ApiBoostPaymentRequest
            {
                ListingId = id,
                ServiceType = serviceType
            }, ct),
            accessToken,
            cancellationToken);

        TempData[payment is not null ? "SuccessMessage" : "ErrorMessage"] = payment is not null
            ? $"Boost ödənişi yaradıldı. Ref: {payment.Reference}" : "Boost əməliyyatı alınmadı.";

        return RedirectToAction(nameof(MyListings));
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
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = tokenResponse.AccessTokenExpiresAt
        };

        authProperties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = tokenResponse.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = tokenResponse.RefreshToken }
        ]);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        HttpContext.Session.SetString(AuthSessionKeys.AccessToken, tokenResponse.AccessToken);
        HttpContext.Session.SetString(AuthSessionKeys.RefreshToken, tokenResponse.RefreshToken);
    }

    private async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = HttpContext.Session.GetString(AuthSessionKeys.RefreshToken)
            ?? await GetStoredTokenAsync("refresh_token");
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

        var storedAccessToken = await GetStoredTokenAsync("access_token");
        if (!string.IsNullOrWhiteSpace(storedAccessToken))
        {
            HttpContext.Session.SetString(AuthSessionKeys.AccessToken, storedAccessToken);
            return storedAccessToken;
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
        var refreshToken = HttpContext.Session.GetString(AuthSessionKeys.RefreshToken)
            ?? await GetStoredTokenAsync("refresh_token");
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

    private async Task<string?> GetStoredTokenAsync(string tokenName)
    {
        var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return authResult?.Properties?.GetTokenValue(tokenName);
    }

    private async Task<T?> ExecuteWithRefreshAsync<T>(
        Func<string, CancellationToken, Task<T?>> operation,
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

            try
            {
                return await operation(newAccessToken, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }

    private async Task<bool?> ExecuteWithRefreshAsync(
        Func<string, CancellationToken, Task<bool>> operation,
        string accessToken,
        CancellationToken cancellationToken)
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

            try
            {
                return await operation(newAccessToken, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
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

    private static bool RequiresOtpVerification(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("otp", StringComparison.OrdinalIgnoreCase)
               || message.Contains("verified", StringComparison.OrdinalIgnoreCase);
    }
}
