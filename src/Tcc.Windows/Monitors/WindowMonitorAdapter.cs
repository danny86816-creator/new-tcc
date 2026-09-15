using System.Runtime.InteropServices;

namespace Tcc.Windows.Monitors;

public sealed class WindowMonitorFacts
{
    internal WindowMonitorFacts(
        string deviceName,
        int workAreaX,
        int workAreaY,
        int workAreaWidth,
        int workAreaHeight,
        uint dpi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceName);
        if (workAreaWidth <= 0 || workAreaHeight <= 0 || dpi == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workAreaWidth));
        }

        _ = checked(workAreaX + workAreaWidth);
        _ = checked(workAreaY + workAreaHeight);

        DeviceName = deviceName;
        WorkAreaX = workAreaX;
        WorkAreaY = workAreaY;
        WorkAreaWidth = workAreaWidth;
        WorkAreaHeight = workAreaHeight;
        Dpi = dpi;
    }

    public string DeviceName { get; }

    public int WorkAreaX { get; }

    public int WorkAreaY { get; }

    public int WorkAreaWidth { get; }

    public int WorkAreaHeight { get; }

    public uint Dpi { get; }

    public (int X, int Y, int Width, int Height) FitBounds(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        int fittedWidth = Math.Min(width, WorkAreaWidth);
        int fittedHeight = Math.Min(height, WorkAreaHeight);
        long maximumX = (long)WorkAreaX + WorkAreaWidth - fittedWidth;
        long maximumY = (long)WorkAreaY + WorkAreaHeight - fittedHeight;
        int fittedX = (int)Math.Clamp((long)x, WorkAreaX, maximumX);
        int fittedY = (int)Math.Clamp((long)y, WorkAreaY, maximumY);
        return (fittedX, fittedY, fittedWidth, fittedHeight);
    }
}

public sealed class WindowMonitorAdapter
{
    private const uint MonitorDefaultToNearest = 2;
    private const uint MonitorDefaultToPrimary = 1;
    private const uint NoZOrderNoActivate = 0x0014;

    public WindowMonitorAdapter()
    {
    }

    public WindowMonitorFacts? Capture(nint windowHandle)
    {
        _ = GetType();
        if (windowHandle == 0 || !IsWindow(windowHandle))
        {
            return null;
        }

        nint monitor = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        if (monitor == 0)
        {
            monitor = MonitorFromWindow(windowHandle, MonitorDefaultToPrimary);
        }

        if (monitor == 0)
        {
            return null;
        }

        NativeMonitorInfo info = new()
        {
            Size = (uint)Marshal.SizeOf<NativeMonitorInfo>(),
            DeviceName = string.Empty,
        };
        if (!GetMonitorInfo(monitor, ref info))
        {
            return null;
        }

        uint dpi = GetDpiForWindow(windowHandle);
        long width = (long)info.Work.Right - info.Work.Left;
        long height = (long)info.Work.Bottom - info.Work.Top;
        if (dpi == 0 || width <= 0 || height <= 0 || width > int.MaxValue || height > int.MaxValue
            || string.IsNullOrWhiteSpace(info.DeviceName))
        {
            return null;
        }

        return new WindowMonitorFacts(
            info.DeviceName,
            info.Work.Left,
            info.Work.Top,
            (int)width,
            (int)height,
            dpi);
    }

    public bool EnsureVisible(nint windowHandle)
    {
        WindowMonitorFacts? facts = Capture(windowHandle);
        if (facts is null || !GetWindowRect(windowHandle, out NativeRect rectangle))
        {
            return false;
        }

        long width = (long)rectangle.Right - rectangle.Left;
        long height = (long)rectangle.Bottom - rectangle.Top;
        if (width <= 0 || height <= 0 || width > int.MaxValue || height > int.MaxValue)
        {
            return false;
        }

        (int X, int Y, int Width, int Height) fitted = facts.FitBounds(
            rectangle.Left,
            rectangle.Top,
            (int)width,
            (int)height);
        if (fitted.X == rectangle.Left
            && fitted.Y == rectangle.Top
            && fitted.Width == width
            && fitted.Height == height)
        {
            return true;
        }

        return SetWindowPos(
            windowHandle,
            0,
            fitted.X,
            fitted.Y,
            fitted.Width,
            fitted.Height,
            NoZOrderNoActivate);
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitorHandle, ref NativeMonitorInfo monitorInfo);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint windowHandle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(nint windowHandle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(nint windowHandle, out NativeRect rectangle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        nint windowHandle,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeMonitorInfo
    {
        internal uint Size;
        internal NativeRect Monitor;
        internal NativeRect Work;
        internal uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string DeviceName;
    }
}
