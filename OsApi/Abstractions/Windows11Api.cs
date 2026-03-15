using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace AppManager.OsApi.Abstractions
{
    public partial class Windows11Api : IOsApi
    {
        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
        private static partial uint GetCurrentThreadId();

        public static uint CurrentThreadId => GetCurrentThreadId();


        [LibraryImport("USER32", EntryPoint = "ShutdownBlockReasonCreate", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        public static partial int ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

        [LibraryImport("USER32", EntryPoint = "ShutdownBlockReasonDestroy", SetLastError = true)]
        public static partial int ShutdownBlockReasonDestroy(IntPtr hWnd);
    }
}
