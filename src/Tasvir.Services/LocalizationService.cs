using System.Globalization;
using Tasvir.Core.Services;
using Tasvir.Services.Resources;

namespace Tasvir.Services;

public sealed class LocalizationService : ILocalizationService
{
    private static readonly IReadOnlyList<string> Languages = new[] { "en", "tr" };

    private readonly ISettingsService _settings;
    private string _language;

    public LocalizationService(ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _language = Normalize(settings.Current.Language);
        Apply(_language);
    }

    public string Language => _language;

    public IReadOnlyList<string> SupportedLanguages => Languages;

    public event EventHandler? LanguageChanged;

    public void SetLanguage(string language)
    {
        var normalized = Normalize(language);
        if (normalized == _language)
        {
            return;
        }

        _language = normalized;
        _settings.Update(s => s.Language = normalized);
        Apply(normalized);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string Normalize(string? language) =>
        language?.StartsWith("tr", StringComparison.OrdinalIgnoreCase) == true ? "tr" : "en";

    private static void Apply(string language)
    {
        var culture = CultureInfo.GetCultureInfo(language == "tr" ? "tr-TR" : "en-US");
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Strings.Culture = culture;
    }
}
