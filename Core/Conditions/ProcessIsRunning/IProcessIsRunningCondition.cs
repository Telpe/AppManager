using System.ComponentModel;

namespace AppManager.Core.Conditions.ProcessIsRunning
{
    [Description("Parameters for the condition that a certain process/app is running.")]
    [ConditionCategory("Process Management")]
    public interface IProcessIsRunningCondition
    {
        [Description("The application to be launched.\nIf using a filename without extension name, it will try different extension names.\nNot used if filepath includes the full filename.")]
        [ConditionParameter("Name of the process/app to search for.", IsRequired = true)]
        [ParameterOrder(1)]
        string? ProcessName { get; set; }
    }
}