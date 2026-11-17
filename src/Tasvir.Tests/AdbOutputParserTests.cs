using Tasvir.Core.Models;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class AdbOutputParserTests
{
    private const string DevicesOutput = """
        List of devices attached
        emulator-5554	device product:sdk_gphone64 model:Pixel_8 device:emu64a transport_id:1
        ABCDEF123456	unauthorized transport_id:2
        192.168.1.20:5555	device model:Pixel_7 device:oriole transport_id:3
        OFFLINE123	offline transport_id:4
        bad;serial	device
        """;

    [Fact]
    public void ParseDevices_ParsesKnownStatesAndModels()
    {
        var devices = AdbOutputParser.ParseDevices(DevicesOutput);

        Assert.Equal(4, devices.Count);
        Assert.Equal("Pixel_8", devices[0].Model);
        Assert.Equal(DeviceConnectionState.Connected, devices[0].State);
        Assert.Equal(ConnectionMode.Usb, devices[0].Mode);
        Assert.Equal(DeviceConnectionState.Unauthorized, devices[1].State);
        Assert.Equal(ConnectionMode.Wifi, devices[2].Mode);
        Assert.Equal(DeviceConnectionState.Offline, devices[3].State);
    }

    [Fact]
    public void ParseDevices_RejectsInvalidSerial()
    {
        var devices = AdbOutputParser.ParseDevices("List of devices attached\nbad;serial\tdevice\n");

        Assert.Empty(devices);
    }

    [Fact]
    public void ParseDevices_EmptyOutput_ReturnsEmpty()
    {
        Assert.Empty(AdbOutputParser.ParseDevices(string.Empty));
    }

    [Fact]
    public void ParseVersion_ReturnsFirstLine()
    {
        var version = AdbOutputParser.ParseVersion("Android Debug Bridge version 1.0.41\nVersion 36.0.2\n");

        Assert.Equal("Android Debug Bridge version 1.0.41", version);
    }
}
