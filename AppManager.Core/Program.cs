global using AppManager.Core.Utils;
using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AppManager.Core.Keybinds;
using System.Windows.Threading;

namespace AppManager.Core
{
    internal class Program
    {
        private static TrayApplication? TrayAppValue;
        private static FileSystemWatcher? SettingsFileWatcherValue;
        private static FileSystemWatcher? ProfileFileWatcherValue;
        private static Dispatcher MainDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        private static bool StreamLogging = true;

        [STAThread]
        static void Main(string[] args)
        {
            if (ShouldITerminate()) { return; }
            if (StreamLogging)
            {
                Log.OpenStream();
            }
            
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitializeFileWatchers();
            LoadTriggers();

            Log.WriteLine($"Main thread id: {GlobalKeyboardHook.CurrentThreadId}");

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

                Log.WriteLine("File watchers initialized successfully");
            }
            catch (Exception e)
            {
                Log.WriteLine($"Error initializing file watchers: {e.Message}");
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

                Log.WriteLine("Settings file watcher initialized");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error initializing settings file watcher: {ex.Message}");
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

                Log.WriteLine($"Profile file watcher initialized for: {currentProfileName}");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error initializing profile file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles settings file changes and reloads profile if LastUsedProfileName changed
        /// </summary>
        private static void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            MainDispatcher.InvokeAsync(() =>
            {
                Log.WriteLine($"Settings file changed thread id: {GlobalKeyboardHook.CurrentThreadId}");
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
                    Log.WriteLine($"Error handling settings file change: {ex.Message}");
                }
            });
            
        }

        /// <summary>
        /// Handles profile file changes and reloads triggers
        /// </summary>
        private static void OnProfileFileChanged(object sender, FileSystemEventArgs e)
        {
            MainDispatcher.InvokeAsync(() =>
            {
                Log.WriteLine($"Profile file changed thread id: {GlobalKeyboardHook.CurrentThreadId}");
                try
                {
                    // Add a small delay to ensure file writing is complete
                    System.Threading.Thread.Sleep(100);

                    Log.WriteLine($"Profile file change detected: {e.FullPath}");
                
                    // Reload profile and triggers
                    ReloadProfileAndTriggers();
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Error handling profile file change: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Handles settings file watcher errors
        /// </summary>
        private static void OnSettingsFileWatcherError(object sender, ErrorEventArgs e)
        {
            Log.WriteLine($"Settings file watcher error: {e.GetException().Message}");
            
            // Try to restart the file watcher
            try
            {
                SettingsFileWatcherValue?.Dispose();
                InitializeSettingsFileWatcher();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error restarting settings file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles profile file watcher errors
        /// </summary>
        private static void OnProfileFileWatcherError(object sender, ErrorEventArgs e)
        {
            Log.WriteLine($"Profile file watcher error: {e.GetException().Message}");
            
            // Try to restart the profile file watcher
            try
            {
                ProfileFileWatcherValue?.Dispose();
                InitializeProfileFileWatcher();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error restarting profile file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Reloads the profile and triggers when profile changes are detected
        /// </summary>
        private static void ReloadProfileAndTriggers()
        {
            try
            {
                Log.WriteLine("Reloading profile and triggers...");

                // Dispose existing triggers
                TriggerManager.Dispose();
                
                // Clear the profile cache to force reload
                ProfileManager.ClearCache();
                
                // Load triggers from the reloaded profile
                LoadTriggers();

                Log.WriteLine("Profile and triggers reloaded successfully");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error reloading profile and triggers: {ex.Message}");
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
                Log.WriteLine("AppManager.Core: Application state saved");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"AppManager.Core: Error saving application state: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for AppDomain.ProcessExit
        /// </summary>
        private static void OnProcessExit(object? sender, EventArgs e)
        {
            try
            {
                Log.WriteLine("AppManager.Core: Starting exit cleanup...");

                ActionManager.ExecuteAction(AppActionTypeEnum.Close, "AppManager");
                ActionManager.ExecuteAction(AppActionTypeEnum.Close, "AppManager.Settings");

                // Dispose file watchers
                SettingsFileWatcherValue?.Dispose();
                ProfileFileWatcherValue?.Dispose();
                Log.WriteLine("AppManager.Core: File watchers disposed");

                // Stop all triggers and dispose resources
                TriggerManager.Dispose();
                Log.WriteLine("AppManager.Core: Triggers disposed");

                // Clean up any running actions
                //ActionManager.Dispose();
                //Log.WriteLine("AppManager.Core: Actions disposed");

                // Save any pending data or settings
                SaveApplicationState();

                // Additional cleanup can be added here
                Log.WriteLine("AppManager.Core: Exit cleanup completed");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"AppManager.Core: Error during exit cleanup: {ex.Message}");
            }
            finally
            {
                Log.Dispose();
            }
        }


        /// <summary>
        /// Loads triggers from the current profile
        /// </summary>
        protected static void LoadTriggers()
        {
            try
            {
                TriggerModel[] triggers = ProfileManager.CurrentProfile.Apps.Where(a => null != a.Triggers).SelectMany(a => a.Triggers!.Select(a => a.Value)).ToArray();
                int count = 0;

                foreach (TriggerModel model in triggers) // ProfileManager.CurrentProfile.Triggers
                {
                    TriggerManager.RegisterTrigger(TriggerManager.CreateTrigger(model));
                    count++;
                }

                Log.WriteLine($"Loaded {count} triggers from profile: {ProfileManager.CurrentProfile.Name}");

                ProfileManager.ClearCache();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error loading triggers: {ex.Message}");
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
