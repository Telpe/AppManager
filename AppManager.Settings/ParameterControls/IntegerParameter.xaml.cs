using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class IntegerParameter : BaseParameterControl
    {
        private int _value = 0;
        private int _minValue = 0;
        private int _maxValue = 0;

        public int Value
        {
            get => _value;
            set
            {
                // Apply min/max limits
                int newValue = value;

                if (_minValue < _maxValue)
                {
                    if (newValue < _minValue)
                    {
                        newValue = _minValue;
                    }
                    else if (_maxValue < newValue)
                    {
                        newValue = _maxValue;
                    }
                }

                

                // If not from textbox, update textbox
                if (ValueTextBox.Text != newValue.ToString())
                {
                    ValueTextBox.Text = newValue.ToString();
                }

                // If value changed, update and broadcast
                if (_value != newValue)
                {
                    _value = newValue;
                    AnnouncePropertyChanged(ValueName);
                }
            }
        }

        public int MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue != value)
                {
                    _minValue = _maxValue < value ? _maxValue : value;
                    Value = _value;
                }
            }
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = value < _minValue ? _minValue : value;
                    Value = _value;
                }
            }
        }

        public IntegerParameter()
        {
            _labelText = "Value:";
            _headerText = "Integer Parameter";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;
        }

        public IntegerParameter(int? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null, int? minValue = null, int? maxValue = null) : this()
        {
            if (headerText is string headerTextTemp)
            {
                _headerText = headerTextTemp;
            }
            
            if (labelText is string labelTextTemp)
            {
                _labelText = labelTextTemp;
            }

            if (customValueName is string customValueNameTemp)
            {
                ValueName = customValueNameTemp;
            }

            if (minValue is int minValueTemp) 
            { 
                _minValue = minValueTemp; 
            }
            
            if (maxValue is int maxValueTemp) 
            { 
                _maxValue = maxValueTemp; 
            }

            if (initialValue is int initialValueTemp)
            {
                _value = initialValueTemp;
                ValueTextBox.Text = _value.ToString();
            }

            if (eventHandler is PropertyChangedEventHandler eventHandlerTemp)
            {
                PropertyChanged += eventHandlerTemp;
            }
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int result))
                {
                    Value = result;
                }
                else
                {
                    // Invalid input, revert to current value
                    textBox.Text = _value.ToString();
                    textBox.SelectAll();
                }
            }
        }

        private void ValueTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Only allow numeric input and minus sign at the beginning
            if (sender is TextBox textBox)
            {
                string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
                
                // Allow empty string (for clearing)
                if (string.IsNullOrEmpty(newText))
                {
                    return;
                }

                // Check if the new text would be a valid integer
                e.Handled = !int.TryParse(newText, out _);
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Ensure the textbox contains a valid value when focus is lost
                if (int.TryParse(textBox.Text, out int result))
                {
                    Value = result;
                }
                else
                {
                    textBox.Text = _value.ToString();
                }
            }
        }
    }
}