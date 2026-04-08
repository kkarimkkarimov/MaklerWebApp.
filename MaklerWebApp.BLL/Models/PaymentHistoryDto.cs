using MaklerWebApp.BLL.Contracts.Enums;

namespace MaklerWebApp.BLL.Models;

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public PaymentServiceType ServiceType { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
