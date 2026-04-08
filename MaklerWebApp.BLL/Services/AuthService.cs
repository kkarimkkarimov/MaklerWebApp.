using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MaklerWebApp.BLL.Services;

public class AuthService : IAuthService
{
    private readonly MaklerDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthService(MaklerDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ArgumentException("Email already exists.");
        }

        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = HashPassword(request.Password, salt);

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordSalt = salt,
            PasswordHash = hash,
            IsVerified = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await IssueTokenAsync(user, cancellationToken);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var hash = HashPassword(request.Password, user.PasswordSalt);
        if (hash != user.PasswordHash)
        {
            return null;
        }

        return await IssueTokenAsync(user, cancellationToken);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked, cancellationToken);

        if (tokenEntity is null || tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        tokenEntity.IsRevoked = true;
        return await IssueTokenAsync(tokenEntity.User, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked, cancellationToken);

        if (tokenEntity is null)
        {
            return;
        }

        tokenEntity.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var value = request.EmailOrPhone.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == value || (x.PhoneNumber != null && x.PhoneNumber == request.EmailOrPhone), cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (request.Code != "123456")
        {
            return false;
        }

        user.IsVerified = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<TokenResponse> IssueTokenAsync(AppUser user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessExpires = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: accessExpires,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessExpires
        };
    }

    private static string HashPassword(string password, string salt)
    {
        var input = Encoding.UTF8.GetBytes(password + salt);
        return Convert.ToBase64String(SHA256.HashData(input));
    }
}
