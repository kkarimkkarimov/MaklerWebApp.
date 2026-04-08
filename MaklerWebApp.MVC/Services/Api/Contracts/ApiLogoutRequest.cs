namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiLogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
