using System.ComponentModel;

namespace AppManager.Core.Actions.Close
{
    [Description("Parameters for exiting/closing an app action.")]
    [ActionCategory("Process Management")]
    public interface ICloseAction
    {
        [Description("The application to be closed.\nBy default we ask the app to close. To give a gentle close, so that the app have time to save.")]
        [ActionParameter("Application Name", IsRequired = true)]
        [ParameterOrder(1)]
        string? AppName { get; set; }

        [Description("Consider failure if not succeded after this duration.\nSet -1 for disabled.")]
        [ActionParameter("Deadline Duration", IsRequired = false)]
        [ParameterOrder(2)]
        int? TimeoutMs { get; set; }

        [Description("Force close the app, if not closed before before Deadline.\nIf Deadline is disabled, a gentle close is still attempted first.")]
        [ActionParameter("Force Close", IsRequired = false)]
        [ParameterOrder(3)]
        bool? ForceOperation { get; set; }

        [Description("Closes all apps that include the name.\nEg.: App Name: Browser, closes ChromeBrowser, EdgeBrowser and BrowserHelper.")]
        [ActionParameter("Also Close Similar", IsRequired = false)]
        [ParameterOrder(4)]
        bool? IncludeSimilarNames { get; set; }

        [Description("Also close all apps that are children of the found app(s).\nUsualy the closing app will do this itself.")]
        [ActionParameter("Also Close Child Apps.", IsRequired = false)]
        [ParameterOrder(5)]
        bool? IncludeChildProcesses { get; set; }
    }
}