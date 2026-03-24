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
            return User32Api.IsIconic(windowHandle);
        }

        public bool IsMaximized(IntPtr windowHandle)
        {
            return User32Api.IsZoomed(windowHandle);
        }

        public bool IsHidden(IntPtr windowHandle)
        {
            return !User32Api.IsWindowVisible(windowHandle);
        }

        public void SetDefaultState(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowDefault))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void Hide(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Hide))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void Minimize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Minimize))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void ForceMinimize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ForceMinimize))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void Maximize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowMaximized))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void Normalize(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.ShowNormal))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void Restore(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Restore))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
        public void SetFullScreen(IntPtr windowHandle)
        {
        }
        public void Focus(IntPtr windowHandle)
        {
            if (!User32Api.ShowWindow(windowHandle, ShowWindowEnum.Show))
            {
                int errorCode = Marshal.GetLastPInvokeError();
                string message = new Win32Exception(errorCode).Message;
                throw new Exception($"Error ({errorCode}): {message}");
            }
        }
    }
}
