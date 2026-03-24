using AppManager.OsApi;
using AppManager.OsApi.Events;
using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
//using System.Windows.Forms;
using System.Windows.Input;
//using System.Windows.Interop;

namespace AppManager.OsApi.Windows11
{
    public partial class Windows11Api : IOsApi
    {
        public IntPtr HWND_TOPMOST { get; } = new IntPtr(-1);
        public IntPtr HWND_NOTOPMOST { get; } = new IntPtr(-2);
        public uint SWP_NOMOVE { get; } = 0x0002;
        public uint SWP_NOSIZE { get; } = 0x0001;
        public uint SWP_NOZORDER { get; } = 0x0004;
        public uint SWP_HIDEWINDOW { get; } = 0x0080;

        public IWindowControl Window { get; } = new WindowControlApi();

        public uint CurrentThreadId { get => Kernel32Api.GetCurrentThreadId(); }

        

        public ObservableEvent<object?, HotkeyModel> KeyEvent { get; } = new();

        public Windows11Api() 
        {
            KeyEvent.CountChangedEvent += KeyEventChangedCount;
        }

        public IntPtr GetProcessMainWindowHandle(Process process)
        {
            return process.MainWindowHandle != IntPtr.Zero ? process.MainWindowHandle : throw new NullReferenceException($"No main window found for process: {process.ProcessName}");
        }


        [Obsolete("Temporary")]
        public bool WindowSetState(IntPtr windowHandle, int state)
        {
            return User32Api.ShowWindow(windowHandle, (ShowWindowEnum)state);
        }

        public bool WindowSetForeground(IntPtr windowHandle)
        {
            return User32Api.SetForegroundWindow(windowHandle);
        }

        public bool WindowSetPosition(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags)
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


        private void KeyEventChangedCount(object? sender, int newCount)
        { 
            Debug.WriteLine($"KeyEvent handler count changed: {newCount}");
        }

        
    }
}
