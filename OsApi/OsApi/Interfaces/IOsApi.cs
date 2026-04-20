using System;
using System.Diagnostics;

namespace AppManager.OsApi.Interfaces
{
    public interface IOsApi
    {
        IntPtr HWND_TOPMOST { get; }
        IntPtr HWND_NOTOPMOST { get; }
        uint SWP_NOMOVE { get; }
        uint SWP_NOSIZE { get; }
        uint SWP_NOZORDER { get; }
        uint SWP_HIDEWINDOW { get; }
        IInputControl Input { get; }
        IWindowControl Window { get; }
        uint CurrentThreadId { get; }

        IntPtr GetProcessMainWindowHandle(Process process);


        

        
    }
}
