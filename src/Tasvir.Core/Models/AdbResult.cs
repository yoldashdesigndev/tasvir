namespace Tasvir.Core.Models;

public sealed record AdbResult(bool Success, string Message)
{
    public static AdbResult Ok(string message = "") => new(true, message);

    public static AdbResult Fail(string message) => new(false, message);
}
