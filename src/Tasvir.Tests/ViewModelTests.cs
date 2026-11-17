using Tasvir.Services;
using Tasvir.ViewModels;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class ViewModelTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"tasvir-vm-{Guid.NewGuid():N}.json");

    [Fact]
    public void Shell_Navigation_SwitchesCurrentViewModel()
    {
        using var shell = CreateShell();

        shell.NavigateSettingsCommand.Execute(null);
        Assert.IsType<SettingsViewModel>(shell.CurrentViewModel);
        Assert.True(shell.IsSettingsSelected);
        Assert.False(shell.IsMirrorSelected);

        shell.NavigateAboutCommand.Execute(null);
        Assert.IsType<AboutViewModel>(shell.CurrentViewModel);

        shell.NavigateMirrorCommand.Execute(null);
        Assert.IsType<MirrorViewModel>(shell.CurrentViewModel);
    }

    [Fact]
    public void LanguageChange_UpdatesAllViewModels()
    {
        using var shell = CreateShell();
        var mirror = Assert.IsType<MirrorViewModel>(shell.CurrentViewModel);
        shell.NavigateSettingsCommand.Execute(null);
        var settings = Assert.IsType<SettingsViewModel>(shell.CurrentViewModel);

        shell.ToggleLanguageCommand.Execute(null);

        Assert.Equal("Yansıtma", shell.NavMirror);
        Assert.Equal("Cihaz", mirror.MirrorTitle);
        Assert.Equal("Ayarlar", settings.Title);
    }

    [Fact]
    public void Mirror_ChangingBitrate_PersistsToSettings()
    {
        var settings = new SettingsService(_path);
        settings.Load();
        var localization = new LocalizationService(settings);
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), new FakeDeviceMonitor(), new FakeScrcpyService());

        mirror.SelectedBitrate = "16 Mbps";

        Assert.Equal(16, settings.Current.BitrateMbps);
    }

    private ShellViewModel CreateShell()
    {
        var settings = new SettingsService(_path);
        settings.Load();
        var localization = new LocalizationService(settings);
        return new ShellViewModel(
            localization,
            new MirrorViewModel(settings, localization, new FakeAdbService(), new FakeDeviceMonitor(), new FakeScrcpyService()),
            new SettingsViewModel(settings, localization),
            new AboutViewModel(localization));
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
