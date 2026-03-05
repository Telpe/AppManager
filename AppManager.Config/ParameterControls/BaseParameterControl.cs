using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Config.ParameterControls
{
    public class BaseParameterControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Description { get; set; } = "No description";

        /// <summary>
        /// A custom name for the value, to be used on property changed events.<br/>
        /// Has to be implemented in derived classes.
        /// </summary>
        public string ValueName { get; set; } = "Value";

        public string LabelText { get; set; } = "Value";

        public string HeaderText { get; set; } = "Parameter";

        protected virtual void AnnouncePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
