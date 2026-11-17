namespace Tasvir.Core.Services;

public interface IManagedProcess : IDisposable
{
    IntPtr MainWindowHandle { get; }

    bool HasExited { get; }

    int ExitCode { get; }

    event EventHandler? Exited;

    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    bool CloseMainWindow();

    void Kill();
}
