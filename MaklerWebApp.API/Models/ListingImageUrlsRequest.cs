using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.API.Models;

public class ListingImageUrlsRequest
{
    [Required]
    [MinLength(1)]
    public List<string> ImageUrls { get; set; } = new();
}
