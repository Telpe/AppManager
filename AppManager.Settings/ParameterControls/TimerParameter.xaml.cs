using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AppManager.Core.Utilities;

namespace AppManager.Settings.ParameterControls
{
    public enum TimerEditMode
    {
        Milliseconds = 1,
        SecondsMilliseconds = 2,
        MinutesSecondsMilliseconds = 3
    }

    public partial class TimerParameter : BaseParameterControl
    {
        private int TimerValue = MIN_VALUE;
        private TimerEditMode _editMode = TimerEditMode.MinutesSecondsMilliseconds;
        private bool _isUpdatingFields;
        private Stopwatch? _holdTimer;
        private Task? StepField;
        private TextBox? TextBoxToIncrement;
        private int CurrentIncrement = 0;
        private readonly Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

        protected const int MAX_VALUE = 180000;
        protected const int MIN_VALUE = -1;
        protected const int HOLD_INITIAL_DELAY = 300;
        protected const int HOLD_REPEAT_INTERVAL = 167;
        public static double SliderMinimum { get => MIN_VALUE; }
        public static double SliderMaximum { get => MAX_VALUE; }

        public int Value
        {
            get 
            { 
                return TimerValue; 
            }
            set
            {
                if (_isUpdatingFields) { return; }

                int validatedValue = value < MIN_VALUE ? MIN_VALUE : (MAX_VALUE < value ? MAX_VALUE : value);

                UpdateFieldsFromValue(validatedValue);

                if (TimerValue != validatedValue)
                {
                    TimerValue = validatedValue;
                    BroadcastPropertyChanged(ValueName);
                }
            }
        }

        public TimerEditMode EditMode
        {
            get => _editMode;
            set
            {
                if (_editMode != value)
                {
                    _editMode = value;
                    UpdateModeVisibility();
                    UpdateFieldsFromValue();
                }
            }
        }

        public TimerParameter()
        {
            _headerText = "Timespan:";
            _labelText = "Duration:";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;

            UpdateModeVisibility();
            UpdateFieldsFromValue();
        }

        public TimerParameter(int? milliseconds = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (milliseconds is not null) { TimerValue = (int)milliseconds; }

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

            UpdateFieldsFromValue();

            if (eventHandler != null)
            {
                PropertyChanged += eventHandler;
            }
        }

        private void UpdateModeVisibility()
        {
            Mode1Panel.Visibility = EditMode == TimerEditMode.Milliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode2Panel.Visibility = EditMode == TimerEditMode.SecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode3Panel.Visibility = EditMode == TimerEditMode.MinutesSecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFieldsFromValue(int? aValue = null)
        {
            _isUpdatingFields = true;

            try
            {
                aValue ??= TimerValue;

                switch (EditMode)
                {
                    case TimerEditMode.Milliseconds:
                        MillisecondsTextBox.Text = aValue.ToString();
                        break;
                
                    case TimerEditMode.SecondsMilliseconds:
                        int seconds = (int)aValue / 1000;
                        int remainingMs = (int)aValue % 1000;
                        SecondsTextBox.Text = seconds.ToString();
                        MillisecondsMode2TextBox.Text = remainingMs.ToString();
                        break;
                
                    case TimerEditMode.MinutesSecondsMilliseconds:
                        int minutes = (int)aValue / 60000;
                        int remainingAfterMinutes = (int)aValue % 60000;
                        int secondsMode3 = remainingAfterMinutes / 1000;
                        int msMode3 = remainingAfterMinutes % 1000;
                        MinutesTextBox.Text = minutes.ToString();
                        SecondsMode3TextBox.Text = secondsMode3.ToString();
                        MillisecondsMode3TextBox.Text = msMode3.ToString();
                        break;
                }
            }
            finally { _isUpdatingFields = false; }
        }

        private int GetValueFromFields()
        {
            int newValue = 0;
                
            switch (EditMode)
            {
                case TimerEditMode.Milliseconds:
                    if (int.TryParse(MillisecondsTextBox.Text, out int ms))
                        newValue = ms;
                    break;
                    
                case TimerEditMode.SecondsMilliseconds:
                    if (int.TryParse(SecondsTextBox.Text, out int sec) && 
                        int.TryParse(MillisecondsMode2TextBox.Text, out int ms2))
                    {
                        newValue = (sec * 1000) + ms2;
                    }
                    break;
                    
                case TimerEditMode.MinutesSecondsMilliseconds:
                    if (int.TryParse(MinutesTextBox.Text, out int min) && 
                        int.TryParse(SecondsMode3TextBox.Text, out int sec3) && 
                        int.TryParse(MillisecondsMode3TextBox.Text, out int ms3))
                    {
                        newValue = (min * 60000) + (sec3 * 1000) + ms3;
                    }
                    break;
            }

            return newValue;
        }

        private void EditModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditModeComboBox!.SelectedIndex >= 0)
            {
                EditMode = (TimerEditMode)(EditModeComboBox.SelectedIndex + 1);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Value = GetValueFromFields();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Value = GetValueFromFields();
            }
        }

        private void IncrementButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {

                TextBoxToIncrement = textBox;
                CurrentIncrement = 1;
                StartHoldTimer();
            }
            e.Handled = true;
        }

        private void DecrementButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {
                TextBoxToIncrement = textBox;
                CurrentIncrement = -1;
                StartHoldTimer();
            }
            e.Handled = true;
        }

        private void ArrowButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_holdTimer is not null && _holdTimer.ElapsedMilliseconds < HOLD_INITIAL_DELAY)
            {
                IncrementField();
            }
            StopHoldTimer();
            e.Handled = true;
        }

        private void ArrowButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if(_holdTimer is not null) 
            { 
                StopHoldTimer(); 
            }
        }

        

        private void StartHoldTimer()
        {
            
            _holdTimer = new Stopwatch();
            _holdTimer.Start();

            StepField = new Task(() =>
            {
                Thread.Sleep(HOLD_INITIAL_DELAY);

                while (_holdTimer is not null)
                {
                    MainDispatcher.Invoke(() =>
                    {
                        IncrementField();
                    });
                    Thread.Sleep(HOLD_REPEAT_INTERVAL);
                }
            });

            StepField.Start();
        }

        private void StopHoldTimer()
        {
            _holdTimer?.Stop();
            _holdTimer = null;
            TextBoxToIncrement = null;
            CurrentIncrement = 0;
            StepField = null;
        }

        private void IncrementField()
        {
            try
            {
                if (TextBoxToIncrement != null && int.TryParse(TextBoxToIncrement.Text, out int currentValue))
                {
                    TextBoxToIncrement.Text = (currentValue + CurrentIncrement).ToString();
                    Value = GetValueFromFields();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"IncrementField error: {ex.Message}");
            }
        }


    }
}