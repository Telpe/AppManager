namespace AppManager.Profile
{
    internal interface IGroupManaged
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool Selected { get; set; }
        public bool AutoCloseAll { get; set; }
        public bool IsExpanded { get; set; }
        public string[] MemberApps { get; set; }
    }
}