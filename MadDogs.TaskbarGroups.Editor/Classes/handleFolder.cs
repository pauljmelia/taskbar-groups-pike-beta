namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ShFileInfo
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    internal static class HandleFolder
    {
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath,
                                                  uint dwFileAttributes,
                                                  out ShFileInfo psfi,
                                                  uint cbFileInfo,
                                                  uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon GetFolderIcon(string path)
        {
            const uint FLAGS = SHGFI_ICON | SHGFI_LARGEICON;
            var shfi = new ShFileInfo();

            IntPtr res = SHGetFileInfo(path, FILE_ATTRIBUTE_DIRECTORY, out shfi, (uint)Marshal.SizeOf(shfi), FLAGS);

            if (res == IntPtr.Zero)
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            Icon.FromHandle(shfi.hIcon);
            var icon = (Icon)Icon.FromHandle(shfi.hIcon)
                                 .Clone();

            DestroyIcon(shfi.hIcon);

            return icon;
        }
    }
}