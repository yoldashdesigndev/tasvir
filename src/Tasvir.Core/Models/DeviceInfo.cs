namespace Tasvir.Core.Models;

public sealed record DeviceInfo(
    string Serial,
    string Model,
    string? DeviceName,
    DeviceConnectionState State,
    ConnectionMode Mode)
{
    public string DisplayName =>
        string.IsNullOrWhiteSpace(DeviceName) ? (Model ?? Serial) : DeviceName;
}
