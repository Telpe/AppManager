namespace AppManager.Core.Conditions
{
    public interface IProcessRunningCondition
    {
        string? ProcessName { get; set; }
    }
}