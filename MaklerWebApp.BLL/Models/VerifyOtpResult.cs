namespace MaklerWebApp.BLL.Models;

public class VerifyOtpResult
{
    public bool Verified { get; set; }
    public TokenResponse? Token { get; set; }
}
