namespace MadDogs.TaskbarGroups.Common.Extensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public static class ScreenExtensions
    {
        public static void GetDpi(this Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
        {
            var pnt = new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            IntPtr mon = MonitorFromPoint(pnt, 2);
            GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
        }

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hMonitor,
                                                      [In] DpiType dpiType,
                                                      [Out] out uint dpiX,
                                                      [Out] out uint dpiY);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2
    }
}