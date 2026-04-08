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
            DetailsUrl = "#"
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
}
