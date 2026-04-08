using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Constants;
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
    private const string Pbkdf2HashPrefix = "PBKDF2";
    private const int Pbkdf2Iterations = 100_000;
    private const int Pbkdf2KeySize = 32;
    private const int SaltSize = 32;
    private const int OtpLifetimeMinutes = 5;
    private const int MaxOtpAttempts = 5;

    private readonly MaklerDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly IOtpDeliveryService _otpDeliveryService;

    public AuthService(MaklerDbContext dbContext, IOptions<JwtOptions> jwtOptions, IOtpDeliveryService otpDeliveryService)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _otpDeliveryService = otpDeliveryService;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ArgumentException("Email already exists.");
        }

        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        var hash = HashPassword(request.Password, salt);

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordSalt = salt,
            PasswordHash = hash,
            Role = UserRoles.User,
            IsVerified = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await CreateAndSendOtpAsync(user, cancellationToken);

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

        var isValidPassword = VerifyPassword(request.Password, user, out var needsRehash);
        if (!isValidPassword)
        {
            return null;
        }

        if (needsRehash)
        {
            var newSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
            user.PasswordSalt = newSalt;
            user.PasswordHash = HashPassword(request.Password, newSalt);
            await _dbContext.SaveChangesAsync(cancellationToken);
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

    public async Task RequestOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserByEmailOrPhoneAsync(request.EmailOrPhone, cancellationToken);
        if (user is null)
        {
            return;
        }

        await CreateAndSendOtpAsync(user, cancellationToken);
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserByEmailOrPhoneAsync(request.EmailOrPhone, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var otp = await _dbContext.UserOtpCodes
            .Where(x => x.UserId == user.Id && x.ConsumedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null || otp.ExpiresAt <= now || otp.FailedAttempts >= MaxOtpAttempts)
        {
            return false;
        }

        var isMatch = VerifyOtpCode(request.Code, otp.CodeSalt, otp.CodeHash);
        if (!isMatch)
        {
            otp.FailedAttempts++;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        otp.ConsumedAt = now;
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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? UserRoles.User : user.Role)
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
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            Pbkdf2KeySize);

        return $"{Pbkdf2HashPrefix}${Pbkdf2Iterations}${Convert.ToBase64String(hashBytes)}";
    }

    private static bool VerifyPassword(string password, AppUser user, out bool needsRehash)
    {
        needsRehash = false;

        if (user.PasswordHash.StartsWith($"{Pbkdf2HashPrefix}$", StringComparison.Ordinal))
        {
            var parts = user.PasswordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
            {
                return false;
            }

            var saltBytes = Convert.FromBase64String(user.PasswordSalt);
            var actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                iterations,
                HashAlgorithmName.SHA256,
                Pbkdf2KeySize);

            var expectedHashBytes = Convert.FromBase64String(parts[2]);
            return expectedHashBytes.Length == actualHashBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedHashBytes, actualHashBytes);
        }

        // Legacy fallback (SHA256(password+salt)) for existing users; upgraded on successful login.
        var input = Encoding.UTF8.GetBytes(password + user.PasswordSalt);
        var legacyHash = Convert.ToBase64String(SHA256.HashData(input));
        var isLegacyMatch = legacyHash == user.PasswordHash;
        needsRehash = isLegacyMatch;
        return isLegacyMatch;
    }

    private async Task<AppUser?> FindUserByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken)
    {
        var value = emailOrPhone.Trim();
        var normalizedEmail = value.ToLowerInvariant();

        return await _dbContext.Users.FirstOrDefaultAsync(
            x => x.Email == normalizedEmail || (x.PhoneNumber != null && x.PhoneNumber == value),
            cancellationToken);
    }

    private async Task CreateAndSendOtpAsync(AppUser user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        var hash = HashOtpCode(code, salt);

        var activeOtps = await _dbContext.UserOtpCodes
            .Where(x => x.UserId == user.Id && x.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var activeOtp in activeOtps)
        {
            activeOtp.ConsumedAt = now;
        }

        _dbContext.UserOtpCodes.Add(new UserOtpCode
        {
            UserId = user.Id,
            CodeSalt = salt,
            CodeHash = hash,
            ExpiresAt = now.AddMinutes(OtpLifetimeMinutes)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _otpDeliveryService.SendAsync(user.Email, code, cancellationToken);
    }

    private static string HashOtpCode(string code, string salt)
    {
        var input = Encoding.UTF8.GetBytes(code + salt);
        return Convert.ToBase64String(SHA256.HashData(input));
    }

    private static bool VerifyOtpCode(string code, string salt, string expectedHash)
    {
        var actualHash = HashOtpCode(code, salt);
        var expectedBytes = Convert.FromBase64String(expectedHash);
        var actualBytes = Convert.FromBase64String(actualHash);
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
