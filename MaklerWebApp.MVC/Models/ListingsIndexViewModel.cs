namespace MaklerWebApp.MVC.Models;

public class ListingsIndexViewModel
{
    public ListingSearchViewModel Filters { get; set; } = new();
    public IReadOnlyList<ListingCardViewModel> Items { get; set; } = Array.Empty<ListingCardViewModel>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
