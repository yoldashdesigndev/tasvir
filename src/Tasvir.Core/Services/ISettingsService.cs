using Tasvir.Core.Models;

namespace Tasvir.Core.Services;

public interface ISettingsService
{
    AppSettings Current { get; }

    string? LastError { get; }

    event EventHandler? SettingsChanged;

    void Load();

    void Save();

    void Update(Action<AppSettings> update);
}
