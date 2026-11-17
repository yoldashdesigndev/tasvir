using Tasvir.Services;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class AdbInputValidatorTests
{
    [Theory]
    [InlineData("192.168.1.20")]
    [InlineData("phone.local")]
    [InlineData("localhost")]
    [InlineData("my-phone")]
    public void IsValidHost_AcceptsSafeHosts(string host)
    {
        Assert.True(AdbInputValidator.IsValidHost(host));
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad host")]
    [InlineData("host;cmd")]
    [InlineData("-flag")]
    [InlineData("host/path")]
    public void IsValidHost_RejectsUnsafeHosts(string host)
    {
        Assert.False(AdbInputValidator.IsValidHost(host));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5555)]
    [InlineData(65535)]
    public void IsValidPort_AcceptsValidRange(int port)
    {
        Assert.True(AdbInputValidator.IsValidPort(port));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void IsValidPort_RejectsOutOfRange(int port)
    {
        Assert.False(AdbInputValidator.IsValidPort(port));
    }

    [Theory]
    [InlineData("emulator-5554")]
    [InlineData("192.168.1.20:5555")]
    [InlineData("ABCDEF123456")]
    public void IsValidSerial_AcceptsExpectedFormats(string serial)
    {
        Assert.True(AdbInputValidator.IsValidSerial(serial));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a b")]
    [InlineData("a;b")]
    [InlineData("a/b")]
    public void IsValidSerial_RejectsUnsafeValues(string serial)
    {
        Assert.False(AdbInputValidator.IsValidSerial(serial));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    public void IsValidPairCode_AcceptsSixToEightDigits(string code)
    {
        Assert.True(AdbInputValidator.IsValidPairCode(code));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdef")]
    [InlineData("12 456")]
    [InlineData("")]
    public void IsValidPairCode_RejectsInvalidValues(string code)
    {
        Assert.False(AdbInputValidator.IsValidPairCode(code));
    }

    [Fact]
    public void Sanitize_RemovesControlCharactersAndTruncates()
    {
        var dirty = "abc\u0001def" + new string('x', 900);

        var clean = AdbInputValidator.Sanitize(dirty, 100);

        Assert.Equal(100, clean.Length);
        Assert.DoesNotContain('\u0001', clean);
    }
}
