namespace MaklerWebApp.MVC.Models;

public class HomeIndexViewModel
{
    public bool ApiHealthy { get; set; }
    public int PublicListingCount { get; set; }
    public IReadOnlyList<ListingCardViewModel> FeaturedListings { get; set; } = Array.Empty<ListingCardViewModel>();
}
