using Tasvir.Core.Models;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class ScrcpyServiceTests
{
    private static readonly DeviceInfo Device =
        new("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);

    [Fact]
    public async Task Start_ValidDevice_SetsRunningAndExposesWindow()
    {
        var launcher = new FakeProcessLauncher();
        var service = new ScrcpyService(launcher, new FakeScrcpyPathProvider());

        var result = await service.StartAsync(Device, new AppSettings());

        Assert.True(result.Success);
        Assert.Equal(MirroringState.Running, service.State);
        Assert.Equal(launcher.Process.MainWindowHandle, service.WindowHandle);
        Assert.Equal("scrcpy.exe", launcher.Calls[0].Executable);
    }

    [Fact]
    public async Task Start_Twice_RejectsDuplicate()
    {
        var service = new ScrcpyService(new FakeProcessLauncher(), new FakeScrcpyPathProvider());
        await service.StartAsync(Device, new AppSettings());

        var second = await service.StartAsync(Device, new AppSettings());

        Assert.False(second.Success);
    }

    [Fact]
    public async Task Start_InvalidSerial_RejectsBeforeLaunch()
    {
        var launcher = new FakeProcessLauncher();
        var service = new ScrcpyService(launcher, new FakeScrcpyPathProvider());
        var invalid = Device with { Serial = "bad;serial" };

        var result = await service.StartAsync(invalid, new AppSettings());

        Assert.False(result.Success);
        Assert.Empty(launcher.Calls);
    }

    [Fact]
    public async Task Start_MissingEngine_ReturnsFailure()
    {
        var launcher = new FakeProcessLauncher();
        var paths = new FakeScrcpyPathProvider { Executable = null };
        var service = new ScrcpyService(launcher, paths);

        var result = await service.StartAsync(Device, new AppSettings());

        Assert.False(result.Success);
        Assert.Empty(launcher.Calls);
    }

    [Fact]
    public async Task Stop_ClosesProcessAndClearsWindow()
    {
        var launcher = new FakeProcessLauncher();
        var service = new ScrcpyService(launcher, new FakeScrcpyPathProvider());
        await service.StartAsync(Device, new AppSettings());

        await service.StopAsync();

        Assert.True(launcher.Process.CloseRequested);
        Assert.Equal(MirroringState.Stopped, service.State);
        Assert.Equal(IntPtr.Zero, service.WindowHandle);
    }

    [Fact]
    public async Task ProcessExit_SetsStateToStopped()
    {
        var launcher = new FakeProcessLauncher();
        var service = new ScrcpyService(launcher, new FakeScrcpyPathProvider());
        await service.StartAsync(Device, new AppSettings());

        launcher.Process.RaiseExited();

        Assert.Equal(MirroringState.Stopped, service.State);
    }
}
