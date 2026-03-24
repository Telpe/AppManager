global using AppManager.Core.Utilities;
using AppManager.Core;
using AppManager.Core.Actions;
using AppManager.Core.Models;
using System;
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
        static App()
        {
            // Initialize core dependencies before any instance of App is created
            if (!Dependencies.Initialized) { throw new Exception("Core Dependencies not initialized."); }
        }

        public App()
        {
            if (Shared.ShouldITerminateBringingOtherToFront())
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

        protected void SetCoreRunning()
        {
            if (!ProcessManager.IsProcessRunning("AppManager.Core"))
            {
                Log.WriteLine("AppManager.Core not running, launching it");
                
                ActionFactory.CreateAction(new()
                {
                    ActionType = ActionTypeEnum.Launch,
                    AppName = "AppManager.Core"
                }).Execute();
            }
        }

    }
}
