using System.Text;
using System.Text.RegularExpressions;

namespace Tasvir.Services;

public static partial class AdbInputValidator
{
    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9.\-]{0,252}$", RegexOptions.CultureInvariant)]
    private static partial Regex HostRegex();

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9._\-:]{0,127}$", RegexOptions.CultureInvariant)]
    private static partial Regex SerialRegex();

    [GeneratedRegex(@"^\d{6,8}$", RegexOptions.CultureInvariant)]
    private static partial Regex PairCodeRegex();

    public static bool IsValidHost(string? host) => host is not null && HostRegex().IsMatch(host);

    public static bool IsValidPort(int port) => port is >= 1 and <= 65535;

    public static bool IsValidSerial(string? serial) => serial is not null && SerialRegex().IsMatch(serial);

    public static bool IsValidPairCode(string? code) => code is not null && PairCodeRegex().IsMatch(code);

    public static string Sanitize(string? text, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(Math.Min(text.Length, maxLength));
        foreach (var character in text)
        {
            if (builder.Length >= maxLength)
            {
                break;
            }

            if (char.IsControl(character) && character is not ('\r' or '\n' or '\t'))
            {
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}
