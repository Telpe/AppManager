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

    public partial class TimerParameter : UserControl, INotifyPropertyChanged
    {
        private int? _timerValue;
        private TimerEditMode _editMode = TimerEditMode.MinutesSecondsMilliseconds;
        private bool _isUpdatingFromSlider;
        private bool _isUpdatingFromFields;
        private Stopwatch? _holdTimer;
        private Task? StepField;
        private Button? CurrentHeldButton;
        private int CurrentIncrement = 0;
        private Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

        private const int MAX_VALUE = 180000; // 3 minutes in milliseconds
        private const int MIN_VALUE = -1;
        private const int HOLD_INITIAL_DELAY = 300;
        private const int HOLD_REPEAT_INTERVAL = 167;

        public event EventHandler? TimerValueChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public int? TimerValue
        {
            get => _timerValue;
            set
            {
                if (_timerValue != value)
                {
                    _timerValue = -1 < value ? value : null;
                    if (MAX_VALUE < _timerValue) { _timerValue = MAX_VALUE; }

                    if (!_isUpdatingFromSlider)
                    {
                        UpdateSliderFromValue();
                    }

                    UpdateFieldsFromValue();

                    OnTimerValueChanged();
                    OnPropertyChanged(nameof(TimerValue));
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
            UpdateSliderFromValue();
        }

        public TimerParameter(int? timerValue) : this()
        {
            TimerValue = timerValue;
        }

        private void UpdateModeVisibility()
        {
            Mode1Panel.Visibility = EditMode == TimerEditMode.Milliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode2Panel.Visibility = EditMode == TimerEditMode.SecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
            Mode3Panel.Visibility = EditMode == TimerEditMode.MinutesSecondsMilliseconds ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFieldsFromValue()
        {
            int milliseconds = _timerValue ?? -1;

            switch (EditMode)
            {
                case TimerEditMode.Milliseconds:
                    MillisecondsTextBox.Text = milliseconds.ToString();
                    break;
                
                case TimerEditMode.SecondsMilliseconds:
                    int seconds = milliseconds / 1000;
                    int remainingMs = milliseconds % 1000;
                    SecondsTextBox.Text = seconds.ToString();
                    MillisecondsMode2TextBox.Text = remainingMs.ToString();
                    break;
                
                case TimerEditMode.MinutesSecondsMilliseconds:
                    int minutes = milliseconds / 60000;
                    int remainingAfterMinutes = milliseconds % 60000;
                    int secondsMode3 = remainingAfterMinutes / 1000;
                    int msMode3 = remainingAfterMinutes % 1000;
                    MinutesTextBox.Text = minutes.ToString();
                    SecondsMode3TextBox.Text = secondsMode3.ToString();
                    MillisecondsMode3TextBox.Text = msMode3.ToString();
                    break;
            }
        }

        private void UpdateSliderFromValue()
        {
            TimerSlider.Value = _timerValue ?? -1;
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

                // Handle -1 special case (null)
                if (newValue == -1)
                {
                    TimerValue = null;
                }
                else
                {
                    // Clamp to valid range
                    newValue = Math.Max(MIN_VALUE, Math.Min(MAX_VALUE, newValue));
                    TimerValue = newValue;
                }
            }
            finally
            {
                _isUpdatingFromFields = false;
            }
        }

        private void NormalizeFields()
        {
            switch (EditMode)
            {
                case TimerEditMode.SecondsMilliseconds:
                    if (int.TryParse(SecondsTextBox.Text, out int sec) && 
                        int.TryParse(MillisecondsMode2TextBox.Text, out int ms))
                    {
                        if (ms >= 1000)
                        {
                            sec += ms / 1000;
                            ms = ms % 1000;
                        }
                        else if (ms < 0 && sec > 0)
                        {
                            sec -= 1;
                            ms += 1000;
                        }
                        
                        SecondsTextBox.Text = Math.Max(0, sec).ToString();
                        MillisecondsMode2TextBox.Text = Math.Max(0, ms).ToString();
                    }
                    break;
                
                case TimerEditMode.MinutesSecondsMilliseconds:
                    if (int.TryParse(MinutesTextBox.Text, out int min) && 
                        int.TryParse(SecondsMode3TextBox.Text, out int sec3) && 
                        int.TryParse(MillisecondsMode3TextBox.Text, out int ms3))
                    {
                        // Normalize milliseconds to seconds
                        if (ms3 >= 1000)
                        {
                            sec3 += ms3 / 1000;
                            ms3 = ms3 % 1000;
                        }
                        else if (ms3 < 0 && sec3 > 0)
                        {
                            sec3 -= 1;
                            ms3 += 1000;
                        }
                        
                        // Normalize seconds to minutes
                        if (sec3 >= 60)
                        {
                            min += sec3 / 60;
                            sec3 = sec3 % 60;
                        }
                        else if (sec3 < 0 && min > 0)
                        {
                            min -= 1;
                            sec3 += 60;
                        }
                        
                        MinutesTextBox.Text = Math.Max(0, min).ToString();
                        SecondsMode3TextBox.Text = Math.Max(0, sec3).ToString();
                        MillisecondsMode3TextBox.Text = Math.Max(0, ms3).ToString();
                    }
                    break;
            }
        }

        #region Event Handlers

        private void EditModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditModeComboBox!.SelectedIndex >= 0)
            {
                EditMode = (TimerEditMode)(EditModeComboBox.SelectedIndex + 1);
            }
        }

        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _isUpdatingFromSlider = true;

            try
            {
                TimerValue = (int)e.NewValue;
            }
            finally
            {
                _isUpdatingFromSlider = false;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            NormalizeFields();
            UpdateValueFromFields();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NormalizeFields();
                UpdateValueFromFields();
            }
        }

        private void IncrementButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                CurrentHeldButton = button;
                CurrentIncrement = 1;
                StartHoldTimer();
            }
        }

        private void DecrementButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                CurrentHeldButton = button;
                CurrentIncrement = -1;
                StartHoldTimer();
            }
        }

        private void ArrowButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_holdTimer is null) { return; }

            if (_holdTimer.ElapsedMilliseconds < HOLD_INITIAL_DELAY)
            {
                IncrementField();
            }
            StopHoldTimer();
        }

        private void ArrowButton_MouseLeave(object sender, MouseEventArgs e)
        {
            StopHoldTimer();
        }

        #endregion

        #region Hold Timer Logic

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
            CurrentHeldButton = null;
            CurrentIncrement = 0;
            StepField = null;
        }

        private void IncrementField()
        {
            try
            {
                TextBox? targetTextBox = null;
                
                if (CurrentHeldButton!.Tag is string tag)
                {
                    targetTextBox = tag switch
                    {
                        "Milliseconds" => MillisecondsTextBox,
                        "Seconds" => SecondsTextBox,
                        "MillisecondsMode2" => MillisecondsMode2TextBox,
                        "Minutes" => MinutesTextBox,
                        "SecondsMode3" => SecondsMode3TextBox,
                        "MillisecondsMode3" => MillisecondsMode3TextBox,
                        _ => null
                    };
                }
                
                if (targetTextBox != null && int.TryParse(targetTextBox.Text, out int currentValue))
                {
                    targetTextBox.Text = (currentValue + CurrentIncrement).ToString();
                    UpdateValueFromFields();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"IncrementField error: {ex.Message}");
            }
        }

        #endregion

        protected virtual void OnTimerValueChanged()
        {
            TimerValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}