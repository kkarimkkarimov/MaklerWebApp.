namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiVerifyOtpRequest
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
