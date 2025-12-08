namespace AppManager.Core.Actions
{
    public interface IRestartAction
    {
        string? AppName { get; set; }
        bool? IncludeChildProcesses { get; set; }
        bool? IncludeSimilarNames { get; set; }
        int? TimeoutMs { get; set; }
        bool? ForceOperation { get; set; }
        string? ExecutablePath { get; set; }
        string? Arguments { get; set; }
    }
}