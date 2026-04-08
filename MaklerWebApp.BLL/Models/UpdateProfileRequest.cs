using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [Url]
    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }
}
