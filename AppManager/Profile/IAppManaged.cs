namespace AppManager.Profile
{
    internal interface IAppManaged
    {
        public string AppName { get; set; }
        public bool Selected { get; set; }
        public bool IncludeSimilar { get; set; }
        public bool ForceExit { get; set; }
        public bool IncludeChildren { get; set; }

    }
}
