namespace MaklerWebApp.BLL.Models;

public class ListingMapMarkerDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CurrencyType { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
