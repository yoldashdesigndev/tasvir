namespace Tasvir.Core.Services;

public interface IProcessLauncher
{
    IManagedProcess Start(string executablePath, IReadOnlyList<string> arguments);
}
