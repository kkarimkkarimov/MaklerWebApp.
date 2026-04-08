namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiPaymentHistoryItem
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int ServiceType { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
