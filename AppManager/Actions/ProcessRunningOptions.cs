namespace AppManager.Actions
{
    public class ProcessRunningOptions
    {
        public bool? ForceKill { get; set; } = false;
        public bool? IncludeChildren { get; set; } = true;
        public bool? IncludeTasksLikeGiven { get; set; } = false;
        public bool? ExitWhenDone { get; set; } = false;
    }
}
