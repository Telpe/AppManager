global using AppManager.Core.Utilities;
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
using Microsoft.Win32;
using System.Runtime.CompilerServices;

namespace AppManager.Core
{
    internal class Program
    {
        private static TrayApplication? TrayAppValue;
        private static FileSystemWatcher? SettingsFileWatcherValue;
        private static FileSystemWatcher? ProfileFileWatcherValue;
        private static Dispatcher MainDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        private static bool StreamLogging = true;
        private static string? _settingsFileHash;
        private static string? _profileFileHash;
        private static object ReloadLock = new();

        [STAThread]
        static void Main(string[] args)
        {
            if (Shared.ShouldITerminate()) { return; }

            SystemEvents.SessionEnding += OnSessionEnding;
            MainDispatcher.ShutdownStarted += (s, args) =>
            {
                Log.WriteLine($"Dispatcher ShutdownStarted...({GlobalKeyboardHook.CurrentThreadId})");
                Application.Exit();
                MainDispatcher.Thread.Join(4000);
            };

            if (StreamLogging)
            {
                Log.OpenStream();
            }

            Log.WriteLine($"Main thread id: {GlobalKeyboardHook.CurrentThreadId}");
            Log.WriteLine($"Version: {FileManager.LoadVersion()}");

            InitializeFileWatchers();
            LoadTriggers();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create and run the tray application
            using (TrayAppValue = new TrayApplication())
            {
                Application.Run();
            }

            ExitCleanup();
        }

        /// <summary>
        /// Initializes the file watchers to monitor settings.json and profile files
        /// </summary>
        private static void InitializeFileWatchers()
        {
            try
            {
                lock (ReloadLock)
                {
                    InitializeSettingsFileWatcher();
                    InitializeProfileFileWatcher();
                }

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
                string settingsPath = FileManager.GetSettingsPath();
                
                // Initialize hash for change detection
                _settingsFileHash = CalculateFileHash(settingsPath);
                
                // Create and configure the file watcher
                SettingsFileWatcherValue = FileManager.BuildFileWatcher(settingsPath);
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
                string profilePath = FileManager.GetProfilePath(SettingsManager.CurrentSettings.LastUsedProfileName);
                
                // Initialize hash for change detection
                _profileFileHash = CalculateFileHash(profilePath);
                
                // Create and configure the file watcher
                ProfileFileWatcherValue = FileManager.BuildFileWatcher(profilePath);
                ProfileFileWatcherValue.NotifyFilter = NotifyFilters.LastWrite;
                ProfileFileWatcherValue.EnableRaisingEvents = true;

                // Subscribe to file change events
                ProfileFileWatcherValue.Changed += OnProfileFileChanged;
                ProfileFileWatcherValue.Error += OnProfileFileWatcherError;

                Log.WriteLine($"Profile file watcher initialized for: {SettingsManager.CurrentSettings.LastUsedProfileName}");
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
            lock (ReloadLock)
            {
                // Check if file content actually changed
                string? currentHash = CalculateFileHash(e.FullPath);
                if (currentHash == _settingsFileHash)
                {
                    Log.WriteLine("Settings file change detected but content unchanged - ignoring");
                    return;
                }

                _settingsFileHash = currentHash;
            }

            MainDispatcher.InvokeAsync(() =>
            {
                Log.WriteLine($"Settings file changed thread id: {GlobalKeyboardHook.CurrentThreadId}");
                try
                {
                    string lastProfile = SettingsManager.CurrentSettings.LastUsedProfileName;
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
            lock(ReloadLock)
            {
                // Check if file content actually changed
                string? currentHash = CalculateFileHash(e.FullPath);
                if (currentHash == _profileFileHash)
                {
                    Log.WriteLine("Profile file change detected but content unchanged - ignoring");
                    return;
                }

                _profileFileHash = currentHash;
            }

            MainDispatcher.InvokeAsync(() =>
            {
                Log.WriteLine($"Profile file changed thread id: {GlobalKeyboardHook.CurrentThreadId}");
                try
                {
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
            throw new NotImplementedException("SaveApplicationState is not implemented yet.");
            Log.WriteLine("AppManager.Core: Application state saved");
        }

        /// <summary>
        /// Calculates MD5 hash of a file for change detection
        /// </summary>
        private static string? CalculateFileHash(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                using var md5 = System.Security.Cryptography.MD5.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = md5.ComputeHash(stream);
                return Convert.ToHexString(hashBytes);
            }
            catch
            {
                return null;
            }
        }

        private static void OnSessionEnding(object? sender, SessionEndingEventArgs e)
        {
            Log.WriteLine("AppManager.Core: Session ending.");
            try 
            {
                using (Form form = new()
                {
                    Text = "AppManager cleaning up.",
                    Size = new Size(300, 200)
                })
                {
                    form.Show();

                    GlobalKeyboardHook.ShutdownBlockReasonCreate(form.Handle, "App Manager needs a bit cleaning up.");
                    Application.Exit();
                    MainDispatcher.Thread.Join(4000);
                    GlobalKeyboardHook.ShutdownBlockReasonDestroy(form.Handle);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"AppManager.Core: Error during session ending cleanup: {ex.Message}");
            }
        }

        private static void ExitCleanup()
        {
            try
            {
                Log.WriteLine($"ExitCleanup...({GlobalKeyboardHook.CurrentThreadId})");

                SystemEvents.SessionEnding -= OnSessionEnding;
                
                try
                {
                    ActionFactory.CreateAction(new()
                    {
                        ActionType = AppActionTypeEnum.Close,
                        AppName = "AppManager"
                    }).Execute();
                }
                catch { }

                try
                {
                    ActionFactory.CreateAction(new()
                    {
                        ActionType = AppActionTypeEnum.Close,
                        AppName = "AppManager.Settings"
                    }).Execute();
                }
                catch { }

                try
                {
                    // Dispose file watchers
                    SettingsFileWatcherValue?.Dispose();
                    ProfileFileWatcherValue?.Dispose();
                    Log.WriteLine("AppManager.Core: File watchers disposed");
                }
                catch { }

                try
                {
                    // Stop all triggers and dispose resources
                    TriggerManager.Dispose();
                    Log.WriteLine("AppManager.Core: Triggers disposed");
                }
                catch(Exception ex)
                {
                    Log.WriteLine($"AppManager.Core: Error during exit cleanup \"Stop all triggers\":\n{ex.Message}");
                }

                // Save any pending data or settings
                try
                {
                    SaveApplicationState();
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"AppManager.Core: Error during exit cleanup SaveApplicationState:\n{ex.Message}");
                }

                // Additional cleanup can be added here
                Log.WriteLine("AppManager.Core: Exit cleanup completed");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"AppManager.Core: Error during exit cleanup: {ex.Message}");
            }
            finally
            {
                try
                {
                    Log.WriteLine("AppManager.Core: Disposing Log...");
                    Log.Dispose();
                }
                catch { }
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
                int loadCounter = 0;

                foreach (TriggerModel model in triggers) // ProfileManager.CurrentProfile.Triggers
                {
                    try
                    {
                        TriggerManager.RegisterTrigger(model);
                        loadCounter++;
                    }
                    catch (Exception e)
                    {
                        Log.WriteLine($"Error registering trigger: {e.Message}\n{e.Source}\n{e.StackTrace}\n");
                    }
                }

                Log.WriteLine($"Loaded {loadCounter}/{triggers.Length} triggers from profile: {ProfileManager.CurrentProfile.Name}");

                ProfileManager.ClearCache();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error loading triggers: {ex.Message}");
            }
        }

    }
}
