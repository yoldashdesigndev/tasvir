using Tasvir.Core.Models;

namespace Tasvir.Services;

public static class AdbOutputParser
{
    private const int MaxLines = 256;

    public static IReadOnlyList<DeviceInfo> ParseDevices(string output)
    {
        var devices = new List<DeviceInfo>();
        if (string.IsNullOrWhiteSpace(output))
        {
            return devices;
        }

        var lines = output.Split('\n');
        foreach (var rawLine in lines.Take(MaxLines))
        {
            var line = AdbInputValidator.Sanitize(rawLine, 512).Trim();
            if (line.Length == 0 || line.StartsWith("List of devices", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || !AdbInputValidator.IsValidSerial(parts[0]))
            {
                continue;
            }

            var serial = parts[0];
            var model = ExtractToken(parts, "model:") ?? ExtractToken(parts, "device:");
            var mode = serial.Contains(':') ? ConnectionMode.Wifi : ConnectionMode.Usb;

            devices.Add(new DeviceInfo(
                serial,
                model ?? string.Empty,
                model,
                ParseState(parts[1]),
                mode));
        }

        return devices;
    }

    public static string ParseVersion(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return string.Empty;
        }

        var line = AdbInputValidator.Sanitize(output.Split('\n').First(), 256).Trim();
        return line;
    }

    private static DeviceConnectionState ParseState(string value) => value switch
    {
        "device" => DeviceConnectionState.Connected,
        "unauthorized" => DeviceConnectionState.Unauthorized,
        "offline" => DeviceConnectionState.Offline,
        _ => DeviceConnectionState.Error
    };

    private static string? ExtractToken(string[] parts, string prefix) =>
        parts.FirstOrDefault(part => part.StartsWith(prefix, StringComparison.Ordinal))?[prefix.Length..];
}
