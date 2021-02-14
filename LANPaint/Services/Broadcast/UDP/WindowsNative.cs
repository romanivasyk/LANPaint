using System;
using System.Runtime.InteropServices;

namespace LANPaint.Services.Broadcast.UDP
{
    internal static class WindowsNative
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);
    }
}
