using System;
using System.Reflection;

namespace AppManager.Profile
{
    public class GroupManagedModel
    {
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Selected { get; set; }
        public bool AutoCloseAll { get; set; }
        public bool IsExpanded { get; set; } = true;
        public string[] MemberApps { get; set; } = Array.Empty<string>();

        public GroupManagedModel() 
        { 
        }

    }
}