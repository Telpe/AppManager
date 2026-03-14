using System.ComponentModel;

namespace AppManager.Core.Actions.Focus
{
    [Description("Special parameters for the action, to bring a loaded window into vision.")]
    [ActionCategory("Window Management")]
    public interface IFocusAction
    {
        [Description("The application that owns the window.")]
        [ActionParameter("Application Name", IsRequired = true)]
        [ParameterOrder(1)]
        string? AppName { get; set; }
        bool? IncludeSimilarNames { get; set; }
        string? WindowTitle { get; set; }
    }
}