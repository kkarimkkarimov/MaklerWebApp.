namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiListingSummary
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
}
