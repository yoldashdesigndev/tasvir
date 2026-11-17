using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class AdbPathProvider : IAdbPathProvider
{
    public string? FindExecutable()
    {
        var fileName = OperatingSystem.IsWindows() ? "adb.exe" : "adb";

        // Only trusted locations are accepted: the app-local tools folder and the system PATH.
        // The file name is fixed so external input can never choose an executable.
        var localPath = Path.Combine(AppContext.BaseDirectory, "tools", fileName);
        if (File.Exists(localPath))
        {
            return Path.GetFullPath(localPath);
        }

        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return null;
        }

        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }
}
