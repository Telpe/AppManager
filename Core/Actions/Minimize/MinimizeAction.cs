using AppManager.Core.Models;
using AppManager.OsApi;
using System;
using System.Diagnostics;
using System.Linq;

namespace AppManager.Core.Actions.Minimize
{
    public partial class MinimizeAction : BaseAction, IMinimizeAction
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.Minimize;
        public override string Description => "Minimizes an application window";

        public string? AppName { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public string? WindowTitle { get; set; }

        private Process[] TargetProcessesValue = Array.Empty<Process>();

        public MinimizeAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeSimilarNames = model.IncludeSimilarNames;
            WindowTitle = model.WindowTitle;
        }

        protected override bool CanExecuteAction()
        {
            return !string.IsNullOrEmpty(AppName);
        }

        protected override bool ExecuteAction()
        {
            var processes = ProcessManager.FindProcesses(
                AppName!, 
                IncludeSimilarNames ?? false, 
                requireMainWindow: true,
                WindowTitle);
                    
            if (0 == processes.Length || processes.All(p => p.HasExited))
            {
                Log.WriteLine($"No processes found to minimize: {AppName}");
                return false;
            }

            int successCount = 0;

            foreach (var process in processes)
            {
                IntPtr mainWindowHandle = OSAPI.Current.GetProcessMainWindowHandle(process);
                            
                if (mainWindowHandle == IntPtr.Zero)
                {
                    Log.WriteLine($"No main window found for process: {process.ProcessName}");
                    continue;
                }

                OSAPI.Current.Window.Minimize(mainWindowHandle);


                if (OSAPI.Current.Window.IsMinimized(mainWindowHandle))
                {
                    Log.WriteLine($"Successfully minimized window for: {process.ProcessName}");
                    successCount++;
                }
                else
                {
                    Log.WriteLine($"Failed to minimize window for: {process.ProcessName}");
                }
            }
            
            Log.WriteLine($"Minimized {successCount} windows");

            return 0 < successCount;
        }

        public override ActionModel ToModel()
        {
            return ToActionModel<IMinimizeAction>();
        }
    }
}