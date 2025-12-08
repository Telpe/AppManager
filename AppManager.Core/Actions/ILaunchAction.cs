namespace AppManager.Core.Actions
{
    public interface ILaunchAction
    {
        string? AppName { get; set; }
        string? ExecutablePath { get; set; }
        string? Arguments { get; set; }
    }
}