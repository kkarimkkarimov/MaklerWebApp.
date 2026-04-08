using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class VerifyOtpRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;
}
