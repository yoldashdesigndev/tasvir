using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"tasvir-test-{Guid.NewGuid():N}.json");

    [Fact]
    public void Load_ValidJson_RestoresValues()
    {
        File.WriteAllText(_path, """
            {"Language":"tr","MaxWidth":720,"MaxHeight":1280,"MaxFps":30,"BitrateMbps":4,"AutoReconnect":false}
            """);
        var service = new SettingsService(_path);

        service.Load();

        Assert.Equal("tr", service.Current.Language);
        Assert.Equal(720, service.Current.MaxWidth);
        Assert.Equal(30, service.Current.MaxFps);
        Assert.Equal(4, service.Current.BitrateMbps);
        Assert.False(service.Current.AutoReconnect);
    }

    [Fact]
    public void Load_InvalidJson_FallsBackToDefaults()
    {
        File.WriteAllText(_path, "not-json");
        var service = new SettingsService(_path);

        service.Load();

        Assert.Equal("en", service.Current.Language);
        Assert.Equal(1080, service.Current.MaxWidth);
        Assert.False(string.IsNullOrEmpty(service.LastError));
    }

    [Fact]
    public void Update_PersistsAndRaisesSettingsChanged()
    {
        var service = new SettingsService(_path);
        service.Load();
        var raises = 0;
        service.SettingsChanged += (_, _) => raises++;

        service.Update(s => s.BitrateMbps = 16);

        Assert.Equal(1, raises);
        var reloaded = new SettingsService(_path);
        reloaded.Load();
        Assert.Equal(16, reloaded.Current.BitrateMbps);
    }

    [Fact]
    public void Update_RejectsNullCallback()
    {
        var service = new SettingsService(_path);
        Assert.Throws<ArgumentNullException>(() => service.Update(null!));
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
