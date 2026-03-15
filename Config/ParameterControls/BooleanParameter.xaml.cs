using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Config.ParameterControls
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
                    AnnouncePropertyChanged(ValueName);
                }
            }
        }

        public BooleanParameter()
        {
            HeaderText = "Boolean Setting";
            LabelText = "Option:";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;
        }

        public BooleanParameter(bool? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (headerText != null)
            {
                HeaderText = headerText;
            }
            
            if (labelText != null)
            {
                LabelText = labelText;
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