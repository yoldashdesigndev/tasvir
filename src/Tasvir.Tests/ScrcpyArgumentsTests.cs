using Tasvir.Core.Models;
using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class ScrcpyArgumentsTests
{
    private static readonly DeviceInfo Device =
        new("ABC123", "Pixel_7", "Pixel 7", DeviceConnectionState.Connected, ConnectionMode.Usb);

    [Fact]
    public void Build_IncludesSafetyAndQualityOptions()
    {
        var settings = new AppSettings { MaxWidth = 1080, MaxHeight = 1920, MaxFps = 60, BitrateMbps = 8 };

        var arguments = ScrcpyArguments.Build(Device, settings, "Tasvir-test");

        Assert.Equal("-s", arguments[0]);
        Assert.Equal("ABC123", arguments[1]);
        Assert.Contains("--window-title=Tasvir-test", arguments);
        Assert.Contains("--window-borderless", arguments);
        Assert.Contains("--max-size=1920", arguments);
        Assert.Contains("--max-fps=60", arguments);
        Assert.Contains("--video-bit-rate=8M", arguments);
    }

    [Fact]
    public void Build_NativeResolution_OmitsMaxSize()
    {
        var settings = new AppSettings { MaxWidth = 0, MaxHeight = 0 };

        var arguments = ScrcpyArguments.Build(Device, settings, "Tasvir-test");

        Assert.DoesNotContain(arguments, argument => argument.StartsWith("--max-size", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(90, "--orientation=90")]
    [InlineData(180, "--orientation=180")]
    [InlineData(270, "--orientation=270")]
    public void Build_Rotation_AddsOrientationOption(int rotation, string option)
    {
        var settings = new AppSettings { Rotation = rotation };

        var arguments = ScrcpyArguments.Build(Device, settings, "title");

        Assert.Contains(option, arguments);
    }

    [Fact]
    public void Build_InvalidRotation_IsIgnored()
    {
        var settings = new AppSettings { Rotation = 45 };

        var arguments = ScrcpyArguments.Build(Device, settings, "title");

        Assert.DoesNotContain(arguments, argument => argument.StartsWith("--orientation", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_InvalidSerial_Throws()
    {
        var invalid = Device with { Serial = "bad;serial" };

        Assert.Throws<ArgumentException>(() => ScrcpyArguments.Build(invalid, new AppSettings(), "title"));
    }
}
