using System.Runtime.InteropServices;

namespace AppManager.OsApi.Windows11.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point2D
    {
        public int x;
        public int y;
    }
}
