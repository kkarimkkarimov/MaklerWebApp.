namespace MaklerWebApp.DAL.Entities;

public class ListingView
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int? UserId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Listing Listing { get; set; } = null!;
}
