namespace MaklerWebApp.MVC.Services.Api.Contracts;

public class ApiAzerbaijanLocation
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IReadOnlyList<ApiAzerbaijanDistrict> Districts { get; set; } = Array.Empty<ApiAzerbaijanDistrict>();
}

public class ApiAzerbaijanDistrict
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
