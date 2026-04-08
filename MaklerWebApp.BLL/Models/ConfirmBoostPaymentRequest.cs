using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class ConfirmBoostPaymentRequest
{
    [Required]
    [StringLength(64, MinimumLength = 16)]
    public string Reference { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal PaidAmount { get; set; }

    public bool Succeeded { get; set; }

    [StringLength(128)]
    public string? ProviderTransactionId { get; set; }
}
