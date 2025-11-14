using System.Windows.Media;

namespace AppManager.Browser
{
    /// <summary>
    /// Pure data model for browser shortcut information
    /// </summary>
    public class BrowserShortcutModel
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string FileExtension { get; set; }
        public ImageSource IconSource { get; set; }
        public bool HasIcon => IconSource != null;
    }
}