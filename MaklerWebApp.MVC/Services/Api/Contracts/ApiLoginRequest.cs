namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
