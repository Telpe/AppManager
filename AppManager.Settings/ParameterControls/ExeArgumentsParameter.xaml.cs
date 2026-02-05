using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class ExeArgumentsParameter : BaseParameterControl
    {
        private string _value = string.Empty;
        private const int MaxLength = 32698;

        public string Value
        {
            get => _value;
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > MaxLength)
                {
                    newValue = newValue.Substring(0, MaxLength);
                }

                if (_value != newValue)
                {
                    _value = newValue;
                    ArgumentsTextBox.Text = _value;
                    BroadcastPropertyChanged(ValueName);
                }
            }
        }

        public ExeArgumentsParameter()
        {
            _labelText = "Arguments:";
            _headerText = "Executable Arguments";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;
        }

        public ExeArgumentsParameter(string? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (headerText != null)
            {
                _headerText = headerText;
            }
            
            if (labelText != null)
            {
                _labelText = labelText;
            }

            if (initialValue != null)
            {
                _value = initialValue;
            }

            ArgumentsTextBox.Text = _value;
        }

        private void ArgumentsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Value = textBox.Text;
            }
        }
    }
}