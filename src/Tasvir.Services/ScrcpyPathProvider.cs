using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class ScrcpyPathProvider : IScrcpyPathProvider
{
    public string? FindExecutable()
    {
        var fileName = OperatingSystem.IsWindows() ? "scrcpy.exe" : "scrcpy";
        return Find(fileName);
    }

    public string? FindServer() => Find("scrcpy-server");

    private static string? Find(string fileName)
    {
        // The engine is resolved only from the app-local tools directory.
        var path = Path.Combine(AppContext.BaseDirectory, "tools", fileName);
        return File.Exists(path) ? Path.GetFullPath(path) : null;
    }
}
