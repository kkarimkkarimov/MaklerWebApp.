using System.Globalization;

namespace MaklerWebApp.MVC.Localization;

public static class AppLanguageOptions
{
    public const string DefaultLanguage = "az";

    public static readonly string[] SupportedLanguageCodes =
    {
        "az",
        "ru",
        "en"
    };

    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } = SupportedLanguageCodes
        .Select(code => new CultureInfo(code))
        .ToList();
}
