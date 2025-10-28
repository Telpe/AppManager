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
        public string Theme { get; set; } = "Default";
        public int ScanInterval { get; set; } = 2500;
        public bool AutoStart { get; set; } = false;
        public string[] FavoriteApps { get; set; } = Array.Empty<string>();
        public bool MinimizeToTray { get; set; } = false;
        public bool ShowNotifications { get; set; } = true;
        public string SelectedNav1Menu { get; set; } = "Apps";
        public string SelectedNav1List { get; set; } = "Apps";

        // Window settings
        public double WindowWidth { get; set; } = 600;
        public double WindowHeight { get; set; } = 600;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public bool WindowMaximized { get; set; } = false;

    }
}
