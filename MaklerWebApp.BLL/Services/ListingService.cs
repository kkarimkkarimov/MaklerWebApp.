using MaklerWebApp.BLL.Contracts.Enums;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using MaklerWebApp.DAL.Localization;
using MaklerWebApp.DAL.Models;
using MaklerWebApp.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

using DalEnums = MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.BLL.Services;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly MaklerDbContext _dbContext;

    public ListingService(IListingRepository listingRepository, MaklerDbContext dbContext)
    {
        _listingRepository = listingRepository;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ListingDto>> SearchAsync(ListingSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 100 ? 20 : request.PageSize;

        var criteria = new ListingSearchCriteria
        {
            Keyword = request.Keyword,
            City = request.City,
            District = request.District,
            ListingType = request.ListingType.HasValue ? (DalEnums.ListingType?)request.ListingType.Value : null,
            PropertyType = request.PropertyType.HasValue ? (DalEnums.PropertyType?)request.PropertyType.Value : null,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinArea = request.MinArea,
            MaxArea = request.MaxArea,
            MinRooms = request.MinRooms,
            MaxRooms = request.MaxRooms,
            IsNewBuilding = request.IsNewBuilding,
            HasMortgage = request.HasMortgage,
            IsMortgageEligible = request.IsMortgageEligible,
            IsFurnished = request.IsFurnished,
            RepairStatus = request.RepairStatus.HasValue ? (DalEnums.RepairStatus?)request.RepairStatus.Value : null,
            DocumentStatus = request.DocumentStatus.HasValue ? (DalEnums.DocumentStatus?)request.DocumentStatus.Value : null,
            IsFeatured = request.IsFeatured,
            Status = DalEnums.ListingStatus.Approved,
            AdStatus = request.AdStatus.HasValue ? (DalEnums.AdStatus?)request.AdStatus.Value : null,
            PublishedFrom = request.PublishedFrom,
            PublishedTo = request.PublishedTo,
            OnlyWithImages = request.OnlyWithImages,
            IncludeDeleted = false,
            SortBy = request.SortBy,
            Descending = request.Descending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _listingRepository.SearchAsync(criteria, cancellationToken);

        return new PagedResult<ListingDto>
        {
            Items = result.Items.Select(x => MapToDto(x, request.LanguageCode)).ToList(),
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ListingDto?> GetByIdAsync(int id, string? languageCode, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken);
        return listing is null ? null : MapToDto(listing, languageCode);
    }

    public async Task<PagedResult<ListingDto>> GetByUserIdAsync(int userId, int page, int pageSize, string? languageCode, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 || pageSize > 100 ? 20 : pageSize;

        var query = _dbContext.Listings
            .AsNoTracking()
            .Include(x => x.Images)
            .Include(x => x.Translations)
            .Where(x => x.OwnerUserId == userId && !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ListingDto>
        {
            Items = items.Select(x => MapToDto(x, languageCode)).ToList(),
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }

    public async Task<ListingDto> CreateAsync(CreateListingRequest request, int ownerUserId, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Title, request.Price, request.Area, request.Rooms, request.ContactPhone);

        var listing = new Listing
        {
            OwnerUserId = ownerUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            CurrencyType = (DalEnums.CurrencyType)request.CurrencyType,
            Area = request.Area,
            Rooms = request.Rooms,
            Floor = request.Floor,
            TotalFloors = request.TotalFloors,
            PropertyType = (DalEnums.PropertyType)request.PropertyType,
            ListingType = (DalEnums.ListingType)request.ListingType,
            City = request.City.Trim(),
            District = request.District.Trim(),
            Address = request.Address.Trim(),
            IsNewBuilding = request.IsNewBuilding,
            HasMortgage = request.HasMortgage,
            IsMortgageEligible = request.IsMortgageEligible,
            IsFurnished = request.IsFurnished,
            RepairStatus = (DalEnums.RepairStatus)request.RepairStatus,
            DocumentStatus = (DalEnums.DocumentStatus)request.DocumentStatus,
            ContactName = request.ContactName.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Status = DalEnums.ListingStatus.Pending,
            Images = request.Images
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .Select(x => new ListingImage
                {
                    ImageUrl = x.ImageUrl.Trim(),
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Translations = BuildTranslations(request.Translations)
        };

        var created = await _listingRepository.AddAsync(listing, cancellationToken);
        return MapToDto(created, null);
    }

    public async Task<bool> UpdateAsync(int id, UpdateListingRequest request, int ownerUserId, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Title, request.Price, request.Area, request.Rooms, request.ContactPhone);

        var existing = await _dbContext.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (existing is null || existing.OwnerUserId != ownerUserId)
        {
            return false;
        }

        var listing = new Listing
        {
            Id = id,
            OwnerUserId = ownerUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            CurrencyType = (DalEnums.CurrencyType)request.CurrencyType,
            Area = request.Area,
            Rooms = request.Rooms,
            Floor = request.Floor,
            TotalFloors = request.TotalFloors,
            PropertyType = (DalEnums.PropertyType)request.PropertyType,
            ListingType = (DalEnums.ListingType)request.ListingType,
            City = request.City.Trim(),
            District = request.District.Trim(),
            Address = request.Address.Trim(),
            IsNewBuilding = request.IsNewBuilding,
            HasMortgage = request.HasMortgage,
            IsMortgageEligible = request.IsMortgageEligible,
            IsFurnished = request.IsFurnished,
            RepairStatus = (DalEnums.RepairStatus)request.RepairStatus,
            DocumentStatus = (DalEnums.DocumentStatus)request.DocumentStatus,
            ContactName = request.ContactName.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Status = DalEnums.ListingStatus.Pending,
            ModerationNote = null,
            ModeratedAt = null,
            Images = request.Images
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .Select(x => new ListingImage
                {
                    ImageUrl = x.ImageUrl.Trim(),
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Translations = BuildTranslations(request.Translations)
        };

        return await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (existing is null || existing.OwnerUserId != ownerUserId)
        {
            return false;
        }

        return await _listingRepository.SoftDeleteAsync(id, cancellationToken);
    }

    public Task<bool> ModerateAsync(int id, ModerateListingRequest request, CancellationToken cancellationToken = default)
    {
        return _listingRepository.SetStatusAsync(id, (DalEnums.ListingStatus)request.Status, request.ModerationNote, cancellationToken);
    }

    public Task<bool> SetFeaturedAsync(int id, SetFeaturedRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IsFeatured && request.FeaturedUntil.HasValue && request.FeaturedUntil.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("FeaturedUntil must be in the future.");
        }

        return _listingRepository.SetFeaturedAsync(id, request.IsFeatured, request.FeaturedUntil, cancellationToken);
    }

    public async Task<bool> SetAdStatusAsync(int id, PatchListingAdStatusRequest request, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (existing is null || existing.OwnerUserId != ownerUserId)
        {
            return false;
        }

        existing.AdStatus = (DalEnums.AdStatus)request.AdStatus;
        existing.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task AddViewAsync(int id, int? userId, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (listing is null)
        {
            return;
        }

        listing.ViewCount += 1;
        _dbContext.ListingViews.Add(new ListingView
        {
            ListingId = id,
            UserId = userId,
            ViewedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AddImagesAsync(int listingId, IReadOnlyList<string> imageUrls, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == listingId && !x.IsDeleted, cancellationToken);

        if (listing is null || listing.OwnerUserId != ownerUserId)
        {
            return false;
        }

        var currentSort = listing.Images.Count == 0 ? 0 : listing.Images.Max(x => x.SortOrder) + 1;
        foreach (var imageUrl in imageUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            listing.Images.Add(new ListingImage
            {
                ImageUrl = imageUrl.Trim(),
                SortOrder = currentSort++,
                IsPrimary = listing.Images.Count == 0
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string?> DeleteImageAsync(int listingId, int imageId, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == listingId && !x.IsDeleted, cancellationToken);

        if (listing is null || listing.OwnerUserId != ownerUserId)
        {
            return null;
        }

        var image = listing.Images.FirstOrDefault(x => x.Id == imageId);
        if (image is null)
        {
            return null;
        }

        var removedUrl = image.ImageUrl;
        var wasPrimary = image.IsPrimary;

        listing.Images.Remove(image);

        if (wasPrimary)
        {
            var nextPrimary = listing.Images
                .OrderBy(x => x.SortOrder)
                .FirstOrDefault();

            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return removedUrl;
    }

    public async Task<string?> ReplaceImageAsync(int listingId, int imageId, string newImageUrl, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == listingId && !x.IsDeleted, cancellationToken);

        if (listing is null || listing.OwnerUserId != ownerUserId)
        {
            return null;
        }

        var image = listing.Images.FirstOrDefault(x => x.Id == imageId);
        if (image is null)
        {
            return null;
        }

        var oldUrl = image.ImageUrl;
        image.ImageUrl = newImageUrl.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return oldUrl;
    }

    public async Task<bool> ReorderImagesAsync(int listingId, ListingImageReorderRequest request, int ownerUserId, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == listingId && !x.IsDeleted, cancellationToken);

        if (listing is null || listing.OwnerUserId != ownerUserId)
        {
            return false;
        }

        var map = request.Items.ToDictionary(x => x.ImageId, x => x);
        foreach (var image in listing.Images)
        {
            if (!map.TryGetValue(image.Id, out var item))
            {
                continue;
            }

            image.SortOrder = item.SortOrder;
            image.IsPrimary = item.IsPrimary;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ListingDto MapToDto(Listing listing, string? languageCode)
    {
        var requestedLanguageCode = string.IsNullOrWhiteSpace(languageCode)
            ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            : languageCode;

        var normalizedLanguageCode = AppLanguageOptions.NormalizeOrDefault(requestedLanguageCode);

        var normalizedTranslations = listing.Translations
            .Select(x => new
            {
                LanguageCode = AppLanguageOptions.NormalizeOrDefault(x.LanguageCode),
                Translation = x
            })
            .ToList();

        var localized = normalizedTranslations
            .FirstOrDefault(x => x.LanguageCode == normalizedLanguageCode)?.Translation
            ?? normalizedTranslations.FirstOrDefault(x => x.LanguageCode == AppLanguageOptions.DefaultLanguage)?.Translation
            ?? listing.Translations.FirstOrDefault();

        return new ListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            CurrencyType = (CurrencyType)listing.CurrencyType,
            Area = listing.Area,
            Rooms = listing.Rooms,
            Floor = listing.Floor,
            TotalFloors = listing.TotalFloors,
            PropertyType = (PropertyType)listing.PropertyType,
            ListingType = (ListingType)listing.ListingType,
            City = listing.City,
            District = listing.District,
            Address = listing.Address,
            IsNewBuilding = listing.IsNewBuilding,
            HasMortgage = listing.HasMortgage,
            IsMortgageEligible = listing.IsMortgageEligible,
            IsFurnished = listing.IsFurnished,
            RepairStatus = (RepairStatus)listing.RepairStatus,
            DocumentStatus = (DocumentStatus)listing.DocumentStatus,
            ContactName = listing.ContactName,
            ContactPhone = listing.ContactPhone,
            Status = (ListingStatus)listing.Status,
            ModerationNote = listing.ModerationNote,
            ModeratedAt = listing.ModeratedAt,
            IsFeatured = listing.IsFeatured,
            FeaturedUntil = listing.FeaturedUntil,
            AdStatus = (AdStatus)listing.AdStatus,
            ViewCount = listing.ViewCount,
            PublishedAt = listing.PublishedAt,
            DisplayTitle = localized?.Title ?? listing.Title,
            DisplayDescription = localized?.Description ?? listing.Description,
            Images = listing.Images
                .OrderBy(x => x.SortOrder)
                .Select(x => new ListingImageDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Translations = listing.Translations
                .Select(x => new ListingTranslationDto
                {
                    LanguageCode = x.LanguageCode,
                    Title = x.Title,
                    Description = x.Description
                })
                .ToList()
        };
    }

    private static List<ListingTranslation> BuildTranslations(IEnumerable<ListingTranslationInput> translations)
    {
        return translations
            .Where(x => !string.IsNullOrWhiteSpace(x.LanguageCode) && !string.IsNullOrWhiteSpace(x.Title))
            .Select(x => new
            {
                LanguageCode = AppLanguageOptions.NormalizeOrDefault(x.LanguageCode),
                x.Title,
                x.Description
            })
            .Where(x => AppLanguageOptions.IsSupported(x.LanguageCode))
            .GroupBy(x => x.LanguageCode)
            .Select(x => x.First())
            .Select(x => new ListingTranslation
            {
                LanguageCode = x.LanguageCode,
                Title = x.Title.Trim(),
                Description = x.Description?.Trim() ?? string.Empty
            })
            .ToList();
    }

    private static void ValidateRequest(string title, decimal price, double area, int rooms, string contactPhone)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.");
        }

        if (area <= 0)
        {
            throw new ArgumentException("Area must be greater than zero.");
        }

        if (rooms <= 0)
        {
            throw new ArgumentException("Rooms must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(contactPhone))
        {
            throw new ArgumentException("Contact phone is required.");
        }
    }
}
