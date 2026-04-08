using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Services.Api;
using MaklerWebApp.MVC.Services.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.MVC.Controllers;

public class ListingsController : Controller
{
    private readonly IMaklerApiClient _maklerApiClient;

    public ListingsController(IMaklerApiClient maklerApiClient)
    {
        _maklerApiClient = maklerApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListingSearchViewModel filters, CancellationToken cancellationToken)
    {
        if (filters.Page <= 0)
        {
            filters.Page = 1;
        }

        if (filters.PageSize <= 0 || filters.PageSize > 24)
        {
            filters.PageSize = 9;
        }

        var apiResult = await _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
        {
            Keyword = filters.Keyword,
            City = filters.City,
            MinPrice = filters.MinPrice,
            MaxPrice = filters.MaxPrice,
            Page = filters.Page,
            PageSize = filters.PageSize,
            SortBy = filters.SortBy,
            Descending = filters.Descending
        }, cancellationToken);

        var cards = apiResult.Items.Select(x => new ListingCardViewModel
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
            DetailsUrl = Url.Action(nameof(Details), new { id = x.Id }) ?? "#"
        }).ToList();

        var vm = new ListingsIndexViewModel
        {
            Filters = filters,
            Items = cards,
            TotalCount = apiResult.TotalCount,
            Page = apiResult.Page,
            PageSize = apiResult.PageSize
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var listing = await _maklerApiClient.GetListingByIdAsync(id, cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        await _maklerApiClient.AddListingViewAsync(id, cancellationToken);

        var relatedSearchResult = await _maklerApiClient.SearchListingsAsync(new ApiListingSearchRequest
        {
            City = listing.City,
            Page = 1,
            PageSize = 4,
            SortBy = "published",
            Descending = true
        }, cancellationToken);

        var related = relatedSearchResult.Items
            .Where(x => x.Id != listing.Id)
            .Take(3)
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
                DetailsUrl = Url.Action(nameof(Details), new { id = x.Id }) ?? "#"
            })
            .ToList();

        var imageUrls = listing.Images
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .Select(x => x.ImageUrl)
            .ToList();

        if (imageUrls.Count == 0)
        {
            imageUrls.Add("https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80");
        }

        var model = new ListingDetailsViewModel
        {
            Id = listing.Id,
            Title = string.IsNullOrWhiteSpace(listing.DisplayTitle) ? "Elan" : listing.DisplayTitle,
            Description = listing.DisplayDescription,
            Price = listing.Price,
            Currency = GetCurrencyCode(listing.CurrencyType),
            Rooms = listing.Rooms,
            Area = listing.Area,
            City = listing.City,
            District = listing.District,
            Address = listing.Address,
            ContactName = listing.ContactName,
            ContactPhone = listing.ContactPhone,
            ViewCount = listing.ViewCount + 1,
            PublishedAt = listing.PublishedAt,
            IsFeatured = listing.IsFeatured,
            ImageUrls = imageUrls,
            RelatedListings = related
        };

        return View(model);
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
}
