using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Services.Api;
using MaklerWebApp.MVC.Services.Api.Contracts;
using MaklerWebApp.MVC.Infrastructure;
using Microsoft.AspNetCore.Authorization;
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
            Page = filters.Page,
            PageSize = filters.PageSize,
            SortBy = string.IsNullOrWhiteSpace(filters.SortBy) ? "published" : filters.SortBy,
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
            Filters = new ListingSearchViewModel
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
                Page = apiResult.Page,
                PageSize = apiResult.PageSize,
                SortBy = string.IsNullOrWhiteSpace(filters.SortBy) ? "published" : filters.SortBy,
                Descending = filters.Descending
            },
            Items = cards,
            TotalCount = apiResult.TotalCount,
            Page = apiResult.Page,
            PageSize = apiResult.PageSize
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var listing = await _maklerApiClient.GetListingByIdAsync(id, cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        var model = new CreateListingViewModel
        {
            Title = listing.DisplayTitle,
            Description = listing.DisplayDescription,
            Price = listing.Price,
            CurrencyType = listing.CurrencyType,
            Area = listing.Area,
            Rooms = listing.Rooms,
            City = listing.City,
            District = listing.District,
            Address = listing.Address,
            ContactName = listing.ContactName,
            ContactPhone = listing.ContactPhone,
            PrimaryImageUrl = listing.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => x.ImageUrl).FirstOrDefault(),
            SecondaryImageUrl = listing.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => x.ImageUrl).Skip(1).FirstOrDefault()
        };

        ViewData["ListingId"] = id;
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateListingViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ListingId"] = id;
            return View(model);
        }

        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Edit), new { id }) });
        }

        var images = new List<ApiCreateListingImageInput>();
        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            images.Add(new ApiCreateListingImageInput { ImageUrl = model.PrimaryImageUrl.Trim(), IsPrimary = true, SortOrder = 0 });
        }

        if (!string.IsNullOrWhiteSpace(model.SecondaryImageUrl))
        {
            images.Add(new ApiCreateListingImageInput { ImageUrl = model.SecondaryImageUrl.Trim(), IsPrimary = images.Count == 0, SortOrder = 1 });
        }

        try
        {
            var updated = await _maklerApiClient.UpdateListingAsync(accessToken, id, new ApiCreateListingRequest
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                CurrencyType = model.CurrencyType,
                Area = model.Area,
                Rooms = model.Rooms,
                Floor = model.Floor,
                TotalFloors = model.TotalFloors,
                PropertyType = model.PropertyType,
                ListingType = model.ListingType,
                City = model.City.Trim(),
                District = model.District.Trim(),
                Address = model.Address.Trim(),
                IsNewBuilding = model.IsNewBuilding,
                HasMortgage = model.HasMortgage,
                IsMortgageEligible = model.IsMortgageEligible,
                IsFurnished = model.IsFurnished,
                RepairStatus = model.RepairStatus,
                DocumentStatus = model.DocumentStatus,
                ContactName = model.ContactName.Trim(),
                ContactPhone = model.ContactPhone.Trim(),
                Images = images,
                Translations =
                [
                    new ApiCreateListingTranslationInput
                    {
                        LanguageCode = "az",
                        Title = model.Title.Trim(),
                        Description = model.Description.Trim()
                    }
                ]
            }, cancellationToken);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Elan yenilənmədi. Yenidən cəhd edin.");
                ViewData["ListingId"] = id;
                return View(model);
            }

            TempData["SuccessMessage"] = "Elan uğurla yeniləndi.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Edit), new { id }) });
        }
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

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View(new CreateListingViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            return BadRequest(new { message = "Ən azı bir şəkil seçin." });
        }

        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized(new { message = "Sessiya bitib. Yenidən daxil olun." });
        }

        try
        {
            var uploadedUrls = await _maklerApiClient.UploadListingImagesAsync(accessToken, files.Take(2).ToList(), cancellationToken);
            if (uploadedUrls.Count == 0)
            {
                return BadRequest(new { message = "Şəkillər yüklənmədi. Yenidən cəhd edin." });
            }

            return Ok(new { imageUrls = uploadedUrls });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Sessiya bitib. Yenidən daxil olun." });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateListingViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accessToken = HttpContext.Session.GetString(AuthSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create)) });
        }

        var images = new List<ApiCreateListingImageInput>();
        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            images.Add(new ApiCreateListingImageInput { ImageUrl = model.PrimaryImageUrl.Trim(), IsPrimary = true, SortOrder = 0 });
        }

        if (!string.IsNullOrWhiteSpace(model.SecondaryImageUrl))
        {
            images.Add(new ApiCreateListingImageInput { ImageUrl = model.SecondaryImageUrl.Trim(), IsPrimary = images.Count == 0, SortOrder = 1 });
        }

        try
        {
            var created = await _maklerApiClient.CreateListingAsync(accessToken, new ApiCreateListingRequest
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                CurrencyType = model.CurrencyType,
                Area = model.Area,
                Rooms = model.Rooms,
                Floor = model.Floor,
                TotalFloors = model.TotalFloors,
                PropertyType = model.PropertyType,
                ListingType = model.ListingType,
                City = model.City.Trim(),
                District = model.District.Trim(),
                Address = model.Address.Trim(),
                IsNewBuilding = model.IsNewBuilding,
                HasMortgage = model.HasMortgage,
                IsMortgageEligible = model.IsMortgageEligible,
                IsFurnished = model.IsFurnished,
                RepairStatus = model.RepairStatus,
                DocumentStatus = model.DocumentStatus,
                ContactName = model.ContactName.Trim(),
                ContactPhone = model.ContactPhone.Trim(),
                Images = images,
                Translations =
                [
                    new ApiCreateListingTranslationInput
                    {
                        LanguageCode = "az",
                        Title = model.Title.Trim(),
                        Description = model.Description.Trim()
                    }
                ]
            }, cancellationToken);

            if (created is null)
            {
                ModelState.AddModelError(string.Empty, "Elan yaradılmadı. Məlumatları yoxlayıb yenidən cəhd edin.");
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create)) });
        }
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
