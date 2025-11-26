using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public partial class MinimizeAction : BaseAction
    {
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Minimize;
        public override string Description => "Minimizes an application window";

        public string? AppName { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public string? WindowTitle { get; set; }

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;
        private Process[] TargetProcessesStored = Array.Empty<Process>();

        public MinimizeAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeSimilarNames = model.IncludeSimilarNames;
            WindowTitle = model.WindowTitle;
        }

        protected override bool CanExecuteAction()
        {
            TargetProcessesStored = GetTargetProcesses();

            return !string.IsNullOrEmpty(AppName) && !TargetProcessesStored.Any(p => p.HasExited);
        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    var processes = GetTargetProcesses();
                    
                    if (!processes.Any())
                    {
                        Debug.WriteLine($"No processes found to minimize: {AppName}");
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
                    Debug.WriteLine($"Failed to minimize {AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        private Process[] GetTargetProcesses()
        {
            try
            {
                Process[] processes;
                
                if (IncludeSimilarNames??false)
                {
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(AppName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }
                else
                {
                    processes = Process.GetProcessesByName(AppName)
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }

                // If window title is specified, filter by it
                if (!string.IsNullOrEmpty(WindowTitle))
                {
                    processes = processes
                        .Where(p => p.MainWindowTitle.IndexOf(WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }

                return processes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding processes to minimize {AppName}: {ex.Message}");
                return Array.Empty<Process>();
            }
        }

        public override ActionModel ToModel()
        {
            return new ActionModel
            {
                AppName = AppName,
                ActionType = ActionType,
                IncludeSimilarNames = IncludeSimilarNames,
                WindowTitle = WindowTitle,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}