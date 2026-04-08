namespace MaklerWebApp.DAL.Entities;

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public Listing Listing { get; set; } = null!;
}
