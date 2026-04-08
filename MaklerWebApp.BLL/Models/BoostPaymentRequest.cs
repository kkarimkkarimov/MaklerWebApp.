using MaklerWebApp.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class BoostPaymentRequest
{
    [Range(1, int.MaxValue)]
    public int ListingId { get; set; }
    public PaymentServiceType ServiceType { get; set; }

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal Amount { get; set; }
}
