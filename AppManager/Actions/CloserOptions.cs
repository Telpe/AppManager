namespace AppManager.Actions
{
    public class CloserOptions
    {
        public bool? ForceKill { get; set; } = false;
        public bool? IncludeChildren { get; set; } = true;
        public bool? IncludeTasksLikeGiven { get; set; } = false;
        public bool? ExitWhenDone { get; set; } = false;
    }
}
