using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class ScrcpyService : IScrcpyService, IDisposable
{
    private static readonly TimeSpan WindowTimeout = TimeSpan.FromSeconds(12);
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(2);

    private readonly IProcessLauncher _launcher;
    private readonly IScrcpyPathProvider _paths;
    private readonly SynchronizationContext? _context;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IManagedProcess? _process;
    private IntPtr _windowHandle;
    private string? _lastError;
    private MirroringState _state = MirroringState.Stopped;

    public ScrcpyService(IProcessLauncher launcher, IScrcpyPathProvider paths)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(paths);
        _launcher = launcher;
        _paths = paths;
        _context = SynchronizationContext.Current;
    }

    public MirroringState State => _state;

    public IntPtr WindowHandle => _windowHandle;

    public string? LastError => _lastError;

    public event EventHandler<MirroringState>? StateChanged;

    public event EventHandler? WindowReady;

    public async Task<AdbResult> StartAsync(
        DeviceInfo device,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(settings);

        if (!AdbInputValidator.IsValidSerial(device.Serial))
        {
            return AdbResult.Fail("Invalid device identifier");
        }

        var executable = _paths.FindExecutable();
        var server = _paths.FindServer();
        if (executable is null || server is null)
        {
            return AdbResult.Fail("Scrcpy engine not found");
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state is MirroringState.Starting or MirroringState.Running)
            {
                return AdbResult.Fail("Mirroring is already running");
            }

            SetState(MirroringState.Starting);
            _lastError = null;

            var title = "Tasvir-" + Guid.NewGuid().ToString("N");
            _process = _launcher.Start(executable, ScrcpyArguments.Build(device, settings, title));
            _process.Exited += OnProcessExited;

            var handle = await WaitForWindowAsync(_process, cancellationToken).ConfigureAwait(false);
            if (handle == IntPtr.Zero)
            {
                await StopAsync(cancellationToken).ConfigureAwait(false);
                return AdbResult.Fail("Scrcpy window did not appear");
            }

            _windowHandle = handle;
            SetState(MirroringState.Running);
            Raise(() => WindowReady?.Invoke(this, EventArgs.Empty));
            return AdbResult.Ok();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _lastError = "Mirroring start was cancelled";
            SetState(MirroringState.Stopped);
            throw;
        }
        catch (AdbException exception)
        {
            _lastError = exception.Message;
            SetState(MirroringState.Error);
            return AdbResult.Fail(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            _lastError = exception.Message;
            SetState(MirroringState.Error);
            return AdbResult.Fail(exception.Message);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var process = _process;
        if (process is null)
        {
            SetState(MirroringState.Stopped);
            return;
        }

        if (!process.HasExited)
        {
            process.CloseMainWindow();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(ShutdownTimeout);
            try
            {
                await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                process.Kill();
            }
        }

        process.Exited -= OnProcessExited;
        process.Dispose();
        _process = null;
        _windowHandle = IntPtr.Zero;
        SetState(MirroringState.Stopped);
    }

    public void Dispose()
    {
        _process?.Dispose();
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    private static async Task<IntPtr> WaitForWindowAsync(IManagedProcess process, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + WindowTimeout;
        while (DateTime.UtcNow < deadline && !process.HasExited)
        {
            var handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                return handle;
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        return IntPtr.Zero;
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        var exitCode = _process?.ExitCode ?? 0;
        _windowHandle = IntPtr.Zero;
        _lastError = exitCode == 0 ? null : $"Scrcpy exited with code {exitCode}";
        SetState(MirroringState.Stopped);
    }

    private void SetState(MirroringState state)
    {
        if (_state == state)
        {
            return;
        }

        _state = state;
        Raise(() => StateChanged?.Invoke(this, state));
    }

    private void Raise(Action action)
    {
        if (_context is null)
        {
            action();
            return;
        }

        _context.Post(_ => action(), null);
    }
}
