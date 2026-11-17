namespace Tasvir.Core.Services;

public interface IScrcpyPathProvider
{
    string? FindExecutable();

    string? FindServer();
}
