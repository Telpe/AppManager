using AppManager.OsApi.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AppManager.OsApi.Windows11
{
    public class WindowControlApi : IWindowControl
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
        public void SetFullScreen(IntPtr windowHandle)
        {
        }
        public void Focus(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Show))
            {
                ThrowLastError();
            }
        }

        private void ThrowIfInvalidErrorHandle()
        {
            if (1400 == Marshal.GetLastPInvokeError())
            {
                ThrowLastError(1400);
            }
        }

        private void ThrowLastError(int? error = null)
        {
            error ??= Marshal.GetLastPInvokeError();
            throw new Exception($"Win32 error ({error}): {new Win32Exception((int)error).Message}");
        }
    }
}
