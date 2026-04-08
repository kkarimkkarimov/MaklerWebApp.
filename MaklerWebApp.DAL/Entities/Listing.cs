using MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.DAL.Entities;

public class Listing
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
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
    public bool IsMortgageEligible { get; set; }
    public bool IsFurnished { get; set; }
    public RepairStatus RepairStatus { get; set; } = RepairStatus.NoRepair;
    public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.NoDocument;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public ListingStatus Status { get; set; } = ListingStatus.Pending;
    public string? ModerationNote { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? FeaturedUntil { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AdStatus AdStatus { get; set; } = AdStatus.Active;
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<ListingTranslation> Translations { get; set; } = new List<ListingTranslation>();
    public ICollection<ListingView> Views { get; set; } = new List<ListingView>();
}
