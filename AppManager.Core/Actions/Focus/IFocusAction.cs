namespace AppManager.Core.Actions.Focus
{
    public interface IFocusAction
    {
        string? AppName { get; set; }
        bool? IncludeSimilarNames { get; set; }
        string? WindowTitle { get; set; }
    }
}