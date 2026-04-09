namespace MaklerWebApp.MVC.Models;

public class HomeIndexViewModel
{
    public bool ApiHealthy { get; set; }
    public int PublicListingCount { get; set; }
    public ListingSearchViewModel Search { get; set; } = new();
    public bool IsSearchApplied { get; set; }
    public IReadOnlyList<ListingCardViewModel> SearchResults { get; set; } = Array.Empty<ListingCardViewModel>();
    public IReadOnlyList<AzerbaijanLocationViewModel> AzerbaijanLocations { get; set; } = Array.Empty<AzerbaijanLocationViewModel>();
    public IReadOnlyList<ListingCardViewModel> ResidentialComplexes { get; set; } = Array.Empty<ListingCardViewModel>();
    public IReadOnlyList<ListingCardViewModel> AgencyListings { get; set; } = Array.Empty<ListingCardViewModel>();
    public IReadOnlyList<ListingCardViewModel> PremiumListings { get; set; } = Array.Empty<ListingCardViewModel>();
}
