namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiListingSearchRequest
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = true;
}
