using AppManager.Core.Models;
using System.Diagnostics;

namespace AppManager.Core.Utils
{
    public static class SettingsManager
    {
        private static SettingsModel? _CurrentSettings;
        public static SettingsModel CurrentSettings 
        { 
            get => _CurrentSettings ??= LoadSettings();
        }

        public static SettingsModel LoadSettings()
        {
            SettingsModel settings;
            try
            {
                string settingsFile = FileManager.GetSettingsPath();
                settings = FileManager.LoadJsonFile<SettingsModel>(settingsFile);
                
                // Check if file was actually loaded (has non-default values)
                if (!string.IsNullOrEmpty(settings.Theme))
                {
                    Debug.WriteLine("Settings loaded successfully");
                }
                else
                {
                    settings = new SettingsModel();
                    _CurrentSettings = settings;
                    Debug.WriteLine("Default settings created");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                settings = new SettingsModel();
            }

            return settings;
        }

        public static void SaveSettings()
        {
            // Update version info before saving
            CurrentSettings.Version = FileManager.LoadVersion();

            string settingsFile = FileManager.GetSettingsPath();
            bool success = FileManager.SaveJsonFile(CurrentSettings, settingsFile);
            
            if (success)
            {
                Debug.WriteLine("Settings saved successfully");
            }
            else
            {
                Debug.WriteLine("Failed to save settings");
            }
        }

        public static void ClearCache() { _CurrentSettings = null; }
    }
}