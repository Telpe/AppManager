using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Windows11.Imports;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AppManager.OsApi.Windows11.GUI
{
    public class WindowControl : IWindowControl
    {
        public bool IsMinimized(IntPtr windowHandle)
        {
            bool result = User32Api.IsIconic(windowHandle);
            ThrowIfInvalidErrorHandle();

            return result;
        }

        public bool IsMaximized(IntPtr windowHandle)
        {
            bool result = User32Api.IsZoomed(windowHandle);
            ThrowIfInvalidErrorHandle();

            return result;
        }

        public bool IsHidden(IntPtr windowHandle)
        {
            bool result = !User32Api.IsWindowVisible(windowHandle);
            ThrowIfInvalidErrorHandle();

            return result;
        }


        public void SetDefaultState(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowDefault))
            {
                ThrowLastError();
            }
        }

        public void Hide(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Hide))
            {
                ThrowLastError();
            }
        }
        public void Minimize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Minimize))
            {
                ThrowLastError();
            }
        }
        public void ForceMinimize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ForceMinimize))
            {
                ThrowLastError();
            }
        }
        public void Maximize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowMaximized))
            {
                ThrowLastError();
            }
        }
        public void Normalize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowNormal))
            {
                ThrowLastError();
            }
        }
        public void Restore(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Restore))
            {
                ThrowLastError();
            }
        }
        public void Show(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Show))
            {
                ThrowLastError();
            }
        }
        public void SetFullScreen(IntPtr windowHandle)
        {
            throw new NotImplementedException("Full screen mode is not implemented yet.");
        }


        public void Focus(IntPtr windowHandle)
        {
            IntPtr fg = User32Api.GetForegroundWindow();

            if (IsHidden(windowHandle)) 
            { User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowNoActivate); }

            if (IsMinimized(windowHandle))
            { Restore(windowHandle); }

            if (!User32Api.SetForegroundWindow(windowHandle) || User32Api.GetForegroundWindow() != windowHandle)
            {
                ForceFocus(windowHandle, fg);
            }
        }

        public void ForceFocus(IntPtr windowHandle, IntPtr? fallbackWindow = null)
        {
            IntPtr fg = User32Api.GetForegroundWindow();
            uint fgThread = User32Api.GetWindowThreadProcessId(fg, IntPtr.Zero);
            uint appThread = User32Api.GetWindowThreadProcessId(windowHandle, IntPtr.Zero);

            User32Api.AttachThreadInput(fgThread, appThread, true);

            User32Api.SetForegroundWindow(windowHandle);

            User32Api.AttachThreadInput(fgThread, appThread, false);

            if (User32Api.GetForegroundWindow() != windowHandle)
            {
                if (fallbackWindow is not null) 
                { Show(fallbackWindow.Value); }

                ThrowLastError(); 
            }
        }

        public bool SetPosition(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags)
        {
            return User32Api.SetWindowPos(windowHandle, insertAfter, x, y, width, height, flags);
        }





        public int ShutdownBlockReasonCreate(IntPtr windowHandle, string reason)
        {
            return User32Api.ShutdownBlockReasonCreate(windowHandle, reason);
        }

        public int ShutdownBlockReasonDestroy(IntPtr windowHandle)
        {
            return User32Api.ShutdownBlockReasonDestroy(windowHandle);
        }


        private static void ThrowIfInvalidErrorHandle()
        {
            if (1400 == Marshal.GetLastPInvokeError())
            {
                ThrowLastError(1400);
            }
        }

        private static void ThrowLastError(int? error = null)
        {
            error ??= Marshal.GetLastPInvokeError();
            throw new Exception($"Win32 error ({error}): {new Win32Exception((int)error).Message}");
        }
    }
}
