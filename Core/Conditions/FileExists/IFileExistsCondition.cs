using System.ComponentModel;

namespace AppManager.Core.Conditions.FileExists
{
    [Description("Parameters for the condition that a certain file exists.")]
    [ConditionCategory("File Management")]
    public interface IFileExistsCondition
    {
        [Description("The path to the file. Use the full file path.\nUse the browse button for assistance.")]
        [ConditionParameter("Filepath", IsRequired = true)]
        [ParameterOrder(1)]
        string? FilePath { get; set; }
    }
}