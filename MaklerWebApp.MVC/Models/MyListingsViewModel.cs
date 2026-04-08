namespace MaklerWebApp.MVC.Models;

public class MyListingsViewModel
{
    public IReadOnlyList<DashboardListingItemViewModel> Items { get; set; } = Array.Empty<DashboardListingItemViewModel>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
