using MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.BLL.Models;

public class BoostPaymentRequest
{
    public int ListingId { get; set; }
    public PaymentServiceType ServiceType { get; set; }
    public decimal Amount { get; set; }
}
