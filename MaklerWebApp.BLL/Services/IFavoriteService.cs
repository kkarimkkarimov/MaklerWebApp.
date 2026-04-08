using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteDto>> GetMyFavoritesAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(int userId, int listingId, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(int userId, int listingId, CancellationToken cancellationToken = default);
}
