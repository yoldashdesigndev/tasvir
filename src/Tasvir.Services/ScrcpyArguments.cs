using System.Globalization;
using Tasvir.Core.Models;

namespace Tasvir.Services;

public static class ScrcpyArguments
{
    public static IReadOnlyList<string> Build(DeviceInfo device, AppSettings settings, string windowTitle)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(settings);
        if (!AdbInputValidator.IsValidSerial(device.Serial))
        {
            throw new ArgumentException("Invalid device serial", nameof(device));
        }

        var arguments = new List<string>
        {
            "-s",
            device.Serial,
            "--window-title=" + windowTitle,
            "--window-borderless",
            "--no-terminal-title"
        };

        var maxSize = Math.Max(settings.MaxWidth, settings.MaxHeight);
        if (maxSize > 0)
        {
            arguments.Add("--max-size=" + Number(maxSize));
        }

        if (settings.MaxFps > 0)
        {
            arguments.Add("--max-fps=" + Number(settings.MaxFps));
        }

        if (settings.BitrateMbps > 0)
        {
            arguments.Add("--video-bit-rate=" + Number(settings.BitrateMbps) + "M");
        }

        if (settings.Rotation is 90 or 180 or 270)
        {
            arguments.Add("--orientation=" + Number(settings.Rotation));
        }

        return arguments;
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
