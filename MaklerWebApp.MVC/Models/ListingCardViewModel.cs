namespace MaklerWebApp.MVC.Models;

public class ListingCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AZN";
    public int Rooms { get; set; }
    public double Area { get; set; }
    public bool IsFeatured { get; set; }
    public string ImageUrl { get; set; } = "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80";
}
