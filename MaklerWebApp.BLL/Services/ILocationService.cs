using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface ILocationService
{
    IReadOnlyList<AzerbaijanCityDto> GetAzerbaijanCities();
    bool IsValidCityDistrict(string city, string district);
    bool IsKnownCity(string city);
    IReadOnlyList<string> GetCitySearchCandidates(string city);
    IReadOnlyList<string> GetDistrictSearchCandidates(string city, string district);
    bool TryResolveCoordinates(string city, string district, out double latitude, out double longitude);
}
