using System.Text.Json.Serialization;
using AppManager.Core.Utils;

namespace AppManager.Core.Models
{
    public class SettingsModel
    {
        [JsonPropertyName("version")]
        [JsonConverter(typeof(VersionJsonConverter))]
        public Version Version { get; set; }

        [JsonPropertyName("windowWidth")]
        public double WindowWidth { get; set; } = 600;

        [JsonPropertyName("windowHeight")]
        public double WindowHeight { get; set; } = 600;

        [JsonPropertyName("windowLeft")]
        public double WindowLeft { get; set; } = 100;

        [JsonPropertyName("windowTop")]
        public double WindowTop { get; set; } = 100;

        [JsonPropertyName("windowMaximized")]
        public bool WindowMaximized { get; set; } = false;

        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "Default";

        [JsonPropertyName("minimizeToTray")]
        public bool MinimizeToTray { get; set; } = false;

        [JsonPropertyName("showNotifications")]
        public bool ShowNotifications { get; set; } = true;

        [JsonPropertyName("lastUsedProfileName")]
        public string LastUsedProfileName { get; set; } = "default";
    }
}