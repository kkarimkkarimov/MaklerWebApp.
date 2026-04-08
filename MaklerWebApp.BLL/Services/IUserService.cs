using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetMeAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> UpdateMeAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
