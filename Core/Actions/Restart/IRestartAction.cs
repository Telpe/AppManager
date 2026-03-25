using System.ComponentModel;

namespace AppManager.Core.Actions.Restart
{
    [Description("Parameters for restarting/relaunching an app action. \nMuch similar to a Close followed by a Launch.\nMay be removed for same reason.")]
    [ActionCategory("Process Management")]
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