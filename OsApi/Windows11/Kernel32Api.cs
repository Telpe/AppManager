using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AppManager.OsApi.Windows11
{
    public partial class Kernel32Api
    {
        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
        public static partial uint GetCurrentThreadId();
    }
}
