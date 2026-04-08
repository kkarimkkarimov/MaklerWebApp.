using MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.BLL.Models;

public class ListingDto
{
    public int Id { get; set; }
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
    public ListingStatus Status { get; set; }
    public string? ModerationNote { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? FeaturedUntil { get; set; }
    public AdStatus AdStatus { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string DisplayDescription { get; set; } = string.Empty;
    public List<ListingImageDto> Images { get; set; } = new();
    public List<ListingTranslationDto> Translations { get; set; } = new();
}
