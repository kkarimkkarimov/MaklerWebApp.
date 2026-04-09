namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiListingDetails
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string DisplayDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CurrencyType { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public IReadOnlyList<ApiListingImage> Images { get; set; } = Array.Empty<ApiListingImage>();
}
