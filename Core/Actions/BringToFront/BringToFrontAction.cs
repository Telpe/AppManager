using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AppManager.OsApi;

namespace AppManager.Core.Actions.BringToFront
{
    public partial class BringToFrontAction : BaseAction, IBringToFrontAction
    {
        public string? AppName { get; set; }
        public override ActionTypeEnum ActionType => ActionTypeEnum.BringToFront;
        public override string Description => "Brings an application window to the front and makes it topmost temporarily";

        public string? WindowTitle { get; set; }
        public int? ProcessLastId { get; set; }

        

        

        public BringToFrontAction(ActionModel model) : base(model)
        { 
            AppName = model.AppName;
            WindowTitle = model.WindowTitle;
            ProcessLastId = model.ProcessLastId;
        }

        protected override bool CanExecuteAction()
        {
            return !String.IsNullOrEmpty(AppName);
        }

        protected override bool ExecuteAction()
        {
            Log.WriteLine($"\nBringing {AppName}'s {(String.IsNullOrWhiteSpace(WindowTitle) ? "main": WindowTitle)} window to the front.");
            Process[] processes = ProcessManager.FindProcesses(
                        AppName!,
                        includeSimilarNames: false,
                        requireMainWindow: true,
                        WindowTitle);

            try
            {
                Process? targetProcess;

                if (-1 < (ProcessLastId ?? -1))
                {
                    targetProcess = processes.FirstOrDefault(p => p.Id == ProcessLastId);
                }
                else
                {
                    targetProcess = processes.FirstOrDefault();
                }

                if (targetProcess is null) 
                { 
                    Log.WriteLine($"No running process found for: {AppName}");
                    return false; 
                }

                IntPtr testHandle = targetProcess.MainWindowHandle;
                Log.WriteLine($"Found process: {targetProcess.ProcessName}");
                Log.WriteLine($" (ID: {targetProcess.Id}), ");
                Log.WriteLine($"MainWindowHandle: {testHandle}");
                Log.WriteLine($"MainWindowTitle: {targetProcess.MainWindowTitle}");
                IntPtr mainWindowHandle = OSAPI.Current.GetProcessMainWindowHandle(targetProcess);

                // If window is minimized, restore it first
                if (OSAPI.Current.WindowIsMinimized(mainWindowHandle))
                {
                    OSAPI.Current.WindowRestore(mainWindowHandle);
                }
                else
                {
                    OSAPI.Current.WindowShow(mainWindowHandle);
                }

                // Make window topmost temporarily
                OSAPI.Current.WindowSetPosition(mainWindowHandle, OSAPI.Current.HWND_TOPMOST, 0, 0, 0, 0, OSAPI.Current.SWP_NOMOVE | OSAPI.Current.SWP_NOSIZE);
                
                // Bring to foreground
                OSAPI.Current.WindowSetForeground(mainWindowHandle);
                
                // Wait a moment, then remove topmost flag
                Thread.Sleep(CoreConstants.StandardUIDelay);
                OSAPI.Current.WindowSetPosition(mainWindowHandle, OSAPI.Current.HWND_NOTOPMOST, 0, 0, 0, 0, OSAPI.Current.SWP_NOMOVE | OSAPI.Current.SWP_NOSIZE);
                Log.WriteLine($"Successfully brought to front: {AppName}");
                return true;
            }
            finally
            {
                foreach (var p in processes)
                {
                    p.Dispose();
                }
            }
            
        }

        public override ActionModel ToModel()
        {
            return ToActionModel<IBringToFrontAction>();
        }
    }
}