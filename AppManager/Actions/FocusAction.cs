using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class FocusAction : IAppAction
    {
        public AppActionEnum ActionName => AppActionEnum.Focus;
        public string Description => "Brings an application window to the foreground";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

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
                    Debug.WriteLine($"No process found to focus: {appName}");
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

                // Bring window to foreground
                bool success = SetForegroundWindow(mainWindowHandle);
                
                if (success)
                {
                    Debug.WriteLine($"Successfully focused window for: {appName}");
                }
                else
                {
                    Debug.WriteLine($"Failed to focus window for: {appName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to focus {appName}: {ex.Message}");
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

                // Return the first process with a visible window
                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding process for focusing {appName}: {ex.Message}");
                return null;
            }
        }
    }
}