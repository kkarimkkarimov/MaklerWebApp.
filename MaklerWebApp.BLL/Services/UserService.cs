using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MaklerWebApp.BLL.Services;

public class UserService : IUserService
{
    private readonly MaklerDbContext _dbContext;

    public UserService(MaklerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto?> GetMeAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserProfileDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                ProfileImageUrl = x.ProfileImageUrl,
                IsVerified = x.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> UpdateMeAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.ProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl) ? null : request.ProfileImageUrl.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            IsVerified = user.IsVerified
        };
    }
}
