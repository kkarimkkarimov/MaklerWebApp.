namespace MaklerWebApp.BLL.Models;

public class VerifyOtpRequest
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
