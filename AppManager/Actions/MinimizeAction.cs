using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class MinimizeAction : IAppAction
    {
        public AppActionEnum ActionName => AppActionEnum.Minimize;
        public string Description => "Minimizes an application window";

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;

        public bool CanExecute(string appName, ActionParameters parameters = null)
        {
            return !string.IsNullOrEmpty(appName) && GetTargetProcesses(appName, parameters).Any();
        }

        public async Task<bool> ExecuteAsync(string appName, ActionParameters parameters = null)
        {
            try
            {
                var processes = GetTargetProcesses(appName, parameters);
                
                if (!processes.Any())
                {
                    Debug.WriteLine($"No processes found to minimize: {appName}");
                    return false;
                }

                bool allMinimized = true;

                foreach (var process in processes)
                {
                    try
                    {
                        IntPtr mainWindowHandle = process.MainWindowHandle;
                        
                        if (mainWindowHandle == IntPtr.Zero)
                        {
                            Debug.WriteLine($"No main window found for process: {process.ProcessName}");
                            continue;
                        }

                        bool success = ShowWindow(mainWindowHandle, SW_MINIMIZE);
                        
                        if (success)
                        {
                            Debug.WriteLine($"Successfully minimized window for: {process.ProcessName}");
                        }
                        else
                        {
                            Debug.WriteLine($"Failed to minimize window for: {process.ProcessName}");
                            allMinimized = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to minimize process {process.ProcessName}: {ex.Message}");
                        allMinimized = false;
                    }
                }

                return allMinimized;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to minimize {appName}: {ex.Message}");
                return false;
            }
        }

        private Process[] GetTargetProcesses(string appName, ActionParameters parameters)
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

                return processes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding processes to minimize {appName}: {ex.Message}");
                return new Process[0];
            }
        }
    }
}