using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class ListingTranslationInput
{
    [Required]
    [RegularExpression("^(az|ru|en)$", ErrorMessage = "LanguageCode must be one of: az, ru, en.")]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
}
