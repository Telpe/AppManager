using System.ComponentModel;

namespace AppManager.Core.Actions.Minimize
{
    [Description("Parameters minimizing a window.")]
    [ActionCategory("Window Management")]
    public interface IMinimizeAction
    {
        [Description("The application that owns the window.")]
        [ActionParameter("Application Name", IsRequired = true)]
        [ParameterOrder(1)]
        string? AppName { get; set; }

        [Description("The app name does not have to be the full name. \nTake care not to select the wrong app.")]
        [ActionParameter("Minimize similar named", IsRequired = false)]
        [ParameterOrder(3)]
        bool? IncludeSimilarNames { get; set; }

        [Description("To select a certain window, of the app, to minimize. \nLeave blank for auto select.")]
        [ActionParameter("Window Title", IsRequired = false)]
        [ParameterOrder(2)]
        string? WindowTitle { get; set; }
    }
}