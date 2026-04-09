namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiUserProfile
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsVerified { get; set; }
}
