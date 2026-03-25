using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppManager.Core.Actions.Launch
{
    [Description("Parameters for starting/launching an app action.")]
    [ActionCategory("Process Management")]
    public interface ILaunchAction
    {// TODO: Create a validation AppNameParameter, or use the AppName, but then require the ExecutablePath to be full filepath.
        [Description("The application to be launched.\nIf using a filename without extension name, it will try different extension names.\nNot used if filepath includes the full filename.")]
        [ActionParameter("File Name", IsRequired = false)]
        [ParameterOrder(1)]
        string? AppName { get; set; }

        [Description("The path to the file. Either the directory containing the executeables or full file path.")]
        [ActionParameter("Filepath", IsRequired = false)]
        [ParameterOrder(2)]
        string? ExecutablePath { get; set; }

        [Description("If the app has launch arguments.")]
        [ActionParameter("Launch Arguments", IsRequired = false)]
        [ParameterOrder(3)]
        string? Arguments { get; set; }

        [Description("If the app is not launched after this duration, it will be considered failed to launch.\nSet to -1 for disabled")]
        [ActionParameter("Deadline Duration", IsRequired = false)]
        [ParameterOrder(4)]
        [Range(-1, int.MaxValue)]
        public int? TimeoutMs { get; set; }
    }
}