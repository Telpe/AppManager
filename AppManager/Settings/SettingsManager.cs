using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AppManager.Settings
{
    public static class SettingsManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppManager");
        public static readonly string SettingsFileName = "settings.json";
        public static readonly string SettingsFile = Path.Combine(SettingsPath, SettingsFileName);

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
                if (File.Exists(SettingsFile))
                {
                    settings = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsFile), JsonOptions);
                    Debug.WriteLine("Settings loaded successfully");
                }
                else
                {
                    settings = new SettingsData();
                    _CurrentSettings = settings;
                    Debug.WriteLine("Default settings created");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                settings = new SettingsData();
            }

            return settings;
        }

        public static void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(SettingsPath);
                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(CurrentSettings, JsonOptions));
                Debug.WriteLine("Settings saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}