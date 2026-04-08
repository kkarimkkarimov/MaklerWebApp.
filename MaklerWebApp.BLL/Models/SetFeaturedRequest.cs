namespace MaklerWebApp.BLL.Models;

public class SetFeaturedRequest
{
    public bool IsFeatured { get; set; }
    public DateTime? FeaturedUntil { get; set; }
}
