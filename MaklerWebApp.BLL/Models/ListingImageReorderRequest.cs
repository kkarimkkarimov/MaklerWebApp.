namespace MaklerWebApp.BLL.Models;

public class ListingImageReorderRequest
{
    public List<ListingImageOrderItem> Items { get; set; } = new();
}

public class ListingImageOrderItem
{
    public int ImageId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
