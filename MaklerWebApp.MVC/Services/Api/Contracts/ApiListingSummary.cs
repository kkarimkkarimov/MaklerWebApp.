namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiListingSummary
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public int Rooms { get; set; }
    public double Area { get; set; }
    public bool IsFeatured { get; set; }
    public IReadOnlyList<ApiListingImage> Images { get; set; } = Array.Empty<ApiListingImage>();
}
