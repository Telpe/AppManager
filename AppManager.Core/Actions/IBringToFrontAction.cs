namespace AppManager.Core.Actions
{
    public interface IBringToFrontAction
    {
        string? AppName { get; set; }
        string? WindowTitle { get; set; }
        int? ProcessLastId { get; set; }
    }
}