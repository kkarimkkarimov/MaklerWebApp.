namespace MaklerWebApp.MVC.Models;

public class ListingDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AZN";
    public int Rooms { get; set; }
    public double Area { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsFeatured { get; set; }
    public IReadOnlyList<string> ImageUrls { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ListingCardViewModel> RelatedListings { get; set; } = Array.Empty<ListingCardViewModel>();
}
