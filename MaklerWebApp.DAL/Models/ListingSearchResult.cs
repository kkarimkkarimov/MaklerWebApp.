using MaklerWebApp.DAL.Entities;

namespace MaklerWebApp.DAL.Models;

public class ListingSearchResult
{
    public IReadOnlyList<Listing> Items { get; set; } = Array.Empty<Listing>();
    public int TotalCount { get; set; }
}
