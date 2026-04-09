using MaklerWebApp.BLL.Models;
using System.Globalization;
using System.Text;

namespace MaklerWebApp.BLL.Services;

public class LocationService : ILocationService
{
    private static readonly IReadOnlyList<AzerbaijanCityDto> AzerbaijanCities =
    [
        new AzerbaijanCityDto
        {
            Name = "Bakı",
            Latitude = 40.409264,
            Longitude = 49.867092,
            Districts =
            [
                new AzerbaijanDistrictDto { Name = "Binəqədi", Latitude = 40.4711, Longitude = 49.8093 },
                new AzerbaijanDistrictDto { Name = "Nərimanov", Latitude = 40.4029, Longitude = 49.8759 },
                new AzerbaijanDistrictDto { Name = "Nəsimi", Latitude = 40.3824, Longitude = 49.8420 },
                new AzerbaijanDistrictDto { Name = "Nizami", Latitude = 40.4062, Longitude = 49.9306 },
                new AzerbaijanDistrictDto { Name = "Səbail", Latitude = 40.3044, Longitude = 49.8465 },
                new AzerbaijanDistrictDto { Name = "Xətai", Latitude = 40.3720, Longitude = 49.9536 },
                new AzerbaijanDistrictDto { Name = "Xəzər", Latitude = 40.4007, Longitude = 50.2391 },
                new AzerbaijanDistrictDto { Name = "Yasamal", Latitude = 40.3777, Longitude = 49.8108 },
                new AzerbaijanDistrictDto { Name = "Qaradağ", Latitude = 40.0361, Longitude = 49.6822 },
                new AzerbaijanDistrictDto { Name = "Sabunçu", Latitude = 40.4418, Longitude = 49.9480 },
                new AzerbaijanDistrictDto { Name = "Suraxanı", Latitude = 40.4140, Longitude = 49.9546 },
                new AzerbaijanDistrictDto { Name = "Pirallahı", Latitude = 40.4702, Longitude = 50.3242 }
            ]
        },
        new AzerbaijanCityDto { Name = "Gəncə", Latitude = 40.6828, Longitude = 46.3606, Districts = [ new AzerbaijanDistrictDto { Name = "Kəpəz", Latitude = 40.6995, Longitude = 46.3501 }, new AzerbaijanDistrictDto { Name = "Nizami", Latitude = 40.6784, Longitude = 46.3675 } ] },
        new AzerbaijanCityDto { Name = "Sumqayıt", Latitude = 40.5855, Longitude = 49.6317, Districts = [ new AzerbaijanDistrictDto { Name = "Sumqayıt", Latitude = 40.5855, Longitude = 49.6317 } ] },
        new AzerbaijanCityDto { Name = "Mingəçevir", Latitude = 40.7639, Longitude = 47.0595, Districts = [ new AzerbaijanDistrictDto { Name = "Mingəçevir", Latitude = 40.7639, Longitude = 47.0595 } ] },
        new AzerbaijanCityDto { Name = "Şirvan", Latitude = 39.9378, Longitude = 48.9290, Districts = [ new AzerbaijanDistrictDto { Name = "Şirvan", Latitude = 39.9378, Longitude = 48.9290 } ] },
        new AzerbaijanCityDto { Name = "Naftalan", Latitude = 40.5090, Longitude = 46.8189, Districts = [ new AzerbaijanDistrictDto { Name = "Naftalan", Latitude = 40.5090, Longitude = 46.8189 } ] },

        new AzerbaijanCityDto { Name = "Abşeron", Latitude = 40.4472, Longitude = 49.7458, Districts = [ new AzerbaijanDistrictDto { Name = "Abşeron", Latitude = 40.4472, Longitude = 49.7458 } ] },
        new AzerbaijanCityDto { Name = "Ağcabədi", Latitude = 40.0510, Longitude = 47.4594, Districts = [ new AzerbaijanDistrictDto { Name = "Ağcabədi", Latitude = 40.0510, Longitude = 47.4594 } ] },
        new AzerbaijanCityDto { Name = "Ağdaş", Latitude = 40.6469, Longitude = 47.4750, Districts = [ new AzerbaijanDistrictDto { Name = "Ağdaş", Latitude = 40.6469, Longitude = 47.4750 } ] },
        new AzerbaijanCityDto { Name = "Ağstafa", Latitude = 41.1189, Longitude = 45.4539, Districts = [ new AzerbaijanDistrictDto { Name = "Ağstafa", Latitude = 41.1189, Longitude = 45.4539 } ] },
        new AzerbaijanCityDto { Name = "Ağsu", Latitude = 40.5703, Longitude = 48.4009, Districts = [ new AzerbaijanDistrictDto { Name = "Ağsu", Latitude = 40.5703, Longitude = 48.4009 } ] },
        new AzerbaijanCityDto { Name = "Astara", Latitude = 38.4560, Longitude = 48.8742, Districts = [ new AzerbaijanDistrictDto { Name = "Astara", Latitude = 38.4560, Longitude = 48.8742 } ] },
        new AzerbaijanCityDto { Name = "Balakən", Latitude = 41.7263, Longitude = 46.4048, Districts = [ new AzerbaijanDistrictDto { Name = "Balakən", Latitude = 41.7263, Longitude = 46.4048 } ] },
        new AzerbaijanCityDto { Name = "Bərdə", Latitude = 40.3758, Longitude = 47.1260, Districts = [ new AzerbaijanDistrictDto { Name = "Bərdə", Latitude = 40.3758, Longitude = 47.1260 } ] },
        new AzerbaijanCityDto { Name = "Beyləqan", Latitude = 39.7756, Longitude = 47.6186, Districts = [ new AzerbaijanDistrictDto { Name = "Beyləqan", Latitude = 39.7756, Longitude = 47.6186 } ] },
        new AzerbaijanCityDto { Name = "Biləsuvar", Latitude = 39.4599, Longitude = 48.5508, Districts = [ new AzerbaijanDistrictDto { Name = "Biləsuvar", Latitude = 39.4599, Longitude = 48.5508 } ] },
        new AzerbaijanCityDto { Name = "Cəlilabad", Latitude = 39.2096, Longitude = 48.4919, Districts = [ new AzerbaijanDistrictDto { Name = "Cəlilabad", Latitude = 39.2096, Longitude = 48.4919 } ] },
        new AzerbaijanCityDto { Name = "Daşkəsən", Latitude = 40.5202, Longitude = 46.0780, Districts = [ new AzerbaijanDistrictDto { Name = "Daşkəsən", Latitude = 40.5202, Longitude = 46.0780 } ] },
        new AzerbaijanCityDto { Name = "Füzuli", Latitude = 39.6001, Longitude = 47.1453, Districts = [ new AzerbaijanDistrictDto { Name = "Füzuli", Latitude = 39.6001, Longitude = 47.1453 } ] },
        new AzerbaijanCityDto { Name = "Gədəbəy", Latitude = 40.5699, Longitude = 45.8123, Districts = [ new AzerbaijanDistrictDto { Name = "Gədəbəy", Latitude = 40.5699, Longitude = 45.8123 } ] },
        new AzerbaijanCityDto { Name = "Goranboy", Latitude = 40.6114, Longitude = 46.7897, Districts = [ new AzerbaijanDistrictDto { Name = "Goranboy", Latitude = 40.6114, Longitude = 46.7897 } ] },
        new AzerbaijanCityDto { Name = "Göyçay", Latitude = 40.6524, Longitude = 47.7414, Districts = [ new AzerbaijanDistrictDto { Name = "Göyçay", Latitude = 40.6524, Longitude = 47.7414 } ] },
        new AzerbaijanCityDto { Name = "Göygöl", Latitude = 40.5864, Longitude = 46.3189, Districts = [ new AzerbaijanDistrictDto { Name = "Göygöl", Latitude = 40.5864, Longitude = 46.3189 } ] },
        new AzerbaijanCityDto { Name = "Hacıqabul", Latitude = 40.0394, Longitude = 48.9364, Districts = [ new AzerbaijanDistrictDto { Name = "Hacıqabul", Latitude = 40.0394, Longitude = 48.9364 } ] },
        new AzerbaijanCityDto { Name = "İmişli", Latitude = 39.8709, Longitude = 48.0596, Districts = [ new AzerbaijanDistrictDto { Name = "İmişli", Latitude = 39.8709, Longitude = 48.0596 } ] },
        new AzerbaijanCityDto { Name = "İsmayıllı", Latitude = 40.7853, Longitude = 48.1516, Districts = [ new AzerbaijanDistrictDto { Name = "İsmayıllı", Latitude = 40.7853, Longitude = 48.1516 } ] },
        new AzerbaijanCityDto { Name = "Qax", Latitude = 41.4207, Longitude = 46.9214, Districts = [ new AzerbaijanDistrictDto { Name = "Qax", Latitude = 41.4207, Longitude = 46.9214 } ] },
        new AzerbaijanCityDto { Name = "Qazax", Latitude = 41.0925, Longitude = 45.3668, Districts = [ new AzerbaijanDistrictDto { Name = "Qazax", Latitude = 41.0925, Longitude = 45.3668 } ] },
        new AzerbaijanCityDto { Name = "Qəbələ", Latitude = 40.9982, Longitude = 47.8700, Districts = [ new AzerbaijanDistrictDto { Name = "Qəbələ", Latitude = 40.9982, Longitude = 47.8700 } ] },
        new AzerbaijanCityDto { Name = "Qobustan", Latitude = 40.5328, Longitude = 48.9288, Districts = [ new AzerbaijanDistrictDto { Name = "Qobustan", Latitude = 40.5328, Longitude = 48.9288 } ] },
        new AzerbaijanCityDto { Name = "Quba", Latitude = 41.3611, Longitude = 48.5134, Districts = [ new AzerbaijanDistrictDto { Name = "Quba", Latitude = 41.3611, Longitude = 48.5134 } ] },
        new AzerbaijanCityDto { Name = "Qusar", Latitude = 41.4267, Longitude = 48.4302, Districts = [ new AzerbaijanDistrictDto { Name = "Qusar", Latitude = 41.4267, Longitude = 48.4302 } ] },
        new AzerbaijanCityDto { Name = "Kürdəmir", Latitude = 40.3449, Longitude = 48.1508, Districts = [ new AzerbaijanDistrictDto { Name = "Kürdəmir", Latitude = 40.3449, Longitude = 48.1508 } ] },
        new AzerbaijanCityDto { Name = "Lerik", Latitude = 38.7746, Longitude = 48.4149, Districts = [ new AzerbaijanDistrictDto { Name = "Lerik", Latitude = 38.7746, Longitude = 48.4149 } ] },
        new AzerbaijanCityDto { Name = "Lənkəran", Latitude = 38.7543, Longitude = 48.8506, Districts = [ new AzerbaijanDistrictDto { Name = "Lənkəran", Latitude = 38.7543, Longitude = 48.8506 } ] },
        new AzerbaijanCityDto { Name = "Masallı", Latitude = 39.0343, Longitude = 48.6654, Districts = [ new AzerbaijanDistrictDto { Name = "Masallı", Latitude = 39.0343, Longitude = 48.6654 } ] },
        new AzerbaijanCityDto { Name = "Neftçala", Latitude = 39.3789, Longitude = 49.2470, Districts = [ new AzerbaijanDistrictDto { Name = "Neftçala", Latitude = 39.3789, Longitude = 49.2470 } ] },
        new AzerbaijanCityDto { Name = "Oğuz", Latitude = 41.0728, Longitude = 47.4653, Districts = [ new AzerbaijanDistrictDto { Name = "Oğuz", Latitude = 41.0728, Longitude = 47.4653 } ] },
        new AzerbaijanCityDto { Name = "Saatlı", Latitude = 39.9321, Longitude = 48.3695, Districts = [ new AzerbaijanDistrictDto { Name = "Saatlı", Latitude = 39.9321, Longitude = 48.3695 } ] },
        new AzerbaijanCityDto { Name = "Sabirabad", Latitude = 40.0087, Longitude = 48.4770, Districts = [ new AzerbaijanDistrictDto { Name = "Sabirabad", Latitude = 40.0087, Longitude = 48.4770 } ] },
        new AzerbaijanCityDto { Name = "Salyan", Latitude = 39.5962, Longitude = 48.9848, Districts = [ new AzerbaijanDistrictDto { Name = "Salyan", Latitude = 39.5962, Longitude = 48.9848 } ] },
        new AzerbaijanCityDto { Name = "Samux", Latitude = 40.7647, Longitude = 46.4084, Districts = [ new AzerbaijanDistrictDto { Name = "Samux", Latitude = 40.7647, Longitude = 46.4084 } ] },
        new AzerbaijanCityDto { Name = "Siyəzən", Latitude = 41.0781, Longitude = 49.1118, Districts = [ new AzerbaijanDistrictDto { Name = "Siyəzən", Latitude = 41.0781, Longitude = 49.1118 } ] },
        new AzerbaijanCityDto { Name = "Şabran", Latitude = 41.2224, Longitude = 48.9867, Districts = [ new AzerbaijanDistrictDto { Name = "Şabran", Latitude = 41.2224, Longitude = 48.9867 } ] },
        new AzerbaijanCityDto { Name = "Şamaxı", Latitude = 40.6314, Longitude = 48.6414, Districts = [ new AzerbaijanDistrictDto { Name = "Şamaxı", Latitude = 40.6314, Longitude = 48.6414 } ] },
        new AzerbaijanCityDto { Name = "Şəki", Latitude = 41.1919, Longitude = 47.1706, Districts = [ new AzerbaijanDistrictDto { Name = "Şəki", Latitude = 41.1919, Longitude = 47.1706 } ] },
        new AzerbaijanCityDto { Name = "Şəmkir", Latitude = 40.8296, Longitude = 46.0178, Districts = [ new AzerbaijanDistrictDto { Name = "Şəmkir", Latitude = 40.8296, Longitude = 46.0178 } ] },
        new AzerbaijanCityDto { Name = "Tərtər", Latitude = 40.3444, Longitude = 46.9284, Districts = [ new AzerbaijanDistrictDto { Name = "Tərtər", Latitude = 40.3444, Longitude = 46.9284 } ] },
        new AzerbaijanCityDto { Name = "Tovuz", Latitude = 40.9935, Longitude = 45.6284, Districts = [ new AzerbaijanDistrictDto { Name = "Tovuz", Latitude = 40.9935, Longitude = 45.6284 } ] },
        new AzerbaijanCityDto { Name = "Ucar", Latitude = 40.5190, Longitude = 47.6545, Districts = [ new AzerbaijanDistrictDto { Name = "Ucar", Latitude = 40.5190, Longitude = 47.6545 } ] },
        new AzerbaijanCityDto { Name = "Xaçmaz", Latitude = 41.4643, Longitude = 48.8060, Districts = [ new AzerbaijanDistrictDto { Name = "Xaçmaz", Latitude = 41.4643, Longitude = 48.8060 } ] },
        new AzerbaijanCityDto { Name = "Xızı", Latitude = 40.9105, Longitude = 49.0748, Districts = [ new AzerbaijanDistrictDto { Name = "Xızı", Latitude = 40.9105, Longitude = 49.0748 } ] },
        new AzerbaijanCityDto { Name = "Yardımlı", Latitude = 38.9054, Longitude = 48.2405, Districts = [ new AzerbaijanDistrictDto { Name = "Yardımlı", Latitude = 38.9054, Longitude = 48.2405 } ] },
        new AzerbaijanCityDto { Name = "Yevlax", Latitude = 40.6170, Longitude = 47.1501, Districts = [ new AzerbaijanDistrictDto { Name = "Yevlax", Latitude = 40.6170, Longitude = 47.1501 } ] },
        new AzerbaijanCityDto { Name = "Zaqatala", Latitude = 41.6336, Longitude = 46.6433, Districts = [ new AzerbaijanDistrictDto { Name = "Zaqatala", Latitude = 41.6336, Longitude = 46.6433 } ] },
        new AzerbaijanCityDto { Name = "Zərdab", Latitude = 40.2169, Longitude = 47.7123, Districts = [ new AzerbaijanDistrictDto { Name = "Zərdab", Latitude = 40.2169, Longitude = 47.7123 } ] },

        new AzerbaijanCityDto { Name = "Laçın", Latitude = 39.6400, Longitude = 46.5500, Districts = [ new AzerbaijanDistrictDto { Name = "Laçın", Latitude = 39.6400, Longitude = 46.5500 } ] },
        new AzerbaijanCityDto { Name = "Kəlbəcər", Latitude = 40.1024, Longitude = 46.0436, Districts = [ new AzerbaijanDistrictDto { Name = "Kəlbəcər", Latitude = 40.1024, Longitude = 46.0436 } ] },
        new AzerbaijanCityDto { Name = "Qubadlı", Latitude = 39.3440, Longitude = 46.5806, Districts = [ new AzerbaijanDistrictDto { Name = "Qubadlı", Latitude = 39.3440, Longitude = 46.5806 } ] },
        new AzerbaijanCityDto { Name = "Zəngilan", Latitude = 39.0876, Longitude = 46.6520, Districts = [ new AzerbaijanDistrictDto { Name = "Zəngilan", Latitude = 39.0876, Longitude = 46.6520 } ] },
        new AzerbaijanCityDto { Name = "Cəbrayıl", Latitude = 39.3987, Longitude = 47.0285, Districts = [ new AzerbaijanDistrictDto { Name = "Cəbrayıl", Latitude = 39.3987, Longitude = 47.0285 } ] },
        new AzerbaijanCityDto { Name = "Xocavənd", Latitude = 39.7900, Longitude = 47.1150, Districts = [ new AzerbaijanDistrictDto { Name = "Xocavənd", Latitude = 39.7900, Longitude = 47.1150 } ] },
        new AzerbaijanCityDto { Name = "Xocalı", Latitude = 39.9125, Longitude = 46.7903, Districts = [ new AzerbaijanDistrictDto { Name = "Xocalı", Latitude = 39.9125, Longitude = 46.7903 } ] },
        new AzerbaijanCityDto { Name = "Şuşa", Latitude = 39.7601, Longitude = 46.7499, Districts = [ new AzerbaijanDistrictDto { Name = "Şuşa", Latitude = 39.7601, Longitude = 46.7499 } ] },
        new AzerbaijanCityDto { Name = "Ağdam", Latitude = 39.9937, Longitude = 46.9289, Districts = [ new AzerbaijanDistrictDto { Name = "Ağdam", Latitude = 39.9937, Longitude = 46.9289 } ] },

        new AzerbaijanCityDto { Name = "Naxçıvan", Latitude = 39.2092, Longitude = 45.4122, Districts = [ new AzerbaijanDistrictDto { Name = "Naxçıvan", Latitude = 39.2092, Longitude = 45.4122 } ] },
        new AzerbaijanCityDto { Name = "Babək", Latitude = 39.1503, Longitude = 45.4486, Districts = [ new AzerbaijanDistrictDto { Name = "Babək", Latitude = 39.1503, Longitude = 45.4486 } ] },
        new AzerbaijanCityDto { Name = "Culfa", Latitude = 38.9533, Longitude = 45.6308, Districts = [ new AzerbaijanDistrictDto { Name = "Culfa", Latitude = 38.9533, Longitude = 45.6308 } ] },
        new AzerbaijanCityDto { Name = "Ordubad", Latitude = 38.9096, Longitude = 46.0227, Districts = [ new AzerbaijanDistrictDto { Name = "Ordubad", Latitude = 38.9096, Longitude = 46.0227 } ] },
        new AzerbaijanCityDto { Name = "Şahbuz", Latitude = 39.4087, Longitude = 45.5737, Districts = [ new AzerbaijanDistrictDto { Name = "Şahbuz", Latitude = 39.4087, Longitude = 45.5737 } ] },
        new AzerbaijanCityDto { Name = "Şərur", Latitude = 39.5536, Longitude = 44.9795, Districts = [ new AzerbaijanDistrictDto { Name = "Şərur", Latitude = 39.5536, Longitude = 44.9795 } ] },
        new AzerbaijanCityDto { Name = "Sədərək", Latitude = 39.7140, Longitude = 44.8849, Districts = [ new AzerbaijanDistrictDto { Name = "Sədərək", Latitude = 39.7140, Longitude = 44.8849 } ] },
        new AzerbaijanCityDto { Name = "Kəngərli", Latitude = 39.3886, Longitude = 45.1630, Districts = [ new AzerbaijanDistrictDto { Name = "Kəngərli", Latitude = 39.3886, Longitude = 45.1630 } ] }
    ];

