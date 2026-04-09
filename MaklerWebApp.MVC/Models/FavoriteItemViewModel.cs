namespace MaklerWebApp.MVC.Models;

public class FavoriteItemViewModel
{
    public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
