global using AppManager.Core.Utils;
using AppManager.Core.Actions;
using AppManager.Core.Models;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Timers;
using System.Windows;

namespace AppManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (ShouldITerminate())
            {
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();

            SetCoreRunning();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.WriteLine("AppManager is shutting down.");
                
                // Dispose of logging resources
                Log.Dispose();
            }
            catch (Exception ex)
            {
                // Log any cleanup errors, but don't prevent shutdown
                System.Diagnostics.Debug.WriteLine($"Error during application shutdown: {ex.Message}");
            }
            finally
            {
                // Always call the base OnExit to ensure proper WPF cleanup
                base.OnExit(e);
            }
        }

        protected bool ShouldITerminate()
        {
            if (CheckSelfRunning(out Process? notSelf) && null != notSelf)
            {
                var bringToFrontAction = ActionManager.CreateAction(new ActionModel
                {
                    ActionType = AppActionTypeEnum.BringToFront,
                    AppName = notSelf.ProcessName
                }, notSelf);

                if (bringToFrontAction.CanExecute()) { bringToFrontAction.Execute(); }

                Log.WriteLine($"{notSelf.ProcessName} is already running, bringing existing instance to front");

                return true;
            }

            return false;
        }

        protected bool CheckSelfRunning(out Process? notSelf)
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = ProcessManager.FindProcesses(currentProcess.ProcessName, false, null, true, false, currentProcess.Id);

            if (processes.Length > 0)
            {
                notSelf = processes[0];
                return true;
            }

            notSelf = null;
            return false;
        }

        protected void SetCoreRunning()
        {
            if (!ProcessManager.IsProcessRunning("AppManager.Core"))
            {
                Log.WriteLine("AppManager.Core not running, launching it");
                
                ActionManager.ExecuteAction(new ActionModel
                {
                    ActionType = AppActionTypeEnum.Launch,
                    AppName = "AppManager.Core"
                });
            }
        }

    }
}
