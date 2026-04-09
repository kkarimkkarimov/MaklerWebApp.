using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IListingService
{
    Task<PagedResult<ListingDto>> SearchAsync(ListingSearchRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListingMapMarkerDto>> SearchMapMarkersAsync(ListingSearchRequest request, CancellationToken cancellationToken = default);
    Task<ListingDto?> GetByIdAsync(int id, string? languageCode, CancellationToken cancellationToken = default);
    Task<PagedResult<ListingDto>> GetByUserIdAsync(int userId, int page, int pageSize, string? languageCode, CancellationToken cancellationToken = default);
    Task<ListingDto> CreateAsync(CreateListingRequest request, int ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateListingRequest request, int ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, int ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> ModerateAsync(int id, ModerateListingRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetFeaturedAsync(int id, SetFeaturedRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetAdStatusAsync(int id, PatchListingAdStatusRequest request, int ownerUserId, CancellationToken cancellationToken = default);
    Task AddViewAsync(int id, int? userId, CancellationToken cancellationToken = default);
    Task<bool> AddImagesAsync(int listingId, IReadOnlyList<string> imageUrls, int ownerUserId, CancellationToken cancellationToken = default);
    Task<string?> DeleteImageAsync(int listingId, int imageId, int ownerUserId, CancellationToken cancellationToken = default);
    Task<string?> ReplaceImageAsync(int listingId, int imageId, string newImageUrl, int ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> ReorderImagesAsync(int listingId, ListingImageReorderRequest request, int ownerUserId, CancellationToken cancellationToken = default);
}
