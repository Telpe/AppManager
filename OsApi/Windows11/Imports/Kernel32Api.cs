using System.Runtime.InteropServices;

namespace AppManager.OsApi.Windows11.Imports
{
    public partial class Kernel32Api
    {
        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
        public static partial uint GetCurrentThreadId();
    }
}
