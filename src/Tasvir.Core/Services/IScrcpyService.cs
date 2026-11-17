using Tasvir.Core.Models;

namespace Tasvir.Core.Services;

public interface IScrcpyService
{
    MirroringState State { get; }

    IntPtr WindowHandle { get; }

    string? LastError { get; }

    event EventHandler<MirroringState>? StateChanged;

    event EventHandler? WindowReady;

    Task<AdbResult> StartAsync(DeviceInfo device, AppSettings settings, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
