using AppManager.Core.Utils;
using AppManager.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using AppManager.Core.Shortcuts;
{
    
}

namespace AppManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static GlobalKeyboardHook _GlobalKeyboardHook;

        private static Dictionary<string,string> UnsavedPages = new();

        private Timer CheckIfAppsRunningValue = new();
        public Timer CheckIfAppsRunning { get { return CheckIfAppsRunningValue; } }

        public static readonly AppManager.Core.Version Version;

        static App()
        {
            // Load version from version.json file
            Version = FileManager.LoadVersion();
            Debug.WriteLine($"AppManager Version: {Version}");
        }

        public App()
        {
            InitializeComponent();



            // Load the last used profile (or default if none specified)
            string profileToLoad = !string.IsNullOrEmpty(SettingsManager.CurrentSettings.LastUsedProfileName) 
                ? SettingsManager.CurrentSettings.LastUsedProfileName 
                : ProfileManager.DefaultProfileFilename;
            
            // Load the specific profile
            if (ProfileManager.ProfileExists(profileToLoad))
            {
                ProfileManager.LoadAndSetProfile(profileToLoad);
                Debug.WriteLine($"Loaded last used profile: {profileToLoad}");
            }
            else
            {
                // If the last used profile doesn't exist, fall back to default
                _ = ProfileManager.CurrentProfile; // This triggers loading of default profile
                Debug.WriteLine($"Profile '{profileToLoad}' not found, loaded default profile instead");
            }

            CheckIfAppsRunningValue.Interval = ProfileManager.CurrentProfile.ScanInterval;
            CheckIfAppsRunningValue.AutoReset = true;
            CheckIfAppsRunningValue.Elapsed += new ElapsedEventHandler(CheckRunningHandler);
            //CheckIfAppsRunningValue.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //ProfileManager.SaveProfile();
            base.OnExit(e);
        }

        private void CheckRunningHandler(object sender, ElapsedEventArgs eve)
        {
            Application.Current.Dispatcher.Invoke(CheckRunning);
        }

        public void CheckRunning()
        {
            /*for (int i = 0; i < AppsList.Length; i++)
            {
                AppManaged app;
                try
                {
                    app = (AppManaged)AppsList[i];
                }
                catch (Exception e)
                {
                    app = new AppManaged();
                    Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                }

                if (0 < app.AppName.Length)
                {
                    var processs = Process.GetProcessesByName(app.AppName);
                    app.IsRunning = processs.Length;

                }

            }*/

        }
/*
        private void AppScanInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(AppScanInterval.Text, out int interval))
            {
                if (interval < 100) { interval = 100; }
                CheckIfAppsRunningValue.Interval = interval;
            }
        }
        private void AppScanStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfAppsRunningValue.Enabled)
            {
                // Stop the timer
                CheckIfAppsRunningValue.Stop();

                // Update button appearance for "Start" state
                AppScanStartStop.Background = System.Windows.Media.Brushes.Yellow;
                AppScanStartStop.Content = "Start Scan";
            }
            else
            {
                // Start the timer with the current interval
                CheckIfAppsRunningValue.Start();

                // Update button appearance for "Stop" state
                AppScanStartStop.Background = System.Windows.Media.Brushes.Red;
                AppScanStartStop.Content = "Stop Scan";
            }
        }
*/
/*
        public static void buttonHook_Click(object sender, EventArgs e)
        {
            // Hooks only into specified Keys (here "A" and "B").
            _GlobalKeyboardHook = new GlobalKeyboardHook(new Key[] { Key.A, Key.B });

            // Hooks into all keys.
            _GlobalKeyboardHook = new GlobalKeyboardHook();
            _GlobalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        internal static void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            // EDT: No need to filter for VkSnapshot anymore. This now gets handled
            // through the constructor of GlobalKeyboardHook(...).
            if (e.KeyboardState == KeyboardState.KeyDown)
            {
                // Now you can access both, the key and virtual code
                Key loggedKey = e.KeyboardData.Key;
                int loggedVkCode = e.KeyboardData.VirtualCode;
            }
        }
*/
    }

}
