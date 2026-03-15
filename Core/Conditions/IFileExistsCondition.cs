namespace AppManager.Core.Conditions
{
    public interface IFileExistsCondition
    {
        string? FilePath { get; set; }
    }
}