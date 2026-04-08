namespace MaklerWebApp.BLL.Models;

public class ListingImageInput
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
