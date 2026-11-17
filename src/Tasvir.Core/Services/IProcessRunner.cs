namespace Tasvir.Core.Services;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string executablePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
