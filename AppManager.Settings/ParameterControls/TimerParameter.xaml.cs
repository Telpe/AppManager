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

    public partial class TimerParameter : UserControl, INotifyPropertyChanged // , INotifyValueChanged<int>
    {
        private int TimerValue = MIN_VALUE;
        private TimerEditMode _editMode = TimerEditMode.MinutesSecondsMilliseconds;
        private bool _isUpdatingFromFields;
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
        public string HeaderText { get; } = "Timer";

        //public event EventHandler? OnValueChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Value
        {
            get 
            { 
                return TimerValue; 
            }
            set
            {
                if (TimerValue != value)
                {
                    int validatedValue = value < MIN_VALUE ? MIN_VALUE : (MAX_VALUE < value ? MAX_VALUE : value);

                    if (TimerValue != validatedValue)
                    {
                        TimerValue = validatedValue;

                        UpdateFieldsFromValue();
                        
                        BroadcastTimerValueChanged();
                    }
                    else
                    {
                        UpdateFieldsFromValue();
                    }
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
            InitializeComponent();
            this.DataContext = this;
            UpdateModeVisibility();

            UpdateFieldsFromValue();
        }

        public TimerParameter(int? milliseconds = null, string? header = null) : this()
        {
            if (milliseconds is not null) { TimerValue = (int)milliseconds; }
            if (header is not null) { HeaderText = header; }
        }

        private void UpdateModeVisibility()
        {
            Mode1Panel.Visibility = EditMode == TimerEditMode.Milliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode2Panel.Visibility = EditMode == TimerEditMode.SecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode3Panel.Visibility = EditMode == TimerEditMode.MinutesSecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFieldsFromValue()
        {
            switch (EditMode)
            {
                case TimerEditMode.Milliseconds:
                    MillisecondsTextBox.Text = TimerValue.ToString();
                    break;
                
                case TimerEditMode.SecondsMilliseconds:
                    int seconds = TimerValue / 1000;
                    int remainingMs = TimerValue % 1000;
                    SecondsTextBox.Text = seconds.ToString();
                    MillisecondsMode2TextBox.Text = remainingMs.ToString();
                    break;
                
                case TimerEditMode.MinutesSecondsMilliseconds:
                    int minutes = TimerValue / 60000;
                    int remainingAfterMinutes = TimerValue % 60000;
                    int secondsMode3 = remainingAfterMinutes / 1000;
                    int msMode3 = remainingAfterMinutes % 1000;
                    MinutesTextBox.Text = minutes.ToString();
                    SecondsMode3TextBox.Text = secondsMode3.ToString();
                    MillisecondsMode3TextBox.Text = msMode3.ToString();
                    break;
            }
        }

        private void UpdateValueFromFields()
        {
            _isUpdatingFromFields = true;
            try
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

                Value = newValue;
            }
            finally
            {
                _isUpdatingFromFields = false;
            }
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
            // TODO: only if changed
            UpdateValueFromFields();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateValueFromFields();
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
                    UpdateValueFromFields();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"IncrementField error: {ex.Message}");
            }
        }

        protected virtual void BroadcastTimerValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            //OnValueChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}