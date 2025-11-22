using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class MinimizeAction : BaseAction
    {
        public override string Description => "Minimizes an application window";

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;

        public MinimizeAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            return !string.IsNullOrEmpty(_Model?.AppName) && GetTargetProcesses(_Model).Any();
        }

        public override async Task<bool> ExecuteAsync()
        {
            if (_Model == null || string.IsNullOrEmpty(_Model.AppName))
            {
                Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            // Check conditions before executing
            if (!CheckConditions())
            {
                Debug.WriteLine($"Conditions not met for minimizing {_Model.AppName}");
                return false;
            }

            try
            {
                var processes = GetTargetProcesses(_Model);
                
                if (!processes.Any())
                {
                    Debug.WriteLine($"No processes found to minimize: {_Model.AppName}");
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
                Debug.WriteLine($"Failed to minimize {_Model.AppName}: {ex.Message}");
                return false;
            }
        }

        private Process[] GetTargetProcesses(ActionModel model)
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

                return processes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding processes to minimize {model.AppName}: {ex.Message}");
                return new Process[0];
            }
        }
    }
}