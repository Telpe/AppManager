using System.ComponentModel;

namespace AppManager.Core.Actions.BringToFront
{
    [Description("Special parameters for the action, to bring a loaded window into vision.")]
    [ActionCategory("Window Management")]
    public interface IBringToFrontAction
    {
        [Description("The application that owns the window.")]
        [ActionParameter("Application Name", IsRequired = true)]
        [ParameterOrder(1)]
        string? AppName { get; set; }
        
        [Description("The title of the window.")]
        [ActionParameter("Window Title", IsRequired = false)]
        [ParameterOrder(2)]
        string? WindowTitle { get; set; }
        
        [Description("Only used temporary during runtime.")]
        [ActionParameter("Process ID", IsRequired = false)]
        [ParameterOrder(99)] // Low priority for UI ordering
        int? ProcessLastId { get; set; }
    }
}