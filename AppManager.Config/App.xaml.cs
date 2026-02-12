global using AppManager.Core.Utilities;
using AppManager.Config.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using AppManager.Core.Keybinds;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Config
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

        public const string CommandPrefix = "/";
        public const string PageCommand = CommandPrefix + "page";
        public const string ItemCommand = CommandPrefix + "item";

        static App()
        {
            // Load version from version.json file
            Version = FileManager.LoadVersion();
            Log.WriteLine($"AppManager Config Version: {Version}");
        }

        public App()
        {
            InitializeComponent();

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

        public void App_Startup(object sender, StartupEventArgs e)
        {
            string? page = null;
            string? item = null;

            

            for (int i = 0; i < e.Args.Length; i++)
            {
                int argsCount = 0;

                switch (e.Args[i].ToLower())
                {
                    case PageCommand:

                        argsCount = CountNonCommandArgs(e.Args, i + 1);
                        page = string.Join(" ", e.Args, i + 1, argsCount);
                        break;

                    case ItemCommand:

                        argsCount = CountNonCommandArgs(e.Args, i + 1);
                        item = string.Join(" ", e.Args, i + 1, argsCount);
                        break;
                }

                i += argsCount;
            }

            if (Shared.ShouldITerminateBringingOtherToFront())
            {
                Application.Current.Shutdown();
                return;
            }

            MainWindow mainWindow = new();
            mainWindow.Show();

            SetCoreRunning();
        }

        private int CountNonCommandArgs(string[] args, int startIndex)
        {
            int i = FindNextCommandArgIndex(args, startIndex);

            return -1 == i ? args.Length - startIndex : i - startIndex;
        }

        private int FindNextCommandArgIndex(string[] args, int startIndex)
        {
            for (; startIndex < args.Length; startIndex++)
            {
                if (args[startIndex].StartsWith(CommandPrefix)) { return startIndex; }
            }
            return -1;
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
