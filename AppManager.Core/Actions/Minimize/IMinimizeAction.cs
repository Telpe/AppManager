namespace AppManager.Core.Actions.Minimize
{
    public interface IMinimizeAction
    {
        string? AppName { get; set; }
        bool? IncludeSimilarNames { get; set; }
        string? WindowTitle { get; set; }
    }
}