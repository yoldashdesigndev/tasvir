using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class DeviceMonitor : IDeviceMonitor
{
    private readonly IAdbService _adb;
    private readonly TimeSpan _interval;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CancellationTokenSource? _cancellation;
    private Task? _loop;
    private IReadOnlyList<DeviceInfo> _devices = Array.Empty<DeviceInfo>();

    public DeviceMonitor(IAdbService adb, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(adb);
        _adb = adb;
        _interval = interval ?? TimeSpan.FromSeconds(2);
    }

    public IReadOnlyList<DeviceInfo> Devices => _devices;

    public string? LastError { get; private set; }

    public bool IsRunning => _loop is not null && !_loop.IsCompleted;

    public event EventHandler? DevicesChanged;

    public void StartMonitoring()
    {
        if (_loop is not null && !_loop.IsCompleted)
        {
            return;
        }

        _cancellation?.Dispose();
        _cancellation = new CancellationTokenSource();
        _loop = RunLoopAsync(_cancellation.Token);
    }

    public void StopMonitoring()
    {
        _cancellation?.Cancel();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _devices = await _adb.GetDevicesAsync(cancellationToken).ConfigureAwait(false);
            LastError = null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AdbException exception)
        {
            LastError = exception.Message;
        }
        catch (TimeoutException exception)
        {
            LastError = exception.Message;
        }
        catch (IOException exception)
        {
            LastError = exception.Message;
        }
        catch (InvalidOperationException exception)
        {
            LastError = exception.Message;
        }
        finally
        {
            _gate.Release();
            DevicesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await Task.Delay(_interval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        StopMonitoring();
        _gate.Dispose();
    }
}
