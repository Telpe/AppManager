using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Settings.Interfaces;
using AppManager.Settings.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppManager.Settings.EditorControls
{
    /// <summary>
    /// Interaction logic for ConditionEditorControl.xaml
    /// </summary>
    public partial class ConditionEditorControl : UserControl, IInputEditControl
    {
        public event EventHandler? Edited;

        public event EventHandler? Cancel;

        public event EventHandler<InputEditEventArgs>? Save;

        private ConditionModel ConditionModelValue;

        public ConditionEditorControl(ConditionModel conditionModel)
        {
            ConditionModelValue = conditionModel;

            InitializeComponent();

            if (ConditionTypeComboBox is not null)
            {
                List<ConditionTypeEnum> conditionTypes = Enum.GetValues(typeof(ConditionTypeEnum))
                    .Cast<ConditionTypeEnum>()
                    .ToList();
                ConditionTypeComboBox.ItemsSource = conditionTypes;
                if (conditionTypes.Any())
                {
                    ConditionTypeComboBox.SelectedItem = ConditionModelValue.ConditionType;
                }
            }

            LoadFromModel();
            AttachEventHandlers();
        }

        private void LoadFromModel()
        {
            // Condition Type
            ConditionTypeComboBox.SelectedItem = ConditionModelValue.ConditionType;
            IsNotCheckBox.IsChecked = ConditionModelValue.IsNot;

            // Process Parameters
            ProcessNameTextBox.Text = ConditionModelValue.ProcessName ?? string.Empty;
            ExecutablePathTextBox.Text = ConditionModelValue.ExecutablePath ?? string.Empty;
            IncludeChildProcessesCheckBox.IsChecked = ConditionModelValue.IncludeChildProcesses ?? false;

            // File Parameters
            FilePathTextBox.Text = ConditionModelValue.FilePath ?? string.Empty;

            // Window Parameters
            WindowTitleTextBox.Text = ConditionModelValue.WindowTitle ?? string.Empty;
            WindowClassNameTextBox.Text = ConditionModelValue.WindowClassName ?? string.Empty;

            // Network Parameters
            PortTextBox.Text = ConditionModelValue.Port?.ToString() ?? string.Empty;
            IPAddressTextBox.Text = ConditionModelValue.IPAddress ?? string.Empty;

            // Time Parameters
            StartTimeTextBox.Text = ConditionModelValue.StartTime?.ToString(@"hh\:mm\:ss") ?? string.Empty;
            EndTimeTextBox.Text = ConditionModelValue.EndTime?.ToString(@"hh\:mm\:ss") ?? string.Empty;
            
            // Days of week
            var allowedDays = ConditionModelValue.AllowedDays ?? Array.Empty<DayOfWeek>();
            MondayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Monday);
            TuesdayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Tuesday);
            WednesdayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Wednesday);
            ThursdayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Thursday);
            FridayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Friday);
            SaturdayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Saturday);
            SundayCheckBox.IsChecked = allowedDays.Contains(DayOfWeek.Sunday);

            // System Parameters
            MinSystemUptimeTextBox.Text = ConditionModelValue.MinSystemUptimeMinutes?.ToString() ?? string.Empty;
            MaxSystemUptimeTextBox.Text = ConditionModelValue.MaxSystemUptimeMinutes?.ToString() ?? string.Empty;
            TimeoutMsTextBox.Text = ConditionModelValue.TimeoutMs?.ToString() ?? string.Empty;
        }

        private void AttachEventHandlers()
        {
            // Condition Type
            ConditionTypeComboBox.SelectionChanged += (s, e) => {
                ConditionModelValue.ConditionType = (ConditionTypeEnum)(ConditionTypeComboBox.SelectedItem ?? ConditionTypeEnum.ProcessRunning);
                AnnounceEdited();
            };
            
            IsNotCheckBox.Checked += (s, e) => { ConditionModelValue.IsNot = true; AnnounceEdited(); };
            IsNotCheckBox.Unchecked += (s, e) => { ConditionModelValue.IsNot = false; AnnounceEdited(); };

            // Process Parameters
            ProcessNameTextBox.TextChanged += (s, e) => {
                ConditionModelValue.ProcessName = string.IsNullOrWhiteSpace(ProcessNameTextBox.Text) ? null : ProcessNameTextBox.Text;
                AnnounceEdited();
            };
            
            ExecutablePathTextBox.TextChanged += (s, e) => {
                ConditionModelValue.ExecutablePath = string.IsNullOrWhiteSpace(ExecutablePathTextBox.Text) ? null : ExecutablePathTextBox.Text;
                AnnounceEdited();
            };
            
            IncludeChildProcessesCheckBox.Checked += (s, e) => { ConditionModelValue.IncludeChildProcesses = true; AnnounceEdited(); };
            IncludeChildProcessesCheckBox.Unchecked += (s, e) => { ConditionModelValue.IncludeChildProcesses = false; AnnounceEdited(); };

            // File Parameters
            FilePathTextBox.TextChanged += (s, e) => {
                ConditionModelValue.FilePath = string.IsNullOrWhiteSpace(FilePathTextBox.Text) ? null : FilePathTextBox.Text;
                AnnounceEdited();
            };

            // Window Parameters
            WindowTitleTextBox.TextChanged += (s, e) => {
                ConditionModelValue.WindowTitle = string.IsNullOrWhiteSpace(WindowTitleTextBox.Text) ? null : WindowTitleTextBox.Text;
                AnnounceEdited();
            };
            
            WindowClassNameTextBox.TextChanged += (s, e) => {
                ConditionModelValue.WindowClassName = string.IsNullOrWhiteSpace(WindowClassNameTextBox.Text) ? null : WindowClassNameTextBox.Text;
                AnnounceEdited();
            };

            // Network Parameters
            PortTextBox.TextChanged += (s, e) => {
                ConditionModelValue.Port = int.TryParse(PortTextBox.Text, out int port) ? port : null;
                AnnounceEdited();
            };
            
            IPAddressTextBox.TextChanged += (s, e) => {
                ConditionModelValue.IPAddress = string.IsNullOrWhiteSpace(IPAddressTextBox.Text) ? null : IPAddressTextBox.Text;
                AnnounceEdited();
            };

            // Time Parameters
            StartTimeTextBox.TextChanged += (s, e) => {
                ConditionModelValue.StartTime = TimeSpan.TryParse(StartTimeTextBox.Text, out TimeSpan startTime) ? startTime : null;
                AnnounceEdited();
            };
            
            EndTimeTextBox.TextChanged += (s, e) => {
                ConditionModelValue.EndTime = TimeSpan.TryParse(EndTimeTextBox.Text, out TimeSpan endTime) ? endTime : null;
                AnnounceEdited();
            };

            // Day checkboxes
            var dayCheckBoxes = new (CheckBox checkbox, DayOfWeek day)[] {
                (MondayCheckBox, DayOfWeek.Monday),
                (TuesdayCheckBox, DayOfWeek.Tuesday),
                (WednesdayCheckBox, DayOfWeek.Wednesday),
                (ThursdayCheckBox, DayOfWeek.Thursday),
                (FridayCheckBox, DayOfWeek.Friday),
                (SaturdayCheckBox, DayOfWeek.Saturday),
                (SundayCheckBox, DayOfWeek.Sunday)
            };

            foreach (var (checkbox, day) in dayCheckBoxes)
            {
                checkbox.Checked += (s, e) => UpdateAllowedDays();
                checkbox.Unchecked += (s, e) => UpdateAllowedDays();
            }

            // System Parameters
            MinSystemUptimeTextBox.TextChanged += (s, e) => {
                ConditionModelValue.MinSystemUptimeMinutes = int.TryParse(MinSystemUptimeTextBox.Text, out int minUptime) ? minUptime : null;
                AnnounceEdited();
            };
            
            MaxSystemUptimeTextBox.TextChanged += (s, e) => {
                ConditionModelValue.MaxSystemUptimeMinutes = int.TryParse(MaxSystemUptimeTextBox.Text, out int maxUptime) ? maxUptime : null;
                AnnounceEdited();
            };
            
            TimeoutMsTextBox.TextChanged += (s, e) => {
                ConditionModelValue.TimeoutMs = int.TryParse(TimeoutMsTextBox.Text, out int timeout) ? timeout : null;
                AnnounceEdited();
            };
        }

        private void UpdateAllowedDays()
        {
            var selectedDays = new List<DayOfWeek>();
            
            if (MondayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Monday);
            if (TuesdayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Tuesday);
            if (WednesdayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Wednesday);
            if (ThursdayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Thursday);
            if (FridayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Friday);
            if (SaturdayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Saturday);
            if (SundayCheckBox.IsChecked == true) selectedDays.Add(DayOfWeek.Sunday);

            ConditionModelValue.AllowedDays = selectedDays.Count > 0 ? selectedDays.ToArray() : null;
            AnnounceEdited();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ConditionModelValue.ConditionType = (ConditionTypeEnum)ConditionTypeComboBox.SelectedItem;

            DoSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DoCancel();
        }

        protected void AnnounceEdited()
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }

        protected void DoCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        protected void DoSave()
        {
            Save?.Invoke(this, new InputEditEventArgs(ConditionModelValue.Clone()));
        }
    }
}
