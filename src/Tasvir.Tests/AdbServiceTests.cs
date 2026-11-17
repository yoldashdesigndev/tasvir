using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class AdbServiceTests
{
    [Fact]
    public async Task GetDevices_PassesSafeArgumentsAndParses()
    {
        var runner = new FakeProcessRunner();
        runner.Results.Enqueue(new ProcessResult(0,
            "List of devices attached\nABC123\tdevice model:Pixel_7 device:oriole transport_id:1\n",
            string.Empty));
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var devices = await service.GetDevicesAsync();

        var device = Assert.Single(devices);
        Assert.Equal("ABC123", device.Serial);
        Assert.Equal("Pixel_7", device.Model);
        Assert.Equal(DeviceConnectionState.Connected, device.State);
        Assert.Equal(new[] { "devices", "-l" }, runner.Calls[0].Arguments);
    }

    [Fact]
    public async Task GetDevices_NonZeroExit_ThrowsAdbException()
    {
        var runner = new FakeProcessRunner();
        runner.Results.Enqueue(new ProcessResult(1, string.Empty, "error: no devices"));
        var service = new AdbService(runner, new FakeAdbPathProvider());

        await Assert.ThrowsAsync<AdbException>(() => service.GetDevicesAsync());
    }

    [Fact]
    public async Task GetDevices_AdbMissing_ThrowsAdbException()
    {
        var service = new AdbService(new FakeProcessRunner(), new FakeAdbPathProvider { Path = null });

        await Assert.ThrowsAsync<AdbException>(() => service.GetDevicesAsync());
    }

    [Theory]
    [InlineData("bad host")]
    [InlineData("")]
    [InlineData("host;cmd")]
    public async Task ConnectWireless_InvalidHost_RejectsBeforeRunningProcess(string host)
    {
        var runner = new FakeProcessRunner();
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.ConnectWirelessAsync(host, 5555);

        Assert.False(result.Success);
        Assert.Empty(runner.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public async Task ConnectWireless_InvalidPort_RejectsBeforeRunningProcess(int port)
    {
        var runner = new FakeProcessRunner();
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.ConnectWirelessAsync("192.168.1.20", port);

        Assert.False(result.Success);
        Assert.Empty(runner.Calls);
    }

    [Fact]
    public async Task ConnectWireless_ConnectedOutput_ReturnsSuccess()
    {
        var runner = new FakeProcessRunner();
        runner.Results.Enqueue(new ProcessResult(0, "connected to 192.168.1.20:5555\n", string.Empty));
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.ConnectWirelessAsync("192.168.1.20", 5555);

        Assert.True(result.Success);
        Assert.Equal("connect", runner.Calls[0].Arguments[0]);
        Assert.Equal("192.168.1.20:5555", runner.Calls[0].Arguments[1]);
    }

    [Fact]
    public async Task ConnectWireless_FailedOutput_ReturnsFailure()
    {
        var runner = new FakeProcessRunner();
        runner.Results.Enqueue(new ProcessResult(0, "failed to connect to 192.168.1.20:5555\n", string.Empty));
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.ConnectWirelessAsync("192.168.1.20", 5555);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Pair_ValidInput_RunsPairCommand()
    {
        var runner = new FakeProcessRunner();
        runner.Results.Enqueue(new ProcessResult(0, "Successfully paired to 192.168.1.20:37003\n", string.Empty));
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.PairAsync("192.168.1.20", 37003, "123456");

        Assert.True(result.Success);
        Assert.Equal("pair", runner.Calls[0].Arguments[0]);
        Assert.Equal("192.168.1.20:37003", runner.Calls[0].Arguments[1]);
        Assert.Equal("123456", runner.Calls[0].Arguments[2]);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdef")]
    [InlineData("123456;rm")]
    [InlineData("")]
    public async Task Pair_InvalidCode_RejectsBeforeRunningProcess(string code)
    {
        var runner = new FakeProcessRunner();
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.PairAsync("192.168.1.20", 37003, code);

        Assert.False(result.Success);
        Assert.Empty(runner.Calls);
    }

    [Fact]
    public async Task Disconnect_InvalidSerial_RejectsBeforeRunningProcess()
    {
        var runner = new FakeProcessRunner();
        var service = new AdbService(runner, new FakeAdbPathProvider());

        var result = await service.DisconnectAsync("serial;cmd");

        Assert.False(result.Success);
        Assert.Empty(runner.Calls);
    }
}
