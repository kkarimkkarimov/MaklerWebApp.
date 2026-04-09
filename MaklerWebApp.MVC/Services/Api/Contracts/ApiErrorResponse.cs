namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
}
