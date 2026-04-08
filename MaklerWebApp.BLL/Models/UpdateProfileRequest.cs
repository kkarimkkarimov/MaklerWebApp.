namespace MaklerWebApp.BLL.Models;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
}
