using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class AdbService : IAdbService, IDisposable
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(15);
    private static readonly string[] DevicesArguments = { "devices", "-l" };
    private static readonly string[] VersionArguments = { "version" };

    private readonly IProcessRunner _runner;
    private readonly IAdbPathProvider _pathProvider;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AdbService(IProcessRunner runner, IAdbPathProvider pathProvider)
    {
        ArgumentNullException.ThrowIfNull(runner);
        ArgumentNullException.ThrowIfNull(pathProvider);
        _runner = runner;
        _pathProvider = pathProvider;
    }

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(DevicesArguments, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new AdbException(AdbInputValidator.Sanitize(result.StandardError));
        }

        return AdbOutputParser.ParseDevices(result.StandardOutput);
    }

    public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(VersionArguments, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new AdbException(AdbInputValidator.Sanitize(result.StandardError));
        }

        return AdbOutputParser.ParseVersion(result.StandardOutput);
    }

    public async Task<AdbResult> ConnectWirelessAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        if (!AdbInputValidator.IsValidHost(host))
        {
            return AdbResult.Fail("Invalid host");
        }

        if (!AdbInputValidator.IsValidPort(port))
        {
            return AdbResult.Fail("Invalid port");
        }

        var result = await RunAsync(new[] { "connect", $"{host}:{port}" }, cancellationToken).ConfigureAwait(false);
        var message = AdbInputValidator.Sanitize(
            string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError);

        var connected = result.Succeeded &&
                        message.Contains("connected to", StringComparison.OrdinalIgnoreCase) &&
                        !message.Contains("failed", StringComparison.OrdinalIgnoreCase);

        return connected ? AdbResult.Ok(message) : AdbResult.Fail(message);
    }

    public async Task<AdbResult> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default)
    {
        if (!AdbInputValidator.IsValidHost(host))
        {
            return AdbResult.Fail("Invalid host");
        }

        if (!AdbInputValidator.IsValidPort(port))
        {
            return AdbResult.Fail("Invalid port");
        }

        if (!AdbInputValidator.IsValidPairCode(code))
        {
            return AdbResult.Fail("Invalid pairing code");
        }

        var result = await RunAsync(new[] { "pair", $"{host}:{port}", code }, cancellationToken).ConfigureAwait(false);
        var message = AdbInputValidator.Sanitize(
            string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError);

        var paired = result.Succeeded && message.Contains("successfully paired", StringComparison.OrdinalIgnoreCase);
        return paired ? AdbResult.Ok(message) : AdbResult.Fail(message);
    }

    public async Task<AdbResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default)
    {
        if (!AdbInputValidator.IsValidSerial(serial))
        {
            return AdbResult.Fail("Invalid device identifier");
        }

        var result = await RunAsync(new[] { "disconnect", serial }, cancellationToken).ConfigureAwait(false);
        var message = AdbInputValidator.Sanitize(
            string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError);

        return result.Succeeded ? AdbResult.Ok(message) : AdbResult.Fail(message);
    }

    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ProcessResult> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var executable = _pathProvider.FindExecutable();
        if (executable is null)
        {
            throw new AdbException("ADB executable not found");
        }

        // One adb command at a time prevents duplicate server starts and races.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await _runner.RunAsync(executable, arguments, CommandTimeout, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
