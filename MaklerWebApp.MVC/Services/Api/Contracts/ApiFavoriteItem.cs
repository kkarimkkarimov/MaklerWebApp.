namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiFavoriteItem
{
    public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public DateTime AddedAt { get; set; }
}
