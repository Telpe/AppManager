using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public partial class MinimizeAction : BaseAction, IMinimizeAction
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
        private Process[] TargetProcessesValue = Array.Empty<Process>();

        public MinimizeAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeSimilarNames = model.IncludeSimilarNames;
            WindowTitle = model.WindowTitle;
        }

        protected override bool CanExecuteAction()
        {
            TargetProcessesValue = ProcessManager.FindProcesses(
                AppName!, 
                IncludeSimilarNames ?? false, 
                requireMainWindow: true,
                WindowTitle);

            return !string.IsNullOrEmpty(AppName) && !TargetProcessesValue.Any(p => p.HasExited);
        }

        protected override void ExecuteAction()
        {
            var processes = ProcessManager.FindProcesses(
                AppName!, 
                IncludeSimilarNames ?? false, 
                requireMainWindow: true,
                WindowTitle);
                    
            if (0 == processes.Length)
            {
                Log.WriteLine($"No processes found to minimize: {AppName}");
                return;
            }

            foreach (var process in processes)
            {
                IntPtr mainWindowHandle = process.MainWindowHandle;
                            
                if (mainWindowHandle == IntPtr.Zero)
                {
                    Log.WriteLine($"No main window found for process: {process.ProcessName}");
                    continue;
                }

                bool success = ShowWindow(mainWindowHandle, SW_MINIMIZE);
                            
                if (success)
                {
                    Log.WriteLine($"Successfully minimized window for: {process.ProcessName}");
                }
                else
                {
                    Log.WriteLine($"Failed to minimize window for: {process.ProcessName}");
                }
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