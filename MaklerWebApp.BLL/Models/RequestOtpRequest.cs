using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class RequestOtpRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string EmailOrPhone { get; set; } = string.Empty;
}
