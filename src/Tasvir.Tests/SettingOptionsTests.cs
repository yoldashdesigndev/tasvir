using Tasvir.ViewModels;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class SettingOptionsTests
{
    [Theory]
    [InlineData("1080x1920", 1080, 1920)]
    [InlineData("540x960", 540, 960)]
    public void TryParseResolution_ValidValue(string value, int width, int height)
    {
        Assert.True(SettingOptions.TryParseResolution(value, out var parsedWidth, out var parsedHeight));
        Assert.Equal(width, parsedWidth);
        Assert.Equal(height, parsedHeight);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1x2x3")]
    [InlineData("")]
    public void TryParseResolution_InvalidValue_ReturnsFalse(string value)
    {
        Assert.False(SettingOptions.TryParseResolution(value, out _, out _));
    }

    [Fact]
    public void TryParseResolution_Native_ReturnsZeroDimensions()
    {
        Assert.True(SettingOptions.TryParseResolution("Native", out var width, out var height));
        Assert.Equal(0, width);
        Assert.Equal(0, height);
    }

    [Theory]
    [InlineData("8 Mbps", 8)]
    [InlineData("120 fps", 120)]
    public void TryParseLeadingInt_ValidValue(string value, int expected)
    {
        Assert.True(SettingOptions.TryParseLeadingInt(value, out var result));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryParseLeadingInt_InvalidValue_ReturnsFalseAndZero()
    {
        Assert.False(SettingOptions.TryParseLeadingInt("garbage", out var result));
        Assert.Equal(0, result);
    }
}
