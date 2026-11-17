using System.Globalization;

namespace Tasvir.ViewModels;

public static class SettingOptions
{
    public static IReadOnlyList<string> Resolutions { get; } =
        new[] { "1080x1920", "720x1280", "540x960", "Native" };

    public static IReadOnlyList<string> Bitrates { get; } =
        new[] { "2 Mbps", "4 Mbps", "8 Mbps", "16 Mbps" };

    public static IReadOnlyList<string> FrameRates { get; } =
        new[] { "30 fps", "60 fps", "120 fps" };

    public static IReadOnlyList<string> Rotations { get; } =
        new[] { "0°", "90°", "180°", "270°" };

    public static string FormatResolution(int width, int height) =>
        width <= 0 || height <= 0
            ? "Native"
            : width.ToString(CultureInfo.InvariantCulture) + "x" + height.ToString(CultureInfo.InvariantCulture);

    public static string FormatBitrate(int value) =>
        value.ToString(CultureInfo.InvariantCulture) + " Mbps";

    public static string FormatFrameRate(int value) =>
        value.ToString(CultureInfo.InvariantCulture) + " fps";

    public static bool TryParseResolution(string value, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (string.Equals(value, "Native", StringComparison.Ordinal))
        {
            return true;
        }

        var parts = value.Split('x');
        return parts.Length == 2 &&
               int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out width) &&
               int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out height);
    }

    public static string FormatRotation(int value) => value switch
    {
        90 => "90°",
        180 => "180°",
        270 => "270°",
        _ => "0°"
    };

    public static bool TryParseRotation(string value, out int result) =>
        int.TryParse(value.Trim().TrimEnd('°'), NumberStyles.None, CultureInfo.InvariantCulture, out result) &&
        result is 0 or 90 or 180 or 270;

    public static bool TryParseLeadingInt(string value, out int result)
    {
        result = 0;
        var token = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return token is not null &&
               int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out result);
    }
}
