using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Tasvir.App.Controls;

public sealed class PhonePreviewHost : HwndHost
{
    private const string WindowClassName = "TasvirPhonePreviewHost";
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipChildren = 0x02000000;
    private const int WsPopup = unchecked((int)0x80000000);
    private const int WsCaption = 0x00C00000;
    private const int WsThickFrame = 0x00040000;
    private const int WsSysMenu = 0x00080000;
    private const int WsMinimizeBox = 0x00020000;
    private const int WsMaximizeBox = 0x00010000;
    private const int GwlStyle = -16;
    private const int SwpFrameChanged = 0x0020;
    private const int SwpShowWindow = 0x0040;
    private const int SwShow = 5;

    private static readonly WindowProcDelegate WindowProc = WindowProcMethod;
    private static bool _classRegistered;

    private IntPtr _hostHandle;
    private IntPtr _childHandle;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        RegisterWindowClass();
        _hostHandle = CreateWindowEx(
            0, WindowClassName, string.Empty,
            WsChild | WsVisible | WsClipChildren,
            0, 0, 10, 10,
            hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return new HandleRef(this, _hostHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Detach();
        DestroyWindow(hwnd.Handle);
        _hostHandle = IntPtr.Zero;
    }

    public void Attach(IntPtr childHandle)
    {
        if (childHandle == IntPtr.Zero || _hostHandle == IntPtr.Zero)
        {
            return;
        }

        Detach();
        _childHandle = childHandle;

        var style = GetWindowLongPtr(childHandle, GwlStyle).ToInt64();
        style &= ~(WsCaption | WsThickFrame | WsSysMenu | WsMinimizeBox | WsMaximizeBox | WsPopup);
        style |= WsChild;
        SetWindowLongPtr(childHandle, GwlStyle, new IntPtr(style));

        SetParent(childHandle, _hostHandle);
        ResizeChild();
        ShowWindow(childHandle, SwShow);
    }

    public void Detach()
    {
        if (_childHandle == IntPtr.Zero)
        {
            return;
        }

        SetParent(_childHandle, IntPtr.Zero);
        _childHandle = IntPtr.Zero;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        ResizeChild();
    }

    private void ResizeChild()
    {
        if (_childHandle == IntPtr.Zero || _hostHandle == IntPtr.Zero ||
            !GetClientRect(_hostHandle, out var rect))
        {
            return;
        }

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width > 0 && height > 0)
        {
            MoveWindow(_childHandle, 0, 0, width, height, true);
            SetWindowPos(_childHandle, IntPtr.Zero, 0, 0, width, height, SwpFrameChanged | SwpShowWindow);
        }
    }

    private static void RegisterWindowClass()
    {
        if (_classRegistered)
        {
            return;
        }

        var windowClass = new WindowClassInfo
        {
            lpfnWndProc = WindowProc,
            hInstance = GetModuleHandle(null),
            lpszClassName = WindowClassName
        };
        RegisterClass(ref windowClass);
        _classRegistered = true;
    }

    private static IntPtr WindowProcMethod(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam) =>
        DefWindowProc(hWnd, message, wParam, lParam);

    private delegate IntPtr WindowProcDelegate(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClassInfo
    {
        public uint style;
        public WindowProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
    private static extern IntPtr CreateWindowEx(
        int exStyle, string className, string windowName, int style,
        int x, int y, int width, int height,
        IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr child, IntPtr parent);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr handle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr handle, int index, IntPtr value);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr handle, out Rect rect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr handle, int command);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr handle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterClassW")]
    private static extern ushort RegisterClass(ref WindowClassInfo windowClass);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
}
