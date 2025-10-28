namespace AppManager.Actions
{
    public class ActionParameters
    {
        public bool ForceOperation { get; set; } = false;
        public bool IncludeChildProcesses { get; set; } = false;
        public bool IncludeSimilarNames { get; set; } = false;
        public int TimeoutMs { get; set; } = 5000;
        public string WindowTitle { get; set; }
        public string ExecutablePath { get; set; }
        public string Arguments { get; set; }
    }
}