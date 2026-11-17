using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(
        string executablePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        var fullPath = Path.GetFullPath(executablePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Executable not found.", fullPath);
        }

        var startInfo = new ProcessStartInfo(fullPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // Security: ArgumentList passes each value verbatim, so no shell can reinterpret it.
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync(linked.Token);
            var errorTask = process.StandardError.ReadToEndAsync(linked.Token);
            await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);

            var output = await outputTask.ConfigureAwait(false);
            var error = await errorTask.ConfigureAwait(false);
            return new ProcessResult(process.ExitCode, output, error);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            Kill(process);
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"Process timed out after {timeout.TotalSeconds:0} seconds.");
        }
    }

    private static void Kill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (Win32Exception)
        {
        }
    }
}
