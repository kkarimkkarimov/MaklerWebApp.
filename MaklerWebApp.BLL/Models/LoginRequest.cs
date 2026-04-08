using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
