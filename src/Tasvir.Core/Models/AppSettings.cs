namespace Tasvir.Core.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en";

    public ConnectionMode PreferredConnection { get; set; } = ConnectionMode.Usb;

    public int MaxWidth { get; set; } = 1080;

    public int MaxHeight { get; set; } = 1920;

    public int MaxFps { get; set; } = 60;

    public int BitrateMbps { get; set; } = 8;

    public int Rotation { get; set; }

    public bool AutoReconnect { get; set; } = true;

    public bool ShowDiagnostics { get; set; } = true;
}
