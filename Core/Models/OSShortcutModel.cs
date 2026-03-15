using System.Windows.Media;

namespace AppManager.Core.Models
{
    /// <summary>
    /// Pure data model for browser shortcut information
    /// </summary>
    public class OSShortcutModel
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string FileExtension { get; set; }
        public ImageSource IconSource { get; set; }
        public bool HasIcon => IconSource != null;
    }
}