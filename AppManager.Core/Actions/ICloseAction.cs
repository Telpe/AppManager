namespace AppManager.Core.Actions
{
    public interface ICloseAction
    {
        string? AppName { get; set; }
        int? TimeoutMs { get; set; }
        bool? ForceOperation { get; set; }
        bool? IncludeSimilarNames { get; set; }
        bool? IncludeChildProcesses { get; set; }
    }
}