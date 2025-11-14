using AppManager.Utils;
using System.Diagnostics;

namespace AppManager.Settings
{
    public static class SettingsManager
    {
        private static SettingsData _CurrentSettings;
        public static SettingsData CurrentSettings 
        { 
            get => _CurrentSettings ??= LoadSettings(); 
            private set => _CurrentSettings = value; 
        }

        public static SettingsData LoadSettings()
        {
            SettingsData settings;
            try
            {
                string settingsFile = FileManager.GetSettingsPath();
                settings = FileManager.LoadJsonFile<SettingsData>(settingsFile);
                
                // Check if file was actually loaded (has non-default values)
                if (!string.IsNullOrEmpty(settings.Theme))
                {
                    Debug.WriteLine("Settings loaded successfully");
                }
                else
                {
                    settings = new SettingsData();
                    _CurrentSettings = settings;
                    Debug.WriteLine("Default settings created");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                settings = new SettingsData();
            }

            return settings;
        }

        public static void SaveSettings()
        {
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
    }
}