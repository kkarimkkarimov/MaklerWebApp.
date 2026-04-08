using MaklerWebApp.DAL.Entities;
using MaklerWebApp.DAL.Enums;
using MaklerWebApp.DAL.Models;

namespace MaklerWebApp.DAL.Repositories;

public interface IListingRepository
{
    Task<ListingSearchResult> SearchAsync(ListingSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<Listing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Listing> AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(int id, ListingStatus status, string? moderationNote, CancellationToken cancellationToken = default);
    Task<bool> SetFeaturedAsync(int id, bool isFeatured, DateTime? featuredUntil, CancellationToken cancellationToken = default);
}
