global using AppManager.Core.Utilities;
using AppManager.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using AppManager.Core.Keybinds;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Settings
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private static GlobalKeyboardHook? GlobalKeyboardHookValue;

        private static Dictionary<string, string> UnsavedPages = new();

        private System.Timers.Timer CheckIfAppsRunningValue = new();
        public System.Timers.Timer CheckIfAppsRunning { get { return CheckIfAppsRunningValue; } }

        public static readonly AppManager.Core.Version Version;

        static App()
        {
            // Load version from version.json file
            Version = FileManager.LoadVersion();
            Log.WriteLine($"AppManager Version: {Version}");
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

            //string profileToLoad = !string.IsNullOrEmpty(SettingsManager.CurrentSettings.LastUsedProfileName) 
            //    ? SettingsManager.CurrentSettings.LastUsedProfileName 
            //    : ProfileManager.DefaultProfileFilename;

            //// Load the specific profile
            //if (ProfileManager.ProfileExist(profileToLoad))
            //{
            //    ProfileManager.LoadAndSetProfile(profileToLoad);
            //    Log.WriteLine($"Loaded last used profile: {profileToLoad}");
            //}
            //else
            //{
            //    _ = ProfileManager.CurrentProfile;
            //    Log.WriteLine($"Profile '{profileToLoad}' not found, loaded default profile instead");
            //}

            //CheckIfAppsRunningValue.Interval = ProfileManager.CurrentProfile.ScanInterval;
            //CheckIfAppsRunningValue.AutoReset = true;
            //CheckIfAppsRunningValue.Elapsed += new ElapsedEventHandler(CheckRunningHandler);
            //CheckIfAppsRunningValue.Start();
        }

        protected void SetCoreRunning()
        {
            if (!ProcessManager.IsProcessRunning("AppManager.Core"))
            {
                Log.WriteLine("AppManager.Core not running, launching it");

                ActionFactory.CreateAction(new ActionModel
                {
                    ActionType = AppActionTypeEnum.Launch,
                    AppName = "AppManager.Core"
                }).Execute();
                
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.WriteLine("Settings exit");
            Log.Dispose();
            base.OnExit(e);
        }

       
        
    }

}
