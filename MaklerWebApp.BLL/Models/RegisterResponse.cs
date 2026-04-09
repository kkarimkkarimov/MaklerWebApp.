namespace MaklerWebApp.BLL.Models;

public class RegisterResponse
{
    public bool RequiresOtpVerification { get; set; } = true;
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Message { get; set; } = "OTP code sent. Please verify to activate your account.";
}
