using System;

namespace AppManager.Profile
{
    public class ProfileData
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public AppManagedModel[] Apps { get; set; } = Array.Empty<AppManagedModel>();
        public GroupManagedModel[] AppGroups { get; set; } = Array.Empty<GroupManagedModel>();

        public string Username { get; set; } = "anonymous";
        public int ScanInterval { get; set; } = 2500;
        public bool AutoStart { get; set; } = false;
        public string[] FavoriteApps { get; set; } = Array.Empty<string>();
        public string SelectedNav1Menu { get; set; } = "Apps";
        public string SelectedNav1List { get; set; } = "Apps";

        // Window settings moved to SettingsData
        // Theme, MinimizeToTray, ShowNotifications moved to SettingsData
    }
}
