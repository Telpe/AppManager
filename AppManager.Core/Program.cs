using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager.Core.Triggers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AppManager.Core
{
    internal class Program
    {
        private static TrayApplication? TrayAppValue;
        private static FileSystemWatcher? SettingsFileWatcherValue;
        private static FileSystemWatcher? ProfileFileWatcherValue;

        [STAThread]
        static void Main(string[] args)
        {
            if (ShouldITerminate()) { return; }

            InitializeFileWatchers();
            LoadTriggers();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            // Create and run the tray application
            using (TrayAppValue = new TrayApplication())
            {
                Application.Run();
            }
        }

        /// <summary>
        /// Initializes the file watchers to monitor settings.json and profile files
        /// </summary>
        private static void InitializeFileWatchers()
        {
            try
            {
                InitializeSettingsFileWatcher();
                InitializeProfileFileWatcher();

                Debug.WriteLine("File watchers initialized successfully");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error initializing file watchers: {e.Message}");
            }
        }

        /// <summary>
        /// Initializes the settings file watcher
        /// </summary>
        private static void InitializeSettingsFileWatcher()
        {
            try
            {
                // Create and configure the file watcher
                SettingsFileWatcherValue = FileManager.BuildFileWatcher(FileManager.GetSettingsPath());
                SettingsFileWatcherValue.NotifyFilter = NotifyFilters.LastWrite;
                SettingsFileWatcherValue.EnableRaisingEvents = true;

                // Subscribe to file change events
                SettingsFileWatcherValue.Changed += OnSettingsFileChanged;
                SettingsFileWatcherValue.Error += OnSettingsFileWatcherError;

                Debug.WriteLine("Settings file watcher initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing settings file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes the profile file watcher
        /// </summary>
        private static void InitializeProfileFileWatcher()
        {
            try
            {
                // Create and configure the file watcher
                SettingsManager.CurrentSettings.LastUsedProfileName ??= ProfileManager.DefaultProfileFilename;
                string currentProfileName = SettingsManager.CurrentSettings.LastUsedProfileName;
                ProfileFileWatcherValue = FileManager.BuildFileWatcher(FileManager.GetProfilePath(currentProfileName));
                ProfileFileWatcherValue.NotifyFilter = NotifyFilters.LastWrite;
                ProfileFileWatcherValue.EnableRaisingEvents = true;

                // Subscribe to file change events
                ProfileFileWatcherValue.Changed += OnProfileFileChanged;
                ProfileFileWatcherValue.Error += OnProfileFileWatcherError;

                Debug.WriteLine($"Profile file watcher initialized for: {currentProfileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing profile file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles settings file changes and reloads profile if LastUsedProfileName changed
        /// </summary>
        private static void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Add a small delay to ensure file writing is complete
                System.Threading.Thread.Sleep(100);

                string lastProfile = SettingsManager.CurrentSettings.LastUsedProfileName ?? ProfileManager.DefaultProfileFilename;
                SettingsManager.ClearCache();

                if (SettingsManager.CurrentSettings.LastUsedProfileName != lastProfile)
                {
                    // Reinitialize the profile file watcher for the new profile
                    ProfileFileWatcherValue?.Dispose();
                    InitializeProfileFileWatcher();
                    
                    // Reload profile and triggers
                    ReloadProfileAndTriggers();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling settings file change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles profile file changes and reloads triggers
        /// </summary>
        private static void OnProfileFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Add a small delay to ensure file writing is complete
                System.Threading.Thread.Sleep(100);

                Debug.WriteLine($"Profile file change detected: {e.FullPath}");
                
                // Reload profile and triggers
                ReloadProfileAndTriggers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling profile file change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles settings file watcher errors
        /// </summary>
        private static void OnSettingsFileWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"Settings file watcher error: {e.GetException().Message}");
            
            // Try to restart the file watcher
            try
            {
                SettingsFileWatcherValue?.Dispose();
                InitializeSettingsFileWatcher();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restarting settings file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles profile file watcher errors
        /// </summary>
        private static void OnProfileFileWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"Profile file watcher error: {e.GetException().Message}");
            
            // Try to restart the profile file watcher
            try
            {
                ProfileFileWatcherValue?.Dispose();
                InitializeProfileFileWatcher();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restarting profile file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Reloads the profile and triggers when profile changes are detected
        /// </summary>
        private static void ReloadProfileAndTriggers()
        {
            try
            {
                Debug.WriteLine("Reloading profile and triggers...");

                // Dispose existing triggers
                TriggerManager.Dispose();
                
                // Clear the profile cache to force reload
                ProfileManager.ClearCache();
                
                // Load triggers from the reloaded profile
                LoadTriggers();

                Debug.WriteLine("Profile and triggers reloaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reloading profile and triggers: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves application state before exit
        /// </summary>
        private static void SaveApplicationState()
        {
            try
            {
                // Save current profile or settings if needed
                // This method can be expanded based on your requirements
                Debug.WriteLine("AppManager.Core: Application state saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppManager.Core: Error saving application state: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for AppDomain.ProcessExit
        /// </summary>
        private static void OnProcessExit(object? sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("AppManager.Core: Starting exit cleanup...");

                ActionManager.ExecuteActionAsync(AppActionTypeEnum.Close, "AppManager").Wait();
                ActionManager.ExecuteActionAsync(AppActionTypeEnum.Close, "AppManager.Settings").Wait();

                // Dispose file watchers
                SettingsFileWatcherValue?.Dispose();
                ProfileFileWatcherValue?.Dispose();
                Debug.WriteLine("AppManager.Core: File watchers disposed");

                // Stop all triggers and dispose resources
                TriggerManager.Dispose();
                Debug.WriteLine("AppManager.Core: Triggers disposed");

                // Clean up any running actions
                //ActionManager.Dispose();
                //Debug.WriteLine("AppManager.Core: Actions disposed");

                // Save any pending data or settings
                SaveApplicationState();

                // Additional cleanup can be added here
                Debug.WriteLine("AppManager.Core: Exit cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppManager.Core: Error during exit cleanup: {ex.Message}");
            }
        }


        /// <summary>
        /// Loads triggers from the current profile
        /// </summary>
        protected static void LoadTriggers()
        {
            try
            {
                foreach (TriggerModel model in ProfileManager.CurrentProfile.Triggers)
                {
                    TriggerManager.RegisterTrigger(TriggerManager.CreateTrigger(model));
                }

                ProfileManager.ClearCache();

                Debug.WriteLine($"Loaded {ProfileManager.CurrentProfile.Triggers.Length} triggers from profile: {ProfileManager.CurrentProfile.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading triggers: {ex.Message}");
            }
        }

        // reload profile on last profile changed. also settings.

        protected static bool ShouldITerminate()
        {
            return CheckSelfRunning(out Process? notSelf);
        }

        protected static bool CheckSelfRunning(out Process? notSelf)
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = ProcessManager.FindProcesses(currentProcess.ProcessName, false, null, false, false, currentProcess.Id);

            if (processes.Length > 0)
            {
                notSelf = processes[0];
                return true;
            }

            notSelf = null;
            return false;
        }
    }
}
