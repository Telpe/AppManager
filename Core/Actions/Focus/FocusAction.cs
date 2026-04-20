using AppManager.Core.Models;
using AppManager.OsApi;
using System;
using System.Diagnostics;

namespace AppManager.Core.Actions.Focus
{
    public partial class FocusAction : BaseAction, IFocusAction
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.Focus;
        public override string Description => "Brings an application window to the foreground";
        public string? AppName { get; set; }

        public bool? IncludeSimilarNames { get; set; }
        public string? WindowTitle { get; set; }

        public FocusAction(ActionModel model) : base(model)
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
            Process process = ProcessManager.FindProcess(
                AppName!, 
                IncludeSimilarNames ?? false, 
                requireMainWindow: true,
                WindowTitle) ?? throw new Exception($"No process found to focus: {AppName}");
            
            IntPtr mainWindowHandle = OSAPI.Current.GetProcessMainWindowHandle(process);

            if (mainWindowHandle == IntPtr.Zero)
            {
                throw new Exception($"No main window found for process: {process.ProcessName}");
            }

            OSAPI.Current.Window.Focus(mainWindowHandle);

            if (true)// TODO: Verify if the window is now focused (may require additional logic to check foreground window)
            {
                Log.WriteLine($"Successfully focused window for: {AppName}");
                return true;
            }
            else
            {
                throw new Exception($"Failed to focus window for: {AppName}");
            }
        }

        public override ActionModel ToModel()
        {
            return ToActionModel<IFocusAction>();
        }
    }
}