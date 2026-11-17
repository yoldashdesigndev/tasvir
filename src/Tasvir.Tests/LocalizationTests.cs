using Tasvir.Services;
using Tasvir.Services.Resources;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class LocalizationTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"tasvir-lang-{Guid.NewGuid():N}.json");

    [Fact]
    public void SetLanguage_Turkish_ChangesResourcesAndRaisesEvent()
    {
        var settings = new SettingsService(_path);
        var localization = new LocalizationService(settings);
        var raises = 0;
        localization.LanguageChanged += (_, _) => raises++;

        localization.SetLanguage("tr");

        Assert.Equal("tr", localization.Language);
        Assert.Equal(1, raises);
        Assert.Equal("Yansıtma", Strings.NavMirror);
        Assert.Equal("Telefonun. Masaüstün.", Strings.Tagline);
    }

    [Fact]
    public void SetLanguage_UnsupportedCode_FallsBackToEnglish()
    {
        var settings = new SettingsService(_path);
        var localization = new LocalizationService(settings);

        localization.SetLanguage("de-DE");

        Assert.Equal("en", localization.Language);
        Assert.Equal("Mirror", Strings.NavMirror);
    }

    [Fact]
    public void SetLanguage_SameCode_DoesNotRaiseEvent()
    {
        var settings = new SettingsService(_path);
        var localization = new LocalizationService(settings);
        var raises = 0;
        localization.LanguageChanged += (_, _) => raises++;

        localization.SetLanguage("en");

        Assert.Equal(0, raises);
    }

    public void Dispose()
    {
        new LocalizationService(new SettingsService(_path)).SetLanguage("en");
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
