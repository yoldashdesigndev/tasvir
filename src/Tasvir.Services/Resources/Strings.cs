using System.Globalization;
using System.Resources;

namespace Tasvir.Services.Resources;

public static class Strings
{
    private static readonly ResourceManager Manager =
        new("Tasvir.Services.Resources.Strings", typeof(Strings).Assembly);

    public static CultureInfo Culture
    {
        get => CultureInfo.CurrentUICulture;
        set => CultureInfo.CurrentUICulture = value;
    }

    public static string AppTitle => Get(nameof(AppTitle));
    public static string Tagline => Get(nameof(Tagline));
    public static string NavMirror => Get(nameof(NavMirror));
    public static string NavSettings => Get(nameof(NavSettings));
    public static string NavAbout => Get(nameof(NavAbout));
    public static string StatusDisconnected => Get(nameof(StatusDisconnected));
    public static string StatusConnected => Get(nameof(StatusConnected));
    public static string StatusConnecting => Get(nameof(StatusConnecting));
    public static string LanguageSwitch => Get(nameof(LanguageSwitch));
    public static string MirrorTitle => Get(nameof(MirrorTitle));
    public static string NoDevice => Get(nameof(NoDevice));
    public static string DevicePlaceholder => Get(nameof(DevicePlaceholder));
    public static string Usb => Get(nameof(Usb));
    public static string Wifi => Get(nameof(Wifi));
    public static string StartMirroring => Get(nameof(StartMirroring));
    public static string StopMirroring => Get(nameof(StopMirroring));
    public static string QuickSettings => Get(nameof(QuickSettings));
    public static string Diagnostics => Get(nameof(Diagnostics));
    public static string PreviewPlaceholder => Get(nameof(PreviewPlaceholder));
    public static string ConnectionInfo => Get(nameof(ConnectionInfo));
    public static string Resolution => Get(nameof(Resolution));
    public static string Bitrate => Get(nameof(Bitrate));
    public static string FrameRate => Get(nameof(FrameRate));
    public static string EngineStatus => Get(nameof(EngineStatus));
    public static string SettingsTitle => Get(nameof(SettingsTitle));
    public static string Language => Get(nameof(Language));
    public static string Quality => Get(nameof(Quality));
    public static string Connection => Get(nameof(Connection));
    public static string AutoReconnect => Get(nameof(AutoReconnect));
    public static string AboutTitle => Get(nameof(AboutTitle));
    public static string AboutDescription => Get(nameof(AboutDescription));
    public static string Version => Get(nameof(Version));
    public static string Licenses => Get(nameof(Licenses));
    public static string LicensesText => Get(nameof(LicensesText));
    public static string Copyright => Get(nameof(Copyright));
    public static string Save => Get(nameof(Save));
    public static string Cancel => Get(nameof(Cancel));
    public static string English => Get(nameof(English));
    public static string Turkish => Get(nameof(Turkish));
    public static string StatusUnauthorized => Get(nameof(StatusUnauthorized));
    public static string StatusOffline => Get(nameof(StatusOffline));
    public static string StatusError => Get(nameof(StatusError));
    public static string Refresh => Get(nameof(Refresh));
    public static string AdbVersion => Get(nameof(AdbVersion));
    public static string NotAvailable => Get(nameof(NotAvailable));
    public static string ConnectWireless => Get(nameof(ConnectWireless));
    public static string WirelessTitle => Get(nameof(WirelessTitle));
    public static string HostLabel => Get(nameof(HostLabel));
    public static string PortLabel => Get(nameof(PortLabel));
    public static string Connect => Get(nameof(Connect));
    public static string WirelessInstructions => Get(nameof(WirelessInstructions));
    public static string InvalidHost => Get(nameof(InvalidHost));
    public static string InvalidPort => Get(nameof(InvalidPort));
    public static string ConnectionFailed => Get(nameof(ConnectionFailed));
    public static string AdbNotFound => Get(nameof(AdbNotFound));
    public static string DevicesFound => Get(nameof(DevicesFound));
    public static string LastError => Get(nameof(LastError));
    public static string Disconnect => Get(nameof(Disconnect));
    public static string UsbInstructions => Get(nameof(UsbInstructions));
    public static string DeviceSerial => Get(nameof(DeviceSerial));
    public static string MirrorStarting => Get(nameof(MirrorStarting));
    public static string MirrorRunning => Get(nameof(MirrorRunning));
    public static string MirrorStopped => Get(nameof(MirrorStopped));
    public static string MirrorFailed => Get(nameof(MirrorFailed));
    public static string Fullscreen => Get(nameof(Fullscreen));
    public static string NoDeviceSelected => Get(nameof(NoDeviceSelected));
    public static string ScrcpyNotFound => Get(nameof(ScrcpyNotFound));
    public static string PairMode => Get(nameof(PairMode));
    public static string PairCode => Get(nameof(PairCode));
    public static string PairPort => Get(nameof(PairPort));
    public static string PairInstructions => Get(nameof(PairInstructions));
    public static string InvalidPairCode => Get(nameof(InvalidPairCode));
    public static string Pairing => Get(nameof(Pairing));
    public static string Paired => Get(nameof(Paired));
    public static string Rotation => Get(nameof(Rotation));
    public static string Reconnecting => Get(nameof(Reconnecting));
    public static string DeviceDisconnected => Get(nameof(DeviceDisconnected));
    public static string DeviceCount => Get(nameof(DeviceCount));
    public static string Monitoring => Get(nameof(Monitoring));
    public static string Active => Get(nameof(Active));
    public static string Paused => Get(nameof(Paused));
    public static string SettingsError => Get(nameof(SettingsError));

    private static string Get(string key) => Manager.GetString(key, Culture) ?? key;
}
