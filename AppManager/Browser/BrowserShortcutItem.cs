using AppManager.Core.Models;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppManager.Browser
{
    /// <summary>
    /// View model that wraps the data model and provides UI-ready properties
    /// </summary>
    public class BrowserShortcutItem
    {
        private readonly BrowserShortcutModel _model;

        public BrowserShortcutItem(BrowserShortcutModel model)
        {
            _model = model;
        }

        public string Name => _model.Name;
        public string ExecutablePath => _model.ExecutablePath;
        public string FileExtension => _model.FileExtension;
        public bool HasIcon => _model.HasIcon;

        public ImageSource IconSource => _model.IconSource;

    }
}