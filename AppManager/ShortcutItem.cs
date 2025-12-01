using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AppManager.Core.Models;

namespace AppManager
{
    /// <summary>
    /// View model that wraps the data model and provides UI-ready properties
    /// </summary>
    public class OSShortcutItem
    {
        private readonly OSShortcutModel _model;

        public OSShortcutItem(OSShortcutModel model)
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
