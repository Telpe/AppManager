using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Windows11.GUI;
using AppManager.OsApi.Windows11.Imports;
using AppManager.OsApi.Windows11.Input;
using System;
using System.Diagnostics;

namespace AppManager.OsApi.Windows11
{
    public class Windows11Api : IOsApi
    {
        public IntPtr HWND_TOPMOST { get; } = new IntPtr(-1);
        public IntPtr HWND_NOTOPMOST { get; } = new IntPtr(-2);
        public uint SWP_NOMOVE { get; } = 0x0002;
        public uint SWP_NOSIZE { get; } = 0x0001;
        public uint SWP_NOZORDER { get; } = 0x0004;
        public uint SWP_HIDEWINDOW { get; } = 0x0080;

        public IInputControl Input { get; } = new InputControl();

        public IWindowControl Window { get; } = new WindowControl();

        public uint CurrentThreadId { get => Kernel32Api.GetCurrentThreadId(); }

        

        

        public Windows11Api() 
        {
            
        }

        public IntPtr GetProcessMainWindowHandle(Process process)
        {
            return process.MainWindowHandle != IntPtr.Zero ? process.MainWindowHandle : throw new NullReferenceException($"No main window found for process: {process.ProcessName}");
        }


        private void KeyEventChangedCount(object? sender, int newCount)
        { 
            Debug.WriteLine($"KeyEvent handler count changed: {newCount}");
        }

        
    }
}
