using System.Diagnostics;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class ProcessLauncher : IProcessLauncher
{
    public IManagedProcess Start(string executablePath, IReadOnlyList<string> arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);

        var fullPath = Path.GetFullPath(executablePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Executable not found.", fullPath);
        }

        // Security: fixed executable path and ArgumentList; no shell is involved.
        var startInfo = new ProcessStartInfo(fullPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = Process.Start(startInfo)
                      ?? throw new InvalidOperationException("Failed to start process.");
        return new ManagedProcess(process);
    }
}
