namespace MaklerWebApp.Tests.Unit.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsToken()
    {
        await using var dbContext = CreateDbContext();
        var otpDelivery = new TestOtpDeliveryService();
        var service = new AuthService(dbContext, Options.Create(CreateJwtOptions()), otpDelivery);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Passw0rd!",
            PhoneNumber = "+994501112233"
        });

        var user = await dbContext.Users.SingleAsync();
        var otp = await dbContext.UserOtpCodes.SingleAsync();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal("test@example.com", user.Email);
        Assert.False(user.IsVerified);
        Assert.StartsWith("PBKDF2$", user.PasswordHash, StringComparison.Ordinal);
        Assert.Null(otp.ConsumedAt);
        Assert.Equal("test@example.com", otpDelivery.LastDestination);
        Assert.Matches("^\\d{6}$", otpDelivery.LastCode ?? string.Empty);
    }

    [Fact]
    public async Task VerifyOtpAsync_ValidCode_VerifiesUser()
    {
        await using var dbContext = CreateDbContext();
        var otpDelivery = new TestOtpDeliveryService();
        var service = new AuthService(dbContext, Options.Create(CreateJwtOptions()), otpDelivery);

        await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Otp User",
            Email = "otp@example.com",
            Password = "Passw0rd!"
        });

        var verificationResult = await service.VerifyOtpAsync(new VerifyOtpRequest
        {
            EmailOrPhone = "otp@example.com",
            Code = otpDelivery.LastCode ?? string.Empty
        });

        var user = await dbContext.Users.SingleAsync();
        var otp = await dbContext.UserOtpCodes.SingleAsync();

        Assert.True(verificationResult);
        Assert.True(user.IsVerified);
        Assert.NotNull(otp.ConsumedAt);
    }

    private static MaklerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaklerDbContext>()
            .UseInMemoryDatabase($"auth-tests-{Guid.NewGuid()}")
            .Options;

        return new MaklerDbContext(options);
    }

    private static JwtOptions CreateJwtOptions()
    {
        return new JwtOptions
        {
            Issuer = "tests",
            Audience = "tests-client",
            SecretKey = "UnitTests.VeryStrongSecretKey.For.Jwt.12345",
            AccessTokenMinutes = 30,
            RefreshTokenDays = 7
        };
    }

    private sealed class TestOtpDeliveryService : IOtpDeliveryService
    {
        public string? LastDestination { get; private set; }
        public string? LastCode { get; private set; }

        public Task SendAsync(string destination, string code, CancellationToken cancellationToken = default)
        {
            LastDestination = destination;
            LastCode = code;
            return Task.CompletedTask;
        }
    }
}
