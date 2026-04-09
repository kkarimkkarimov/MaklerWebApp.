namespace MaklerWebApp.BLL.Models;

public class AzerbaijanCityDto
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IReadOnlyList<AzerbaijanDistrictDto> Districts { get; set; } = Array.Empty<AzerbaijanDistrictDto>();
}

public class AzerbaijanDistrictDto
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
