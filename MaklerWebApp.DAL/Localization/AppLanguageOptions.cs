using System.Globalization;

namespace MaklerWebApp.DAL.Localization;

public static class AppLanguageOptions
{
    public const string DefaultLanguage = "az";

    public static readonly string[] SupportedLanguageCodes =
    {
        "az",
        "ru",
        "en"
    };

    private static readonly HashSet<string> SupportedCodesSet = new(SupportedLanguageCodes, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } = SupportedLanguageCodes
        .Select(code => new CultureInfo(code))
        .ToList();

    public static bool IsSupported(string? languageCode)
    {
        return !string.IsNullOrWhiteSpace(languageCode) && SupportedCodesSet.Contains(languageCode.Trim());
    }

    public static string NormalizeOrDefault(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguage;
        }

        var normalized = languageCode.Trim().ToLowerInvariant();
        return SupportedCodesSet.Contains(normalized) ? normalized : DefaultLanguage;
    }
}
