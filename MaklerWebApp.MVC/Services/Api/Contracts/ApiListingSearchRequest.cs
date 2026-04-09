namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiListingSearchRequest
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public int? ListingType { get; set; }
    public int? PropertyType { get; set; }
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
    public int? RepairStatus { get; set; }
    public int? DocumentStatus { get; set; }
    public bool? IsFeatured { get; set; }
    public int? AdStatus { get; set; }
    public bool? OnlyWithImages { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = true;
}
