namespace AppManager.Core.Actions.BringToFront
{
    public interface IBringToFrontAction
    {
        string? AppName { get; set; }
        string? WindowTitle { get; set; }
        int? ProcessLastId { get; set; }
    }
}