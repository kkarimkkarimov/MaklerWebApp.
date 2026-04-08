using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Infrastructure;
using MaklerWebApp.MVC.Services.Api;
using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MaklerWebApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMaklerApiClient _maklerApiClient;

        public HomeController(IMaklerApiClient maklerApiClient)
        {
            _maklerApiClient = maklerApiClient;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var apiHealthy = await _maklerApiClient.IsHealthyAsync(cancellationToken);
            var publicListingCount = await _maklerApiClient.GetPublicListingCountAsync(cancellationToken);

            IReadOnlyList<ApiListingSummary> source;
            var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    var myListings = await _maklerApiClient.GetMyListingsAsync(accessToken, 1, 6, cancellationToken);
                    source = myListings.Items;
                }
                catch (UnauthorizedAccessException)
                {
                    source = Array.Empty<ApiListingSummary>();
                }
            }
            else
            {
                source = Array.Empty<ApiListingSummary>();
            }

            if (source.Count == 0)
            {
                var latest = await _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
                {
                    Page = 1,
                    PageSize = 6,
                    SortBy = "published",
                    Descending = true
                }, cancellationToken);
                source = latest.Items;
            }

            var items = source
                .Take(6)
                .Select(x => new ListingCardViewModel
                {
                    Id = x.Id,
                    Title = string.IsNullOrWhiteSpace(x.DisplayTitle) ? "Elan" : x.DisplayTitle,
                    Location = string.IsNullOrWhiteSpace(x.District) ? x.City : $"{x.City}, {x.District}",
                    Price = x.Price,
                    Rooms = x.Rooms,
                    Area = x.Area,
                    IsFeatured = x.IsFeatured,
                    ImageUrl = x.Images
                        .OrderByDescending(img => img.IsPrimary)
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault() ?? "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80",
                    DetailsUrl = Url.Action("Details", "Listings", new { id = x.Id }) ?? "#"
                })
                .ToList();

            return View(new HomeIndexViewModel
            {
                ApiHealthy = apiHealthy,
                PublicListingCount = publicListingCount ?? 0,
                FeaturedListings = items
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
