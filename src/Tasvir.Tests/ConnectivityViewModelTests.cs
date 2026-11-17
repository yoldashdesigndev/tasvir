using Tasvir.Core.Models;
using Tasvir.Services;
using Tasvir.Services.Resources;
using Tasvir.ViewModels;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class ConnectivityViewModelTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"tasvir-connect-{Guid.NewGuid():N}.json");

    [Fact]
    public void Mirror_DevicesChanged_UpdatesDeviceAndStatus()
    {
        var (settings, localization) = CreateSettings();
        var monitor = new FakeDeviceMonitor();
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), monitor, new FakeScrcpyService());

        monitor.Devices = new[]
        {
            new DeviceInfo("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb)
        };
        monitor.RaiseChanged();

        Assert.Equal("Pixel 7", mirror.DeviceName);
        Assert.Equal("ABC123", mirror.DeviceSerialText);
        Assert.True(mirror.IsConnected);
        Assert.Equal(Strings.StatusConnected, mirror.StatusText);
    }

    [Fact]
    public void Mirror_UnauthorizedDevice_ShowsAuthorizationProblem()
    {
        var (settings, localization) = CreateSettings();
        var monitor = new FakeDeviceMonitor
        {
            Devices = new[]
            {
                new DeviceInfo("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Unauthorized, ConnectionMode.Usb)
            }
        };
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), monitor, new FakeScrcpyService());
        monitor.RaiseChanged();

        Assert.False(mirror.IsConnected);
        Assert.Equal(Strings.StatusUnauthorized, mirror.StatusText);
    }

    [Fact]
    public async Task Mirror_Refresh_ReadsVersionFromAdb()
    {
        var (settings, localization) = CreateSettings();
        var adb = new FakeAdbService { Version = "Android Debug Bridge version 1.0.41" };
        using var mirror = new MirrorViewModel(settings, localization, adb, new FakeDeviceMonitor(), new FakeScrcpyService());

        await mirror.RefreshAsync();

        Assert.Equal(adb.Version, mirror.AdbVersionText);
    }

    [Fact]
    public async Task Wireless_InvalidInput_DoesNotCallAdb()
    {
        var (_, localization) = CreateSettings();
        var adb = new FakeAdbService();
        using var dialog = new WirelessConnectViewModel(adb, localization);

        dialog.Host = "bad host";
        await dialog.ConnectAsync();

        Assert.Equal(Strings.InvalidHost, dialog.ErrorMessage);
        Assert.Empty(adb.Calls);
    }

    [Fact]
    public async Task Wireless_ValidInput_CallsAdbWithValidatedValues()
    {
        var (_, localization) = CreateSettings();
        var adb = new FakeAdbService();
        using var dialog = new WirelessConnectViewModel(adb, localization);
        var closed = false;
        dialog.RequestClose += (_, _) => closed = true;
        dialog.Host = "192.168.1.20";
        dialog.Port = "5555";

        await dialog.ConnectAsync();

        Assert.True(closed);
        Assert.Single(adb.Calls);
        Assert.Equal("192.168.1.20", adb.Calls[0][1]);
    }

    [Fact]
    public async Task Mirror_StartMirroring_UsesSelectedConnectedDevice()
    {
        var (settings, localization) = CreateSettings();
        var device = new DeviceInfo("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);
        var monitor = new FakeDeviceMonitor { Devices = new[] { device } };
        var scrcpy = new FakeScrcpyService();
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), monitor, scrcpy);
        monitor.RaiseChanged();

        await mirror.StartMirroringAsync();

        Assert.True(mirror.IsMirroring);
        Assert.NotEqual(IntPtr.Zero, mirror.WindowHandle);
        Assert.Equal(device.Serial, scrcpy.StartedDevices[0].Serial);
    }

    [Fact]
    public async Task Mirror_StartWithoutDevice_ReportsError()
    {
        var (settings, localization) = CreateSettings();
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), new FakeDeviceMonitor(), new FakeScrcpyService());

        await mirror.StartMirroringAsync();

        Assert.Equal(Strings.NoDeviceSelected, mirror.LastErrorText);
        Assert.False(mirror.IsMirroring);
    }

    [Fact]
    public async Task Wireless_PairingWithInvalidCode_DoesNotCallAdb()
    {
        var (_, localization) = CreateSettings();
        var adb = new FakeAdbService();
        using var dialog = new WirelessConnectViewModel(adb, localization);
        dialog.IsPairMode = true;
        dialog.Host = "192.168.1.20";
        dialog.PairCode = "abc";

        await dialog.PairAsync();

        Assert.Equal(Strings.InvalidPairCode, dialog.ErrorMessage);
        Assert.Empty(adb.Calls);
    }

    [Fact]
    public async Task Mirror_AutoReconnect_RestartsWhenDeviceReturns()
    {
        var (settings, localization) = CreateSettings();
        settings.Current.AutoReconnect = true;
        var device = new DeviceInfo("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);
        var monitor = new FakeDeviceMonitor { Devices = new[] { device } };
        var scrcpy = new FakeScrcpyService();
        using var mirror = new MirrorViewModel(settings, localization, new FakeAdbService(), monitor, scrcpy);
        monitor.RaiseChanged();
        await mirror.StartMirroringAsync();

        scrcpy.RaiseState(MirroringState.Stopped);
        monitor.Devices = new[] { device with { State = DeviceConnectionState.Offline } };
        monitor.RaiseChanged();
        monitor.Devices = new[] { device };
        monitor.RaiseChanged();
        await Task.Delay(500);

        Assert.True(scrcpy.StartedDevices.Count >= 2);
    }

    private (SettingsService Settings, LocalizationService Localization) CreateSettings()
    {
        var settings = new SettingsService(_path);
        settings.Load();
        var localization = new LocalizationService(settings);
        localization.SetLanguage("en");
        return (settings, localization);
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
