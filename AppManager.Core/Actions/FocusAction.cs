using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class FocusAction : BaseAction
    {
        public override string Description => "Brings an application window to the foreground";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public FocusAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            return !string.IsNullOrEmpty(_Model?.AppName) && GetTargetProcess(_Model) != null;
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
                Debug.WriteLine($"Conditions not met for focusing {_Model.AppName}");
                return false;
            }

            try
            {
                var process = GetTargetProcess(_Model);
                
                if (process == null)
                {
                    Debug.WriteLine($"No process found to focus: {_Model.AppName}");
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
                    Debug.WriteLine($"Successfully focused window for: {_Model.AppName}");
                }
                else
                {
                    Debug.WriteLine($"Failed to focus window for: {_Model.AppName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to focus {_Model.AppName}: {ex.Message}");
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