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

        public async Task<IActionResult> Index([FromQuery] ListingSearchViewModel filters, CancellationToken cancellationToken)
        {
            var apiHealthy = await _maklerApiClient.IsHealthyAsync(cancellationToken);
            var publicListingCount = await _maklerApiClient.GetPublicListingCountAsync(cancellationToken);

            filters ??= new ListingSearchViewModel();
            if (filters.Page <= 0)
            {
                filters.Page = 1;
            }

            if (filters.PageSize <= 0 || filters.PageSize > 24)
            {
                filters.PageSize = 9;
            }

            var isSearchApplied = HasAnySearchFilter(filters);

            var residentialTask = _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
            {
                ListingType = 1,
                PropertyType = 1,
                Page = 1,
                PageSize = 8,
                SortBy = "published",
                Descending = true
            }, cancellationToken);

            var agencyTask = _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
            {
                ListingType = 2,
                Page = 1,
                PageSize = 8,
                SortBy = "published",
                Descending = true
            }, cancellationToken);

            var premiumTask = _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
            {
                IsFeatured = true,
                Page = 1,
                PageSize = 8,
                SortBy = "published",
                Descending = true
            }, cancellationToken);

            var latestTask = _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
            {
                Page = 1,
                PageSize = 24,
                SortBy = "published",
                Descending = true
            }, cancellationToken);

            Task<ApiPagedResult<ApiListingSummary>>? searchTask = null;
            if (isSearchApplied)
            {
                searchTask = _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
                {
                    Keyword = filters.Keyword,
                    City = filters.City,
                    District = filters.District,
                    ListingType = filters.ListingType,
                    PropertyType = filters.PropertyType,
                    MinPrice = filters.MinPrice,
                    MaxPrice = filters.MaxPrice,
                    MinArea = filters.MinArea,
                    MaxArea = filters.MaxArea,
                    MinRooms = filters.MinRooms,
                    MaxRooms = filters.MaxRooms,
                    IsNewBuilding = filters.IsNewBuilding,
                    HasMortgage = filters.HasMortgage,
                    IsMortgageEligible = filters.IsMortgageEligible,
                    IsFurnished = filters.IsFurnished,
                    RepairStatus = filters.RepairStatus,
                    DocumentStatus = filters.DocumentStatus,
                    IsFeatured = filters.IsFeatured,
                    AdStatus = filters.AdStatus,
                    OnlyWithImages = filters.OnlyWithImages,
                    Page = 1,
                    PageSize = 12,
                    SortBy = string.IsNullOrWhiteSpace(filters.SortBy) ? "published" : filters.SortBy,
                    Descending = filters.Descending
                }, cancellationToken);
            }

            var locationsTask = _maklerApiClient.GetAzerbaijanLocationsAsync(cancellationToken);

            var allTasks = new List<Task> { residentialTask, agencyTask, premiumTask, latestTask, locationsTask };
            if (searchTask is not null)
            {
                allTasks.Add(searchTask);
            }

            await Task.WhenAll(allTasks);

            var latest = latestTask.Result.Items;
            var residential = BuildSection(residentialTask.Result.Items, latest, 8);
            var agency = BuildSection(agencyTask.Result.Items, latest, 8);
            var premium = BuildSection(premiumTask.Result.Items, latest.Where(x => x.IsFeatured).ToList(), 8);

            if (premium.Count == 0)
            {
                premium = BuildSection(latest.Where(x => x.IsFeatured).ToList(), latest, 8);
            }

            var locations = locationsTask.Result
                .Select(x => new AzerbaijanLocationViewModel
                {
                    Name = x.Name,
                    Districts = x.Districts.Select(d => d.Name).ToList()
                })
                .ToList();

            var searchResults = searchTask?.Result.Items.Select(ToListingCard).ToList() ?? new List<ListingCardViewModel>();

            return View(new HomeIndexViewModel
            {
                ApiHealthy = apiHealthy,
                PublicListingCount = publicListingCount ?? 0,
                Search = new ListingSearchViewModel
                {
                    Keyword = filters.Keyword,
                    City = filters.City,
                    District = filters.District,
                    ListingType = filters.ListingType,
                    PropertyType = filters.PropertyType,
                    MinPrice = filters.MinPrice,
                    MaxPrice = filters.MaxPrice,
                    MinArea = filters.MinArea,
                    MaxArea = filters.MaxArea,
                    MinRooms = filters.MinRooms,
                    MaxRooms = filters.MaxRooms,
                    IsNewBuilding = filters.IsNewBuilding,
                    HasMortgage = filters.HasMortgage,
                    IsMortgageEligible = filters.IsMortgageEligible,
                    IsFurnished = filters.IsFurnished,
                    RepairStatus = filters.RepairStatus,
                    DocumentStatus = filters.DocumentStatus,
                    IsFeatured = filters.IsFeatured,
                    AdStatus = filters.AdStatus,
                    OnlyWithImages = filters.OnlyWithImages,
                    SortBy = string.IsNullOrWhiteSpace(filters.SortBy) ? "published" : filters.SortBy,
                    Descending = filters.Descending,
                    Page = 1,
                    PageSize = filters.PageSize
                },
                IsSearchApplied = isSearchApplied,
                SearchResults = searchResults,
                AzerbaijanLocations = locations,
                ResidentialComplexes = residential,
                AgencyListings = agency,
                PremiumListings = premium
            });
        }

        private static bool HasAnySearchFilter(ListingSearchViewModel filters)
        {
            return !string.IsNullOrWhiteSpace(filters.Keyword)
                   || !string.IsNullOrWhiteSpace(filters.City)
                   || !string.IsNullOrWhiteSpace(filters.District)
                   || filters.ListingType.HasValue
                   || filters.PropertyType.HasValue
                   || filters.MinPrice.HasValue
                   || filters.MaxPrice.HasValue
                   || filters.MinArea.HasValue
                   || filters.MaxArea.HasValue
                   || filters.MinRooms.HasValue
                   || filters.MaxRooms.HasValue
                   || filters.IsNewBuilding == true
                   || filters.HasMortgage == true
                   || filters.IsMortgageEligible == true
                   || filters.IsFurnished == true
                   || filters.RepairStatus.HasValue
                   || filters.DocumentStatus.HasValue
                   || filters.IsFeatured == true
                   || filters.AdStatus.HasValue
                   || filters.OnlyWithImages == true;
        }

        private List<ListingCardViewModel> BuildSection(IReadOnlyList<ApiListingSummary> primary, IReadOnlyList<ApiListingSummary> fallback, int take)
        {
            var result = primary.Take(take).Select(ToListingCard).ToList();
            if (result.Count >= take)
            {
                return result;
            }

            var existingIds = result.Select(x => x.Id).ToHashSet();
            foreach (var item in fallback)
            {
                if (existingIds.Contains(item.Id))
                {
                    continue;
                }

                result.Add(ToListingCard(item));
                existingIds.Add(item.Id);

                if (result.Count >= take)
                {
                    break;
                }
            }

            return result;
        }

        private ListingCardViewModel ToListingCard(ApiListingSummary x)
        {
            return new ListingCardViewModel
            {
                Id = x.Id,
                Title = string.IsNullOrWhiteSpace(x.DisplayTitle) ? "Elan" : x.DisplayTitle,
                Location = string.IsNullOrWhiteSpace(x.District) ? x.City : $"{x.City}, {x.District}",
                Price = x.Price,
                Currency = GetCurrencyCode(x.CurrencyType),
                Rooms = x.Rooms,
                Area = x.Area,
                IsFeatured = x.IsFeatured,
                ImageUrl = x.Images
                    .OrderByDescending(img => img.IsPrimary)
                    .ThenBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault() ?? "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80",
                DetailsUrl = Url.Action("Details", "Listings", new { id = x.Id }) ?? "#"
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