    private static readonly Dictionary<string, string[]> CityAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bakı"] = ["Baki", "Baku"],
        ["Gəncə"] = ["Ganca", "Ganja"],
        ["Şəki"] = ["Sheki", "Seki"],
        ["Şamaxı"] = ["Shamaxi", "Samaxi"],
        ["Qəbələ"] = ["Gabala", "Qabala"],
        ["Qusar"] = ["Gusar"],
        ["Quba"] = ["Guba"],
        ["Lənkəran"] = ["Lenkeran", "Lankaran"],
        ["Göygöl"] = ["Goygol"],
        ["Siyəzən"] = ["Siazan", "Siyazan"]
    };

    private static readonly Dictionary<string, AzerbaijanCityDto> CityIndex = BuildCityIndex();

    public IReadOnlyList<AzerbaijanCityDto> GetAzerbaijanCities() => AzerbaijanCities;

    public bool IsValidCityDistrict(string city, string district)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(district))
        {
            return false;
        }

        var cityMatch = ResolveCity(city);
        if (cityMatch is null)
        {
            return false;
        }

        var normalizedDistrict = Normalize(district);
        return cityMatch.Districts.Any(x => Normalize(x.Name) == normalizedDistrict || Normalize(ToAscii(x.Name)) == normalizedDistrict);
    }

    public bool IsKnownCity(string city)
    {
        return ResolveCity(city) is not null;
    }

    public IReadOnlyList<string> GetCitySearchCandidates(string city)
    {
        var cityMatch = ResolveCity(city);
        if (cityMatch is null)
        {
            return Array.Empty<string>();
        }

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            cityMatch.Name,
            ToAscii(cityMatch.Name),
            city.Trim()
        };

        if (CityAliases.TryGetValue(cityMatch.Name, out var aliases))
        {
            foreach (var alias in aliases)
            {
                set.Add(alias);
            }
        }

        return set.ToList();
    }

    public IReadOnlyList<string> GetDistrictSearchCandidates(string city, string district)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            return Array.Empty<string>();
        }

        var cityMatch = ResolveCity(city);
        if (cityMatch is null)
        {
            return Array.Empty<string>();
        }

        var normalizedDistrict = Normalize(district);
        var districtMatch = cityMatch.Districts.FirstOrDefault(x => Normalize(x.Name) == normalizedDistrict || Normalize(ToAscii(x.Name)) == normalizedDistrict);
        if (districtMatch is null)
        {
            return Array.Empty<string>();
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            districtMatch.Name,
            ToAscii(districtMatch.Name),
            district.Trim()
        }.ToList();
    }

    public bool TryResolveCoordinates(string city, string district, out double latitude, out double longitude)
    {
        latitude = 40.409264;
        longitude = 49.867092;

        var cityMatch = ResolveCity(city);
        if (cityMatch is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            var normalizedDistrict = Normalize(district);
            var districtMatch = cityMatch.Districts.FirstOrDefault(x => Normalize(x.Name) == normalizedDistrict || Normalize(ToAscii(x.Name)) == normalizedDistrict);
            if (districtMatch is not null)
            {
                latitude = districtMatch.Latitude;
                longitude = districtMatch.Longitude;
                return true;
            }
        }

        latitude = cityMatch.Latitude;
        longitude = cityMatch.Longitude;
        return true;
    }

    private static AzerbaijanCityDto? ResolveCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return null;
        }

        var normalized = Normalize(city);
        return CityIndex.GetValueOrDefault(normalized);
    }

    private static Dictionary<string, AzerbaijanCityDto> BuildCityIndex()
    {
        var map = new Dictionary<string, AzerbaijanCityDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var city in AzerbaijanCities)
        {
            map[Normalize(city.Name)] = city;
            map[Normalize(ToAscii(city.Name))] = city;

            if (CityAliases.TryGetValue(city.Name, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    map[Normalize(alias)] = city;
                }
            }
        }

        return map;
    }

    private static string Normalize(string value)
    {
        var ascii = ToAscii(value).Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(ascii.Length);
        foreach (var c in ascii)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    private static string ToAscii(string value)
    {
        return value
            .Replace('ə', 'e').Replace('Ə', 'E')
            .Replace('ş', 's').Replace('Ş', 'S')
            .Replace('ç', 'c').Replace('Ç', 'C')
            .Replace('ğ', 'g'). Replace('Ğ', 'G')
            .Replace('ı', 'i').Replace('İ', 'I')
            .Replace('ö', 'o').Replace('Ö', 'O')
            .Replace('ü', 'u').Replace('Ü', 'U')
            .Trim();
    }
}
