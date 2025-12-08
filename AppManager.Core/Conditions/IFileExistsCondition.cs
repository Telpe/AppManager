namespace AppManager.Core.Conditions
{
    public interface IFileExistsCondition
    {
        string? ExecutablePath { get; set; }
    }
}