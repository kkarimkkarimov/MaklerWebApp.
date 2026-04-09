namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiVerifyOtpResponse
{
    public bool Verified { get; set; }
    public ApiTokenResponse? Token { get; set; }
}
