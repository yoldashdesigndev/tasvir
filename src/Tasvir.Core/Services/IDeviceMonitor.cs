using Tasvir.Core.Models;

namespace Tasvir.Core.Services;

public interface IDeviceMonitor : IDisposable
{
    IReadOnlyList<DeviceInfo> Devices { get; }

    string? LastError { get; }

    bool IsRunning { get; }

    event EventHandler? DevicesChanged;

    void StartMonitoring();

    void StopMonitoring();

    Task RefreshAsync(CancellationToken cancellationToken = default);
}
