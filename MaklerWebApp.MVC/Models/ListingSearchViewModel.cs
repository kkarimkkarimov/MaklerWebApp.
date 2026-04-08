namespace MaklerWebApp.MVC.Models;

public class ListingSearchViewModel
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string? SortBy { get; set; } = "published";
    public bool Descending { get; set; } = true;
}
