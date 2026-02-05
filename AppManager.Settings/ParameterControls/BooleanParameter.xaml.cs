using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class BooleanParameter : BaseParameterControl
    {
        private bool? _value;

        public bool? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    ValueCheckBox.IsChecked = _value;
                    BroadcastPropertyChanged(ValueName);
                }
            }
        }

        public BooleanParameter()
        {
            _headerText = "Boolean Setting";
            _labelText = "Option:";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;
        }

        public BooleanParameter(bool? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (headerText != null)
            {
                _headerText = headerText;
            }
            
            if (labelText != null)
            {
                _labelText = labelText;
            }

            if (customValueName != null)
            {
                ValueName = customValueName;
            }

            if (initialValue != null)
            {
                Value = initialValue;
            }

            if (eventHandler != null)
            {
                PropertyChanged += eventHandler;
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                Value = checkBox.IsChecked;
            }
        }
    }
}