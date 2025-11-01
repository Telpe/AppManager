using AppManager.Profile;
using AppManager.Shortcuts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace AppManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static GlobalKeyboardHook _GlobalKeyboardHook;

        // Profile-related properties
        private static ProfileData _CurrentProfile;
        public static ProfileData CurrentProfile 
        { 
            get => _CurrentProfile; 
            private set => _CurrentProfile = value; 
        }

        // Profile storage paths
        public static readonly string StorePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppManager");
        public static readonly string StoreName = "AppsManaged.json";
        public static readonly string StoreFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppManager", StoreName);

        private static Dictionary<string,string> UnsavedPages = new();

        private Timer CheckIfAppsRunningValue = new();
        public Timer CheckIfAppsRunning { get { return CheckIfAppsRunningValue; } }

        public App()
        {
            // Load profile first
            CurrentProfile = LoadProfile();

            InitializeComponent();

            CheckIfAppsRunningValue.Interval = 2500;
            CheckIfAppsRunningValue.AutoReset = true;
            CheckIfAppsRunningValue.Elapsed += new ElapsedEventHandler(CheckRunningHandler);
            //CheckIfAppsRunningValue.Start();
        }

        private ProfileData LoadProfile()
        {
            ProfileData profile;
            try
            {
                if (File.Exists(StoreFile))
                {
                    string profileJson = File.ReadAllText(StoreFile);
                    profile = JsonSerializer.Deserialize<ProfileData>(profileJson);
                    Debug.WriteLine("Profile loaded successfully");
                }
                else
                {
                    // Create default profile if none exists
                    profile = new ProfileData
                    {
                        Username = Environment.UserName
                    };
                    SaveProfile();
                    Debug.WriteLine("Default profile created");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
                // Create default profile on error
                profile = new ProfileData();
            }

            return profile;
        }



        public static void SaveProfile()
        {
            //Major Minor  Build Revision
            CurrentProfile.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                Directory.CreateDirectory(StorePath);

                JsonSerializerOptions.Default.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                File.WriteAllText(StoreFile, JsonSerializer.Serialize(CurrentProfile, new JsonSerializerOptions { WriteIndented = true }));
                Debug.WriteLine("Profile saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving profile: {ex.Message}");
            }
        }


        public static bool ManagedAppsFileExists()
        {
            return File.Exists(StoreFile);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Save profile when application exits
            //SaveProfile();
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
