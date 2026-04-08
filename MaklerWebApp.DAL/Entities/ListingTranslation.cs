namespace MaklerWebApp.DAL.Entities;

public class ListingTranslation
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Listing Listing { get; set; } = null!;
}
