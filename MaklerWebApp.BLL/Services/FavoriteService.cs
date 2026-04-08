using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaklerWebApp.BLL.Services;

public class FavoriteService : IFavoriteService
{
    private readonly MaklerDbContext _dbContext;

    public FavoriteService(MaklerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FavoriteDto>> GetMyFavoritesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favorites
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.Listing.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FavoriteDto
            {
                ListingId = x.ListingId,
                Title = x.Listing.Title,
                Price = x.Listing.Price,
                City = x.Listing.City,
                District = x.Listing.District,
                CoverImageUrl = x.Listing.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                AddedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AddAsync(int userId, int listingId, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Favorites.AnyAsync(x => x.UserId == userId && x.ListingId == listingId, cancellationToken);
        if (exists)
        {
            return true;
        }

        var listingExists = await _dbContext.Listings.AnyAsync(x => x.Id == listingId && !x.IsDeleted, cancellationToken);
        if (!listingExists)
        {
            return false;
        }

        _dbContext.Favorites.Add(new Favorite
        {
            UserId = userId,
            ListingId = listingId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(int userId, int listingId, CancellationToken cancellationToken = default)
    {
        var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.ListingId == listingId, cancellationToken);
        if (favorite is null)
        {
            return false;
        }

        _dbContext.Favorites.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
