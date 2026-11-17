namespace Tasvir.Core.Services;

public interface ILocalizationService
{
    string Language { get; }

    IReadOnlyList<string> SupportedLanguages { get; }

    event EventHandler? LanguageChanged;

    void SetLanguage(string language);
}
