namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiCreateListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CurrencyType { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }
    public int PropertyType { get; set; }
    public int ListingType { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsNewBuilding { get; set; }
    public bool HasMortgage { get; set; }
    public bool IsMortgageEligible { get; set; }
    public bool IsFurnished { get; set; }
    public int RepairStatus { get; set; }
    public int DocumentStatus { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<ApiCreateListingImageInput> Images { get; set; } = new();
    public List<ApiCreateListingTranslationInput> Translations { get; set; } = new();
}

public class ApiCreateListingImageInput
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class ApiCreateListingTranslationInput
{
    public string LanguageCode { get; set; } = "az";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
