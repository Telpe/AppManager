using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Config.ParameterControls
{
    public partial class MultiIntParameter : BaseParameterControl
    {
        private string _value = string.Empty;
        private char _separator = ',';

        public string Value
        {
            get => _value;
            set
            {
                string newValue = ValidateAndFormatValue(value);

                // If not from textbox, update textbox
                if (ValueTextBox.Text != newValue)
                {
                    ValueTextBox.Text = newValue;
                }

                // If value changed, update and broadcast
                if (_value != newValue)
                {
                    _value = newValue;
                    AnnouncePropertyChanged(ValueName);
                }
            }
        }

        public char Separator
        {
            get => _separator;
            set
            {
                if (_separator != value)
                {
                    _value = _value.Replace(_separator, value);

                    _separator = value;
                    
                    Value = _value;
                }
            }
        }

        public MultiIntParameter()
        {
            LabelText = "Values:";
            HeaderText = "Multi-Integer Parameter";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;
            
            SeparatorTextBox.Text = _separator.ToString();
        }

        public MultiIntParameter(string? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null, char? separator = null) : this()
        {
            if (headerText is string headerTextTemp)
            {
                HeaderText = headerTextTemp;
            }
            
            if (labelText is string labelTextTemp)
            {
                LabelText = labelTextTemp;
            }

            if (customValueName is string customValueNameTemp)
            {
                ValueName = customValueNameTemp;
            }

            if (separator is char separatorTemp)
            {
                _separator = separatorTemp;
                SeparatorTextBox.Text = _separator.ToString();
            }

            if (initialValue is string initialValueTemp)
            {
                _value = ValidateAndFormatValue(initialValueTemp);
                ValueTextBox.Text = _value;
            }

            if (eventHandler is PropertyChangedEventHandler eventHandlerTemp)
            {
                PropertyChanged += eventHandlerTemp;
            }
        }

        private string ValidateAndFormatValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                string[] parts = input.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = (int.TryParse(parts[i], out int newInt) ? newInt : 0).ToString();
                }

                return string.Join(_separator, parts);
            }
            catch
            {
                return _value; // Return current valid value on error
            }
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string validatedValue = ValidateAndFormatValue(textBox.Text);
                
                // Only update if the validated value is different from what's shown
                if (textBox.Text != validatedValue && !string.IsNullOrEmpty(validatedValue))
                {
                    Value = validatedValue;
                }
                else if (!string.IsNullOrEmpty(textBox.Text))
                {
                    // Allow partial input during typing
                    _value = textBox.Text;
                }
            }
        }

        private void ValueTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow digits, separator, minus sign, and spaces
            if (sender is TextBox textBox)
            {
                char inputChar = e.Text.Length > 0 ? e.Text[0] : '\0';
                
                // Allow digits, separator, minus sign, and spaces
                if (!char.IsDigit(inputChar) && 
                    inputChar != _separator && 
                    inputChar != '-' && 
                    inputChar != ' ')
                {
                    e.Handled = true;
                }
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Ensure the textbox contains a valid formatted value when focus is lost
                string validatedValue = ValidateAndFormatValue(textBox.Text);
                Value = validatedValue;
            }
        }

        private void SeparatorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text.Length > 0)
            {
                char newSeparator = textBox.Text[0];
                
                if (_separator != newSeparator)
                {
                    _separator = newSeparator;
                    
                    // Revalidate current value with new separator
                    Value = _value;
                }
            }
        }
    }
}