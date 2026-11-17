using System.Text.Json;
using Tasvir.Core.Models;
using Tasvir.Core.Services;

namespace Tasvir.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;

    public SettingsService(string? path = null)
    {
        // The default path is derived from a trusted SpecialFolder, never from external input.
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Tasvir",
            "settings.json");
        Current = new AppSettings();
    }

    public AppSettings Current { get; private set; }

    public string? LastError { get; private set; }

    public event EventHandler? SettingsChanged;

    public void Load()
    {
        LastError = null;
        try
        {
            if (!File.Exists(_path))
            {
                return;
            }

            Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), Options)
                      ?? new AppSettings();
        }
        catch (JsonException exception)
        {
            Current = new AppSettings();
            LastError = exception.Message;
        }
        catch (IOException exception)
        {
            Current = new AppSettings();
            LastError = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            Current = new AppSettings();
            LastError = exception.Message;
        }
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_path, JsonSerializer.Serialize(Current, Options));
    }

    public void Update(Action<AppSettings> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        update(Current);
        Save();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
