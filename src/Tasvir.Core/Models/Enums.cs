namespace Tasvir.Core.Models;

public enum ConnectionMode
{
    Usb,
    Wifi
}

public enum DeviceConnectionState
{
    Offline,
    Connecting,
    Connected,
    Unauthorized,
    Error
}

public enum MirroringState
{
    Stopped,
    Starting,
    Running,
    Error
}
