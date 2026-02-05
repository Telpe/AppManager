using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class StringParameter : BaseParameterControl
    {
        private string _value = string.Empty;
        private bool AsAreaValue = false;
        private int _maxLength = -1; // Less than 1 means no limit

        public string Value
        {
            get => _value;
            set
            {
                // !! NO LOOP !!
                // set length
                // if not from textbox, update textbox
                // textbox change will call back to set value, so no need to broadcast again.
                // -no length change.
                // -textbox already updated.
                // -has value actually changed, not if just truncated, else update and broadcast.
                // if textbox changed, _value is always equal to newValue, so no need to update or broadcast.
                // if textbox not changed, all is well there, so we just need to see if value actually changed, if so update and broadcast.


                string newValue = 0 < _maxLength && _maxLength < value.Length 
                    ? value[.._maxLength] 
                    : value;

                // if not as textbox, update textbox
                if (ValueTextBox.Text != newValue)
                {
                    ValueTextBox.Text = newValue;
                }

                // if value changed, update and broadcast
                if (_value != newValue)
                {
                    _value = newValue;
                    BroadcastPropertyChanged(ValueName);
                }
            }
        }

        public bool AsArea
        {
            get => AsAreaValue;
            set
            {
                if (AsAreaValue != value)
                {
                    AsAreaValue = value;
                    UpdateControlVisibility();
                }
            }
        }

        public int MaxLength
        {
            get => _maxLength;
            set
            {
                if (_maxLength != value)
                {
                    _maxLength = value < 1 ? int.MaxValue : value;
                    ValueTextBox.MaxLength = _maxLength;
                }
            }
        }

        public StringParameter()
        {
            _labelText = "Value:";
            _headerText = "String Parameter";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;

            UpdateControlVisibility();
        }

        public StringParameter(string? initialValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null, bool asArea = false, int maxLength = -1) : this()
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

            _maxLength = maxLength;
            AsAreaValue = asArea;

            if (_maxLength > 0)
            {
                ValueTextBox.MaxLength = _maxLength;
            }

            if (initialValue != null)
            {
                _value = initialValue;
            }

            UpdateControlVisibility();

            if (eventHandler != null)
            {
                PropertyChanged += eventHandler;
            }
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Value = textBox.Text;
            }
        }

        private void UpdateControlVisibility()
        {
            if (AsAreaValue)
            {
                ValueTextBox.TextWrapping = TextWrapping.Wrap;
                ValueTextBox.AcceptsReturn = true;
            }
            else
            {
                ValueTextBox.TextWrapping = TextWrapping.NoWrap;
                ValueTextBox.AcceptsReturn = false;
            }
        }

    }
}