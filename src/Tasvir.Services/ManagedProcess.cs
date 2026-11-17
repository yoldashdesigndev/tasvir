using System.Diagnostics;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class ManagedProcess : IManagedProcess
{
    private readonly Process _process;
    private readonly TaskCompletionSource _exitCompletion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ManagedProcess(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);
        _process = process;
        _process.EnableRaisingEvents = true;
        _process.Exited += OnExited;
        if (_process.HasExited)
        {
            _exitCompletion.TrySetResult();
        }
    }

    public IntPtr MainWindowHandle
    {
        get
        {
            _process.Refresh();
            return _process.MainWindowHandle;
        }
    }

    public bool HasExited => _process.HasExited;

    public int ExitCode => _process.HasExited ? _process.ExitCode : 0;

    public event EventHandler? Exited;

    public Task WaitForExitAsync(CancellationToken cancellationToken = default) =>
        _exitCompletion.Task.WaitAsync(cancellationToken);

    public bool CloseMainWindow() => !_process.HasExited && _process.CloseMainWindow();

    public void Kill()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void Dispose()
    {
        _process.Exited -= OnExited;
        _process.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnExited(object? sender, EventArgs e)
    {
        _exitCompletion.TrySetResult();
        Exited?.Invoke(this, EventArgs.Empty);
    }
}
