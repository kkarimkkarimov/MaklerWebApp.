namespace MaklerWebApp.MVC.Models;

public class MapListingMarkerViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Location { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DetailsUrl { get; set; } = "#";
}
