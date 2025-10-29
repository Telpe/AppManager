using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class FocusAction : IAppAction
    {
        public string Description => "Brings an application window to the foreground";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public bool CanExecute(ActionModel model)
        {
            return !string.IsNullOrEmpty(model?.AppName) && GetTargetProcess(model) != null;
        }

        public async Task<bool> ExecuteAsync(ActionModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.AppName))
            {
                Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            try
            {
                var process = GetTargetProcess(model);
                
                if (process == null)
                {
                    Debug.WriteLine($"No process found to focus: {model.AppName}");
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
                    Debug.WriteLine($"Successfully focused window for: {model.AppName}");
                }
                else
                {
                    Debug.WriteLine($"Failed to focus window for: {model.AppName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to focus {model.AppName}: {ex.Message}");
                return false;
            }
        }

        private Process GetTargetProcess(ActionModel model)
        {
            try
            {
                Process[] processes;
                
                if (model.IncludeSimilarNames)
                {
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(model.AppName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }
                else
                {
                    processes = Process.GetProcessesByName(model.AppName)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }

                // If window title is specified, filter by it
                if (!string.IsNullOrEmpty(model.WindowTitle))
                {
                    processes = processes
                        .Where(p => p.MainWindowTitle.IndexOf(model.WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }

                // Return the first process with a visible window
                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding process for focusing {model.AppName}: {ex.Message}");
                return null;
            }
        }
    }
}