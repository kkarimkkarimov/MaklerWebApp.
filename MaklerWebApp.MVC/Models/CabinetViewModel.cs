using MaklerWebApp.MVC.Services.Api.Contracts;

namespace MaklerWebApp.MVC.Models;

public class CabinetViewModel
{
    public IReadOnlyList<ApiPaymentHistoryItem> PaymentHistory { get; set; } = Array.Empty<ApiPaymentHistoryItem>();
}
