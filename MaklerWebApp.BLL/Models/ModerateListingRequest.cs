using MaklerWebApp.BLL.Contracts.Enums;

namespace MaklerWebApp.BLL.Models;

public class ModerateListingRequest
{
    public ListingStatus Status { get; set; }
    public string? ModerationNote { get; set; }
}
