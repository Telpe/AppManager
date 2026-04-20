using System;

namespace AppManager.OsApi.Interfaces
{
    public interface IWindowControl
    {
        bool IsMinimized(IntPtr windowHandle);
        bool IsMaximized(IntPtr windowHandle);
        bool IsHidden(IntPtr windowHandle);

        void SetDefaultState(IntPtr windowHandle);

        void Hide(IntPtr windowHandle);
        void Show(IntPtr windowHandle);
        void Minimize(IntPtr windowHandle);
        void ForceMinimize(IntPtr windowHandle);
        void Maximize(IntPtr windowHandle);
        void Normalize(IntPtr windowHandle);
        void Restore(IntPtr windowHandle);
        void SetFullScreen(IntPtr windowHandle);

        void Focus(IntPtr windowHandle);
        void ForceFocus(IntPtr windowHandle, IntPtr? fallbackWindow = null);
        bool SetPosition(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        int ShutdownBlockReasonCreate(IntPtr windowHandle, string reason);
        int ShutdownBlockReasonDestroy(IntPtr windowHandle);
    }
}
