using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.MVC.Models;

public class VerifyOtpViewModel
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^\\d{6}$")]
    public string Code { get; set; } = string.Empty;
}
