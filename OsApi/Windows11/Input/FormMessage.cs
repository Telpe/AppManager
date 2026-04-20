using System;
using System.Runtime.InteropServices;

namespace AppManager.OsApi.Windows11.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FormMessage
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point2D pt;
    }
}
