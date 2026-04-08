namespace MaklerWebApp.BLL.Models;

public class JwtOptions
{
    public string Issuer { get; set; } = "MaklerWebApp";
    public string Audience { get; set; } = "MaklerWebApp.Client";
    public string SecretKey { get; set; } = "SUPER_SECRET_KEY_CHANGE_ME_1234567890";
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 30;
}
