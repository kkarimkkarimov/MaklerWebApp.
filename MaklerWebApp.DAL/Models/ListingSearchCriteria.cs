using MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.DAL.Models;

public class ListingSearchCriteria
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public ListingType? ListingType { get; set; }
    public PropertyType? PropertyType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinArea { get; set; }
    public double? MaxArea { get; set; }
    public int? MinRooms { get; set; }
    public int? MaxRooms { get; set; }
    public bool? IsNewBuilding { get; set; }
    public bool? HasMortgage { get; set; }
    public bool? IsMortgageEligible { get; set; }
    public bool? IsFurnished { get; set; }
    public RepairStatus? RepairStatus { get; set; }
    public DocumentStatus? DocumentStatus { get; set; }
    public bool? IsFeatured { get; set; }
    public ListingStatus? Status { get; set; }
    public AdStatus? AdStatus { get; set; }
    public DateTime? PublishedFrom { get; set; }
    public DateTime? PublishedTo { get; set; }
    public bool? OnlyWithImages { get; set; }
    public bool IncludeDeleted { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
