using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using MaklerWebApp.DAL.Enums;
using MaklerWebApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MaklerWebApp.DAL.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly MaklerDbContext _dbContext;

    public ListingRepository(MaklerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListingSearchResult> SearchAsync(ListingSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Listings
            .AsNoTracking()
            .Include(x => x.Images)
            .Include(x => x.Translations)
            .AsQueryable();

        if (!criteria.IncludeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = $"%{criteria.Keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Title, keyword) ||
                EF.Functions.Like(x.Description, keyword) ||
                EF.Functions.Like(x.Address, keyword));
        }

        if (!string.IsNullOrWhiteSpace(criteria.City))
        {
            query = query.Where(x => x.City == criteria.City);
        }

        if (!string.IsNullOrWhiteSpace(criteria.District))
        {
            query = query.Where(x => x.District == criteria.District);
        }

        if (criteria.ListingType.HasValue)
        {
            query = query.Where(x => x.ListingType == criteria.ListingType.Value);
        }

        if (criteria.PropertyType.HasValue)
        {
            query = query.Where(x => x.PropertyType == criteria.PropertyType.Value);
        }

        if (criteria.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= criteria.MaxPrice.Value);
        }

        if (criteria.MinArea.HasValue)
        {
            query = query.Where(x => x.Area >= criteria.MinArea.Value);
        }

        if (criteria.MaxArea.HasValue)
        {
            query = query.Where(x => x.Area <= criteria.MaxArea.Value);
        }

        if (criteria.MinRooms.HasValue)
        {
            query = query.Where(x => x.Rooms >= criteria.MinRooms.Value);
        }

        if (criteria.MaxRooms.HasValue)
        {
            query = query.Where(x => x.Rooms <= criteria.MaxRooms.Value);
        }

        if (criteria.IsNewBuilding.HasValue)
        {
            query = query.Where(x => x.IsNewBuilding == criteria.IsNewBuilding.Value);
        }

        if (criteria.HasMortgage.HasValue)
        {
            query = query.Where(x => x.HasMortgage == criteria.HasMortgage.Value);
        }

        if (criteria.IsMortgageEligible.HasValue)
        {
            query = query.Where(x => x.IsMortgageEligible == criteria.IsMortgageEligible.Value);
        }

        if (criteria.IsFurnished.HasValue)
        {
            query = query.Where(x => x.IsFurnished == criteria.IsFurnished.Value);
        }

        if (criteria.RepairStatus.HasValue)
        {
            query = query.Where(x => x.RepairStatus == criteria.RepairStatus.Value);
        }

        if (criteria.DocumentStatus.HasValue)
        {
            query = query.Where(x => x.DocumentStatus == criteria.DocumentStatus.Value);
        }

        if (criteria.IsFeatured.HasValue)
        {
            if (criteria.IsFeatured.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(x => x.IsFeatured && (!x.FeaturedUntil.HasValue || x.FeaturedUntil >= now));
            }
            else
            {
                query = query.Where(x => !x.IsFeatured || (x.FeaturedUntil.HasValue && x.FeaturedUntil < DateTime.UtcNow));
            }
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(x => x.Status == criteria.Status.Value);
        }

        if (criteria.AdStatus.HasValue)
        {
            query = query.Where(x => x.AdStatus == criteria.AdStatus.Value);
        }

        if (criteria.PublishedFrom.HasValue)
        {
            query = query.Where(x => x.PublishedAt >= criteria.PublishedFrom.Value);
        }

        if (criteria.PublishedTo.HasValue)
        {
            query = query.Where(x => x.PublishedAt <= criteria.PublishedTo.Value);
        }

        if (criteria.OnlyWithImages.HasValue)
        {
            query = criteria.OnlyWithImages.Value
                ? query.Where(x => x.Images.Any())
                : query.Where(x => !x.Images.Any());
        }

        query = ApplySorting(query, criteria.SortBy, criteria.Descending);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (criteria.Page - 1) * criteria.PageSize;

        var items = await query
            .Skip(skip)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new ListingSearchResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Listing> ApplySorting(IQueryable<Listing> query, string? sortBy, bool descending)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "price" => descending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "area" => descending ? query.OrderByDescending(x => x.Area) : query.OrderBy(x => x.Area),
            "rooms" => descending ? query.OrderByDescending(x => x.Rooms) : query.OrderBy(x => x.Rooms),
            "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => descending
                ? query.OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.PublishedAt)
                : query.OrderByDescending(x => x.IsFeatured).ThenBy(x => x.PublishedAt)
        };
    }

    public Task<Listing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Listings
            .AsNoTracking()
            .Include(x => x.Images)
            .Include(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<Listing> AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _dbContext.Listings.Add(listing);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return listing;
    }

    public async Task<bool> UpdateAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings
            .Include(x => x.Images)
            .Include(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == listing.Id && !x.IsDeleted, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Title = listing.Title;
        existing.Description = listing.Description;
        existing.Price = listing.Price;
        existing.CurrencyType = listing.CurrencyType;
        existing.Area = listing.Area;
        existing.Rooms = listing.Rooms;
        existing.Floor = listing.Floor;
        existing.TotalFloors = listing.TotalFloors;
        existing.PropertyType = listing.PropertyType;
        existing.ListingType = listing.ListingType;
        existing.City = listing.City;
        existing.District = listing.District;
        existing.Address = listing.Address;
        existing.IsNewBuilding = listing.IsNewBuilding;
        existing.HasMortgage = listing.HasMortgage;
        existing.IsMortgageEligible = listing.IsMortgageEligible;
        existing.IsFurnished = listing.IsFurnished;
        existing.RepairStatus = listing.RepairStatus;
        existing.DocumentStatus = listing.DocumentStatus;
        existing.ContactName = listing.ContactName;
        existing.ContactPhone = listing.ContactPhone;
        existing.Status = listing.Status;
        existing.ModerationNote = listing.ModerationNote;
        existing.ModeratedAt = listing.ModeratedAt;

        existing.Images.Clear();
        foreach (var image in listing.Images)
        {
            existing.Images.Add(new ListingImage
            {
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                SortOrder = image.SortOrder
            });
        }

        existing.Translations.Clear();
        foreach (var translation in listing.Translations)
        {
            existing.Translations.Add(new ListingTranslation
            {
                LanguageCode = translation.LanguageCode,
                Title = translation.Title,
                Description = translation.Description
            });
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetStatusAsync(int id, ListingStatus status, string? moderationNote, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Status = status;
        existing.ModerationNote = string.IsNullOrWhiteSpace(moderationNote) ? null : moderationNote.Trim();
        existing.ModeratedAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        if (status == ListingStatus.Approved)
        {
            existing.PublishedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetFeaturedAsync(int id, bool isFeatured, DateTime? featuredUntil, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.IsFeatured = isFeatured;
        existing.FeaturedUntil = isFeatured ? featuredUntil : null;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
