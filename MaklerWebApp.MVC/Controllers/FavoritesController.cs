using MaklerWebApp.MVC.Infrastructure;
using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.MVC.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IMaklerApiClient _maklerApiClient;

    public FavoritesController(IMaklerApiClient maklerApiClient)
    {
        _maklerApiClient = maklerApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        try
        {
            var favorites = await _maklerApiClient.GetFavoritesAsync(accessToken, cancellationToken);
            var model = new FavoritesIndexViewModel
            {
                Items = favorites.Select(x => new FavoriteItemViewModel
                {
                    ListingId = x.ListingId,
                    Title = string.IsNullOrWhiteSpace(x.Title) ? "Elan" : x.Title,
                    Location = string.IsNullOrWhiteSpace(x.District) ? x.City : $"{x.City}, {x.District}",
                    Price = x.Price,
                    AddedAt = x.AddedAt,
                    ImageUrl = string.IsNullOrWhiteSpace(x.CoverImageUrl)
                        ? "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80"
                        : x.CoverImageUrl
                }).ToList()
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int listingId, string? returnUrl, CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action("Details", "Listings", new { id = listingId }) });
        }

        try
        {
            _ = await _maklerApiClient.AddFavoriteAsync(accessToken, listingId, cancellationToken);
            TempData["SuccessMessage"] = "Elan seçilmişlərə əlavə edildi.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action("Details", "Listings", new { id = listingId }) });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int listingId, string? returnUrl, CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action(nameof(Index)) });
        }

        try
        {
            _ = await _maklerApiClient.RemoveFavoriteAsync(accessToken, listingId, cancellationToken);
            TempData["SuccessMessage"] = "Elan seçilmişlərdən silindi.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action(nameof(Index)) });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
