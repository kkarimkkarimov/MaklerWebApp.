using MaklerWebApp.BLL.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class BoostPaymentRequest
{
    [Range(1, int.MaxValue)]
    public int ListingId { get; set; }
    public PaymentServiceType ServiceType { get; set; }
}
