using Tasvir.Core.Models;

namespace Tasvir.Core.Services;

public interface IAdbService
{
    Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);

    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);

    Task<AdbResult> ConnectWirelessAsync(string host, int port, CancellationToken cancellationToken = default);

    Task<AdbResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default);

    Task<AdbResult> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default);
}
