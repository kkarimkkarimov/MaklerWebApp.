using MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.BLL.Models;

public class UpdateListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }
    public PropertyType PropertyType { get; set; }
    public ListingType ListingType { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsNewBuilding { get; set; }
    public bool HasMortgage { get; set; }
    public bool IsFurnished { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<ListingImageInput> Images { get; set; } = new();
    public List<ListingTranslationInput> Translations { get; set; } = new();
}
