using Tasvir.Core.Models;
using Tasvir.Core.Services;

namespace Tasvir.Tests;

internal sealed class FakeAdbService : IAdbService
{
    public IReadOnlyList<DeviceInfo> Devices { get; set; } = Array.Empty<DeviceInfo>();

    public string Version { get; set; } = "Android Debug Bridge version 1.0.41";

    public AdbResult ConnectResult { get; set; } = AdbResult.Ok("connected to 192.168.1.20:5555");

    public AdbResult DisconnectResult { get; set; } = AdbResult.Ok("disconnected");

    public AdbResult PairResult { get; set; } = AdbResult.Ok("Successfully paired to device");

    public List<IReadOnlyList<string>> Calls { get; } = new();

    public Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Devices);
    }

    public Task<string> GetVersionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Version);

    public Task<AdbResult> ConnectWirelessAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        Calls.Add(new[] { "connect", host, port.ToString() });
        return Task.FromResult(ConnectResult);
    }

    public Task<AdbResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default)
    {
        Calls.Add(new[] { "disconnect", serial });
        return Task.FromResult(DisconnectResult);
    }

    public Task<AdbResult> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default)
    {
        Calls.Add(new[] { "pair", host, port.ToString(), code });
        return Task.FromResult(PairResult);
    }
}

internal sealed class FakeDeviceMonitor : IDeviceMonitor
{
    public IReadOnlyList<DeviceInfo> Devices { get; set; } = Array.Empty<DeviceInfo>();

    public string? LastError { get; set; }

    public bool IsRunning { get; private set; }

    public event EventHandler? DevicesChanged;

    public void RaiseChanged() => DevicesChanged?.Invoke(this, EventArgs.Empty);

    public void StartMonitoring() => IsRunning = true;

    public void StopMonitoring() => IsRunning = false;

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        RaiseChanged();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopMonitoring();
        GC.SuppressFinalize(this);
    }
}

internal sealed class FakeProcessRunner : IProcessRunner
{
    public Queue<ProcessResult> Results { get; } = new();

    public List<(string Executable, IReadOnlyList<string> Arguments, TimeSpan Timeout)> Calls { get; } = new();

    public Task<ProcessResult> RunAsync(
        string executablePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add((executablePath, arguments, timeout));
        var result = Results.Count > 0
            ? Results.Dequeue()
            : new ProcessResult(0, string.Empty, string.Empty);
        return Task.FromResult(result);
    }
}

internal sealed class FakeAdbPathProvider : IAdbPathProvider
{
    public string? Path { get; set; } = "adb";

    public string? FindExecutable() => Path;
}

internal sealed class FakeScrcpyService : IScrcpyService
{
    public MirroringState State { get; set; } = MirroringState.Stopped;

    public IntPtr WindowHandle { get; set; }

    public string? LastError { get; set; }

    public AdbResult StartResult { get; set; } = AdbResult.Ok();

    public List<DeviceInfo> StartedDevices { get; } = new();

    public event EventHandler<MirroringState>? StateChanged;

    public event EventHandler? WindowReady;

    public Task<AdbResult> StartAsync(DeviceInfo device, AppSettings settings, CancellationToken cancellationToken = default)
    {
        StartedDevices.Add(device);
        if (!StartResult.Success)
        {
            LastError = StartResult.Message;
            State = MirroringState.Error;
            StateChanged?.Invoke(this, State);
            return Task.FromResult(StartResult);
        }

        State = MirroringState.Running;
        WindowHandle = new IntPtr(1234);
        StateChanged?.Invoke(this, State);
        WindowReady?.Invoke(this, EventArgs.Empty);
        return Task.FromResult(StartResult);
    }

    public void RaiseState(MirroringState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        State = MirroringState.Stopped;
        WindowHandle = IntPtr.Zero;
        StateChanged?.Invoke(this, State);
        return Task.CompletedTask;
    }
}

internal sealed class FakeManagedProcess : IManagedProcess
{
    public IntPtr MainWindowHandle { get; set; } = new IntPtr(4242);

    public bool HasExited { get; set; }

    public int ExitCode { get; set; }

    public bool CloseRequested { get; private set; }

    public bool KillRequested { get; private set; }

    public event EventHandler? Exited;

    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public bool CloseMainWindow()
    {
        CloseRequested = true;
        RaiseExited();
        return true;
    }

    public void Kill()
    {
        KillRequested = true;
        RaiseExited();
    }

    public void RaiseExited()
    {
        HasExited = true;
        Exited?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}

internal sealed class FakeProcessLauncher : IProcessLauncher
{
    public FakeManagedProcess Process { get; set; } = new();

    public List<(string Executable, IReadOnlyList<string> Arguments)> Calls { get; } = new();

    public IManagedProcess Start(string executablePath, IReadOnlyList<string> arguments)
    {
        Calls.Add((executablePath, arguments));
        return Process;
    }
}

internal sealed class FakeScrcpyPathProvider : IScrcpyPathProvider
{
    public string? Executable { get; set; } = "scrcpy.exe";

    public string? Server { get; set; } = "scrcpy-server";

    public string? FindExecutable() => Executable;

    public string? FindServer() => Server;
}
