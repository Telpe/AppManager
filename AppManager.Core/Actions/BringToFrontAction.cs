using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class BringToFrontAction : BaseAction
    {
        public override string Description => "Brings an application window to the front and makes it topmost temporarily";

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

        public BringToFrontAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            return !string.IsNullOrEmpty(_Model?.AppName) && GetTargetProcess(_Model) != null;
        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(async () =>
            {
                if (_Model == null || string.IsNullOrEmpty(_Model.AppName))
            {
                Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            // Check conditions before executing
            if (!CheckConditions())
            {
                Debug.WriteLine($"Conditions not met for bringing to front {_Model.AppName}");
                return false;
            }

            try
            {
                var process = GetTargetProcess(_Model);
                
                if (process == null)
                {
                    Debug.WriteLine($"No process found to bring to front: {_Model.AppName}");
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

                Debug.WriteLine($"Successfully brought to front: {_Model.AppName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to bring to front {_Model.AppName}: {ex.Message}");
                return false;
            }
                });
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

                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding process to bring to front {model.AppName}: {ex.Message}");
                return null;
            }
        }
    }
}