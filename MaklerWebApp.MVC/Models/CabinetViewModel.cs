using MaklerWebApp.MVC.Services.Api.Contracts;

namespace MaklerWebApp.MVC.Models;

public class CabinetViewModel
{
    public IReadOnlyList<ApiPaymentHistoryItem> PaymentHistory { get; set; } = Array.Empty<ApiPaymentHistoryItem>();
    public IReadOnlyList<DashboardListingItemViewModel> RecentListings { get; set; } = Array.Empty<DashboardListingItemViewModel>();
    public int TotalListingsCount { get; set; }
    public int FeaturedListingsCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
}
