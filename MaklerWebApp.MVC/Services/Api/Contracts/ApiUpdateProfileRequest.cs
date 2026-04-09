namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiUpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
}
