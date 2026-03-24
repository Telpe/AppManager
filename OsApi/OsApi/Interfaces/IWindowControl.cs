using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.OsApi.Interfaces
{
    public interface IWindowControl
    {
        bool IsMinimized(IntPtr windowHandle);
        bool IsMaximized(IntPtr windowHandle);
        bool IsHidden(IntPtr windowHandle);

        void SetDefaultState(IntPtr windowHandle);
        void Hide(IntPtr windowHandle);
        void Minimize(IntPtr windowHandle);
        void ForceMinimize(IntPtr windowHandle);
        void Maximize(IntPtr windowHandle);
        void Normalize(IntPtr windowHandle);
        void Restore(IntPtr windowHandle);
        void SetFullScreen(IntPtr windowHandle);
        void Focus(IntPtr windowHandle);
    }
}
