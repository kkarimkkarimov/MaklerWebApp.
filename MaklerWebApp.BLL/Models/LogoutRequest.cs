using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class LogoutRequest
{
    [Required]
    [MinLength(20)]
    public string RefreshToken { get; set; } = string.Empty;
}
