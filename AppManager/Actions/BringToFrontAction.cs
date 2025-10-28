using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class BringToFrontAction : IAppAction
    {
        public AppActionEnum ActionName => AppActionEnum.BringToFront;
        public string Description => "Brings an application window to the front and makes it topmost temporarily";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public bool CanExecute(string appName, ActionParameters parameters = null)
        {
            return !string.IsNullOrEmpty(appName) && GetTargetProcess(appName, parameters) != null;
        }

        public async Task<bool> ExecuteAsync(string appName, ActionParameters parameters = null)
        {
            try
            {
                var process = GetTargetProcess(appName, parameters);
                
                if (process == null)
                {
                    Debug.WriteLine($"No process found to bring to front: {appName}");
                    return false;
                }

                IntPtr mainWindowHandle = process.MainWindowHandle;
                
                if (mainWindowHandle == IntPtr.Zero)
                {
                    Debug.WriteLine($"No main window found for process: {process.ProcessName}");
                    return false;
                }

                // If window is minimized, restore it first
                if (IsIconic(mainWindowHandle))
                {
                    ShowWindow(mainWindowHandle, SW_RESTORE);
                }
                else
                {
                    ShowWindow(mainWindowHandle, SW_SHOW);
                }

                // Make window topmost temporarily
                SetWindowPos(mainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                
                // Bring to foreground
                SetForegroundWindow(mainWindowHandle);
                
                // Wait a moment, then remove topmost flag
                await Task.Delay(100);
                SetWindowPos(mainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                Debug.WriteLine($"Successfully brought to front: {appName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to bring to front {appName}: {ex.Message}");
                return false;
            }
        }

        private Process GetTargetProcess(string appName, ActionParameters parameters)
        {
            try
            {
                Process[] processes;
                
                if (parameters?.IncludeSimilarNames == true)
                {
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }
                else
                {
                    processes = Process.GetProcessesByName(appName)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }

                // If window title is specified, filter by it
                if (!string.IsNullOrEmpty(parameters?.WindowTitle))
                {
                    processes = processes
                        .Where(p => p.MainWindowTitle.IndexOf(parameters.WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }

                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding process to bring to front {appName}: {ex.Message}");
                return null;
            }
        }
    }
}