using Tasvir.Core.Models;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class SecurityTests
{
    [Fact]
    public async Task ProcessRunner_MissingExecutable_ThrowsBeforeStart()
    {
        var runner = new ProcessRunner();

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            runner.RunAsync(Path.Combine(Path.GetTempPath(), "tasvir-missing.exe"),
                new[] { "devices" }, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task ProcessRunner_InvalidTimeout_ThrowsBeforeStart()
    {
        var runner = new ProcessRunner();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            runner.RunAsync("adb", new[] { "devices" }, TimeSpan.Zero));
    }

    [Fact]
    public async Task ProcessRunner_NullArguments_ThrowsBeforeStart()
    {
        var runner = new ProcessRunner();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runner.RunAsync("adb", null!, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task ProcessRunner_EmptyExecutable_ThrowsBeforeStart()
    {
        var runner = new ProcessRunner();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            runner.RunAsync("  ", new[] { "devices" }, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task ScrcpyService_CancelledStart_ThrowsAndDoesNotLaunch()
    {
        var launcher = new FakeProcessLauncher();
        var service = new ScrcpyService(launcher, new FakeScrcpyPathProvider());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.StartAsync(Device, new AppSettings(), cancellation.Token));

        Assert.Empty(launcher.Calls);
    }

    [Fact]
    public async Task DeviceMonitor_CancelledRefresh_Throws()
    {
        using var monitor = new DeviceMonitor(new FakeAdbService());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => monitor.RefreshAsync(cancellation.Token));
    }

    [Fact]
    public void AdbOutputParser_ControlCharactersAreSanitized()
    {
        var output = "List of devices attached\nABC123\tdevice \u0001model:Pixel_7\n";

        var devices = AdbOutputParser.ParseDevices(output);

        var device = Assert.Single(devices);
        Assert.Equal("Pixel_7", device.Model);
        Assert.DoesNotContain('\u0001', device.Model);
    }

    [Fact]
    public async Task AdbService_PairWithInvalidHost_DoesNotLaunchProcess()
    {
        var runner = new FakeProcessRunner();
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.PairAsync("bad host", 37003, "123456");

        Assert.False(result.Success);
        Assert.Empty(runner.Calls);
    }

    private static readonly DeviceInfo Device =
        new("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);
}
