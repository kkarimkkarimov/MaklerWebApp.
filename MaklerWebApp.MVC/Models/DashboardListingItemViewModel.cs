namespace MaklerWebApp.MVC.Models;

public class DashboardListingItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AZN";
    public string StatusLabel { get; set; } = "Pending";
    public string StatusClass { get; set; } = "badge-soft-warning";
    public bool IsFeatured { get; set; }
    public DateTime PublishedAt { get; set; }
    public string DetailsUrl { get; set; } = "#";
}
