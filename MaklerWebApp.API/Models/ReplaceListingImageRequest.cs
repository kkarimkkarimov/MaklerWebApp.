using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.API.Models;

public class ReplaceListingImageRequest
{
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string NewImageUrl { get; set; } = string.Empty;
}
