using MaklerWebApp.BLL.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace MaklerWebApp.BLL.Models;

public class CreateListingRequest
{
    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; set; }
    public CurrencyType CurrencyType { get; set; }

    [Range(1, 100000)]
    public double Area { get; set; }

    [Range(1, 100)]
    public int Rooms { get; set; }
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }
    public PropertyType PropertyType { get; set; }
    public ListingType ListingType { get; set; }
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string District { get; set; } = string.Empty;

    [Required]
    [StringLength(250, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;
    public bool IsNewBuilding { get; set; }
    public bool HasMortgage { get; set; }
    public bool IsMortgageEligible { get; set; }
    public bool IsFurnished { get; set; }
    public RepairStatus RepairStatus { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 7)]
    public string ContactPhone { get; set; } = string.Empty;
    public List<ListingImageInput> Images { get; set; } = new();
    public List<ListingTranslationInput> Translations { get; set; } = new();
}
