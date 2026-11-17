using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class DeviceMonitorTests
{
    [Fact]
    public async Task Refresh_CapturesErrorWithoutThrowing()
    {
        using var monitor = new DeviceMonitor(new ThrowingAdbService());

        await monitor.RefreshAsync();

        Assert.Equal("adb failed", monitor.LastError);
        Assert.Empty(monitor.Devices);
    }

    [Fact]
    public async Task Refresh_StoresDevicesAndClearsError()
    {
        var device = new DeviceInfo("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);
        var adb = new FakeAdbService { Devices = new[] { device } };
        using var monitor = new DeviceMonitor(adb);

        await monitor.RefreshAsync();

        Assert.Null(monitor.LastError);
        Assert.Single(monitor.Devices);
    }

    [Fact]
    public async Task StartMonitoring_PollsUntilStopped()
    {
        var adb = new CountingAdbService();
        using var monitor = new DeviceMonitor(adb, TimeSpan.FromMilliseconds(20));

        monitor.StartMonitoring();
        await Task.Delay(200);
        monitor.StopMonitoring();
        var countAtStop = adb.Count;
        await Task.Delay(80);

        Assert.True(adb.Count >= 2);
        Assert.Equal(countAtStop, adb.Count);
    }

    private sealed class ThrowingAdbService : IAdbService
    {
        public Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default) =>
            throw new AdbException("adb failed");

        public Task<string> GetVersionAsync(CancellationToken cancellationToken = default) =>
            throw new AdbException("adb failed");

        public Task<AdbResult> ConnectWirelessAsync(string host, int port, CancellationToken cancellationToken = default) =>
            throw new AdbException("adb failed");

        public Task<AdbResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default) =>
            throw new AdbException("adb failed");

        public Task<AdbResult> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default) =>
            throw new AdbException("adb failed");
    }

    private sealed class CountingAdbService : IAdbService
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _count);
            return Task.FromResult<IReadOnlyList<DeviceInfo>>(Array.Empty<DeviceInfo>());
        }

        public Task<string> GetVersionAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult("version");

        public Task<AdbResult> ConnectWirelessAsync(string host, int port, CancellationToken cancellationToken = default) =>
            Task.FromResult(AdbResult.Ok());

        public Task<AdbResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult(AdbResult.Ok());

        public Task<AdbResult> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(AdbResult.Ok());
    }
}
