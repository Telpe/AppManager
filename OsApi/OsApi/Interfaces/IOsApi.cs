using AppManager.OsApi.Events;
using AppManager.OsApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        IWindowControl Window { get; }
        uint CurrentThreadId { get; }

        IntPtr GetProcessMainWindowHandle(Process process);

        [Obsolete("Temporary")]
        bool WindowSetState(IntPtr windowHandle, int state);

        bool WindowSetForeground(IntPtr windowHandle);

        bool WindowSetPosition(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        int ShutdownBlockReasonCreate(IntPtr windowHandle, string reason);

        int ShutdownBlockReasonDestroy(IntPtr windowHandle);

        ObservableEvent<object?, HotkeyModel> KeyEvent { get; }
    }
}
