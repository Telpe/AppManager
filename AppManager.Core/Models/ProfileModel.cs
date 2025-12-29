using AppManager.Core.Utils;
using System;
using System.Text.Json.Serialization;

namespace AppManager.Core.Models
{
    public class ProfileModel
    {
        public string Name { get; set; } = ProfileManager.DefaultProfileFilename;

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

    }
}
