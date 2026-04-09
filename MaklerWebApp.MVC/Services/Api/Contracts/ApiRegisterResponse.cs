namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiRegisterResponse
{
    public bool RequiresOtpVerification { get; set; }
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
