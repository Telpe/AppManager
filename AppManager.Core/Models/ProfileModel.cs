using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppManager.Core.Models
{
    public class ProfileModel
    {
        private string NameValue = ProfileManager.DefaultProfileFilename;
        public string Name 
        { 
            get=>NameValue;
            set
            {
                if(!IsValidProfileName(value))
                {
                    throw new ArgumentException($"Invalid profile name '{value}'");
                }

                NameValue = value;
            }
        }

        [JsonConverter(typeof(VersionJsonConverter))]
        public Version Version { get; set; } = FileManager.LoadVersion();

        public TriggerModel[] Triggers { get; set; } = [];

        [Obsolete("This property is deprecated and will be removed in a future version.")]
        public AppManagedModel[] Apps { get; set; } = Array.Empty<AppManagedModel>();
        [Obsolete("This property is deprecated and will be removed in a future version.")]
        public GroupManagedModel[] AppGroups { get; set; } = Array.Empty<GroupManagedModel>();

        public string Username { get; set; } = "anonymous";
        public int ScanInterval { get; set; } = 2500;
        public bool AutoStart { get; set; } = false;
        public string[] FavoriteApps { get; set; } = Array.Empty<string>();
        public string SelectedNav1Menu { get; set; } = "Apps";
        public string SelectedNav1List { get; set; } = "";

        public static bool IsValidProfileName(string profileName)
        {
            if (string.IsNullOrEmpty(profileName)
             || -1 < profileName.IndexOfAny(Path.GetInvalidFileNameChars()))
            {
                return false;
            }
            return true;
        }
    }
}
