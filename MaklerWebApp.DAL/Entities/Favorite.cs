namespace MaklerWebApp.DAL.Entities;

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
