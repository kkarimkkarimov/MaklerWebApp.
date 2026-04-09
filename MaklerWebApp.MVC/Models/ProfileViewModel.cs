using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.MVC.Models;

public class ProfileViewModel
{
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    [Url]
    public string? ProfileImageUrl { get; set; }

    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
