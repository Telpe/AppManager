using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public class BaseParameterControl : UserControl, INotifyPropertyChanged
    {
        protected string _labelText = String.Empty;
        protected string _headerText = String.Empty;


        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// A custom name for the value, to be used on property changed events.<br/>
        /// Has to be implemented in derived classes.
        /// </summary>
        public string ValueName { get; set; } = "Value";

        public string LabelText
        {
            get => _labelText;
            set
            {
                if (_labelText != value)
                {
                    _labelText = value;
                }
            }
        }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                if (_headerText != value)
                {
                    _headerText = value;
                }
            }
        }

        protected virtual void BroadcastPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
