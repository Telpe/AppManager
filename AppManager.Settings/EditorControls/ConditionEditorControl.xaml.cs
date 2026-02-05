using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Settings.Interfaces;
using AppManager.Settings.ParameterControls;
using AppManager.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

            InitTypeSelector();

            LoadFromModel();

            AttachEventHandlers();
        }

        private void InitTypeSelector()
        {
            int tspI = TypeSelectionPanel.Children.Add(new TypeSelectParameter(typeof(ConditionTypeEnum), null, null, "Choose:"));
            int notI = TypeSelectionPanel.Children.Add(new CheckBox() { Content = "Invert condition (NOT)", Margin = new Thickness(85, 5, 0, 0) });

            (TypeSelectionPanel.Children[tspI] as TypeSelectParameter)!.Selected = ConditionModelValue.ConditionType;
            (TypeSelectionPanel.Children[tspI] as TypeSelectParameter)!.PropertyChanged += ConditionTypeComboBox_SelectionChanged;

            (TypeSelectionPanel.Children[notI] as CheckBox)!.IsChecked = ConditionModelValue.IsNot;
            (TypeSelectionPanel.Children[notI] as CheckBox)!.Checked += IsNotCheckBox_Checked;
            (TypeSelectionPanel.Children[notI] as CheckBox)!.Unchecked += IsNotCheckBox_Unchecked;
        }

        private void LoadFromModel()
        {
            try
            {
                ConditionParameters.Children.Clear();

                if (TypeSelectionPanel!.Children[0] is TypeSelectParameter { Selected: ConditionTypeEnum conditionType })
                {
                    switch (conditionType)
                    {
                        case ConditionTypeEnum.DayOfWeek:
                            //AddParametersFromInterface(typeof(IDayOfWeekCondition));
                            break;
                        case ConditionTypeEnum.FileExists:
                            AddParametersFromInterface(typeof(IFileExistsCondition));
                            break;
                        case ConditionTypeEnum.NetworkPortOpen:
                            //AddParametersFromInterface(typeof(INetworkPortOpenCondition));
                            break;
                        case ConditionTypeEnum.PreviousActionSuccess:
                            AddParametersFromInterface(typeof(IPreviousActionSuccessCondition));
                            break;
                        case ConditionTypeEnum.ProcessRunning:
                            AddParametersFromInterface(typeof(IProcessRunningCondition));
                            break;
                        case ConditionTypeEnum.SystemUptime:
                            //AddParametersFromInterface(typeof(ISystemUptimeCondition));
                            break;
                        case ConditionTypeEnum.TimeRange:
                            //AddParametersFromInterface(typeof(ITimeRangeCondition));
                            break;
                        
                        case ConditionTypeEnum.WindowExists:
                            //AddParametersFromInterface(typeof(IWindowExistsCondition));
                            break;
                        case ConditionTypeEnum.WindowFocused:
                            //AddParametersFromInterface(typeof(IWindowFocusedCondition));
                            break;
                        case ConditionTypeEnum.WindowMinimized:
                            //AddParametersFromInterface(typeof(IWindowMinimizedCondition));
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine($"Loading Condition data error: {ex.Message}");
                throw;
            }

            // Process Parameters
            ProcessNameTextBox.Text = ConditionModelValue.ProcessName ?? string.Empty;
            ExecutablePathTextBox.Text = ConditionModelValue.ExecutablePath ?? string.Empty;
            IncludeChildProcessesCheckBox.IsChecked = ConditionModelValue.IncludeChildProcesses ?? false;

            // File Parameters
            FilePathTextBox.Text = ConditionModelValue.FilePath ?? string.Empty;

            // Window Parameters
            WindowTitleTextBox.Text = ConditionModelValue.WindowTitle ?? string.Empty;

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

        private void AddParametersFromInterface(Type modelType)
        {
            foreach (PropertyInfo prop in modelType.GetProperties())
            {
                switch (prop.Name)
                {
                    case nameof(ConditionModel.ProcessName):

                        ProcessParameter pp = new(ConditionModelValue.ProcessName);
                        UseParameterGroupBox("App Name:").Content = pp;
                        pp.PropertyChanged += ProcessNameTextBox_TextChanged;
                        break;

                    case nameof(ConditionModel.ExecutablePath):

                        FilePathParameter fpp = new(ConditionModelValue.ExecutablePath);
                        UseParameterGroupBox("Executable Path:").Content = fpp;
                        fpp.PropertyChanged += ExecutablePathTextBox_TextChanged;
                        break;

                    case nameof(ConditionModel.AllowedDays):

                        //ExeArgumentsParameter fpp = new(ConditionModelValue.Arguments);
                        //UseParameterGroupBox("Executable Path:").Content = fpp;
                        //fpp.PropertyChanged += ExecutablePathTextBox_TextChanged;
                        //AddTextBoxRow(ConditionModelValue.WindowTitle ?? String.Empty, "Arguments:");
                        break;

                    
                }
            }
        }

        private GroupBox UseParameterGroupBox(string header)
        {
            GroupBox? box = null;

            foreach (object aBox in ConditionParameters.Children)
            {
                if (aBox is GroupBox gb
                    && gb.Header is string sh
                    && sh == header)
                {
                    box = gb;
                    break;
                }
            }

            if (box is null)
            {
                box = new GroupBox
                {
                    Header = header
                };

                ConditionParameters.Children.Add(box);
            }

            return box;
        }

        private StackPanel UseParameterStackPanel(string header, Orientation orientation = Orientation.Vertical)
        {
            GroupBox box = UseParameterGroupBox(header);

            if (box.Content is StackPanel sp)
            {
                return sp;
            }

            if (box.Content is null)
            {
                box.Content = new StackPanel
                {
                    Orientation = orientation
                };

                return (StackPanel)box.Content;
            }

            throw new InvalidOperationException($"GroupBox with header '{header}', has Content that is not of type {typeof(StackPanel).Name}");
        }

        




        private void AttachEventHandlers()
        {
            
            IncludeChildProcessesCheckBox.Checked += IncludeChildProcessesCheckBox_Checked;
            IncludeChildProcessesCheckBox.Unchecked += IncludeChildProcessesCheckBox_Unchecked;

            // File Parameters
            FilePathTextBox.TextChanged += FilePathTextBox_TextChanged;

            // Window Parameters
            WindowTitleTextBox.TextChanged += WindowTitleTextBox_TextChanged;

            // Network Parameters
            PortTextBox.TextChanged += PortTextBox_TextChanged;
            IPAddressTextBox.TextChanged += IPAddressTextBox_TextChanged;

            // Time Parameters
            StartTimeTextBox.TextChanged += StartTimeTextBox_TextChanged;
            EndTimeTextBox.TextChanged += EndTimeTextBox_TextChanged;

            // Day checkboxes
            MondayCheckBox.Checked += DayCheckBox_CheckedChanged;
            MondayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            TuesdayCheckBox.Checked += DayCheckBox_CheckedChanged;
            TuesdayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            WednesdayCheckBox.Checked += DayCheckBox_CheckedChanged;
            WednesdayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            ThursdayCheckBox.Checked += DayCheckBox_CheckedChanged;
            ThursdayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            FridayCheckBox.Checked += DayCheckBox_CheckedChanged;
            FridayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            SaturdayCheckBox.Checked += DayCheckBox_CheckedChanged;
            SaturdayCheckBox.Unchecked += DayCheckBox_CheckedChanged;
            SundayCheckBox.Checked += DayCheckBox_CheckedChanged;
            SundayCheckBox.Unchecked += DayCheckBox_CheckedChanged;

            // System Parameters
            MinSystemUptimeTextBox.TextChanged += MinSystemUptimeTextBox_TextChanged;
            MaxSystemUptimeTextBox.TextChanged += MaxSystemUptimeTextBox_TextChanged;
            TimeoutMsTextBox.TextChanged += TimeoutMsTextBox_TextChanged;
        }

        private void ConditionTypeComboBox_SelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeSelectParameter tsp && tsp.Selected is ConditionTypeEnum typeEnum)
            {
                ConditionModelValue.ConditionType = typeEnum;

                LoadFromModel();

                AnnounceEdited();

                return;
            }

            throw new InvalidOperationException($"{nameof(ConditionTypeComboBox_SelectionChanged)}: {nameof(sender)} is not {nameof(TypeSelectParameter)} or selected {nameof(TypeSelectParameter.Selected)} is not {nameof(ConditionTypeEnum)}");
        }

        private void IsNotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConditionModelValue.IsNot = true;
            AnnounceEdited();
        }

        private void IsNotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConditionModelValue.IsNot = false;
            AnnounceEdited();
        }

        private void ProcessNameTextBox_TextChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ProcessParameter pp)
            {
                ConditionModelValue.ProcessName = string.IsNullOrWhiteSpace(pp.Value) ? null : pp.Value;
                AnnounceEdited();
            }
        }

        private void ExecutablePathTextBox_TextChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is FilePathParameter pp)
            {
                ConditionModelValue.ExecutablePath = string.IsNullOrWhiteSpace(pp.Value) ? null : pp.Value;
                AnnounceEdited();
            }
        }

        private void IncludeChildProcessesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConditionModelValue.IncludeChildProcesses = true;
            AnnounceEdited();
        }

        private void IncludeChildProcessesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConditionModelValue.IncludeChildProcesses = false;
            AnnounceEdited();
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.FilePath = string.IsNullOrWhiteSpace(FilePathTextBox.Text) ? null : FilePathTextBox.Text;
            AnnounceEdited();
        }

        private void WindowTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.WindowTitle = string.IsNullOrWhiteSpace(WindowTitleTextBox.Text) ? null : WindowTitleTextBox.Text;
            AnnounceEdited();
        }

        private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.Port = int.TryParse(PortTextBox.Text, out int port) ? port : null;
            AnnounceEdited();
        }

        private void IPAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.IPAddress = string.IsNullOrWhiteSpace(IPAddressTextBox.Text) ? null : IPAddressTextBox.Text;
            AnnounceEdited();
        }

        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.StartTime = TimeSpan.TryParse(StartTimeTextBox.Text, out TimeSpan startTime) ? startTime : null;
            AnnounceEdited();
        }

        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.EndTime = TimeSpan.TryParse(EndTimeTextBox.Text, out TimeSpan endTime) ? endTime : null;
            AnnounceEdited();
        }

        private void DayCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateAllowedDays();
        }

        private void MinSystemUptimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.MinSystemUptimeMinutes = int.TryParse(MinSystemUptimeTextBox.Text, out int minUptime) ? minUptime : null;
            AnnounceEdited();
        }

        private void MaxSystemUptimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.MaxSystemUptimeMinutes = int.TryParse(MaxSystemUptimeTextBox.Text, out int maxUptime) ? maxUptime : null;
            AnnounceEdited();
        }

        private void TimeoutMsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConditionModelValue.TimeoutMs = int.TryParse(TimeoutMsTextBox.Text, out int timeout) ? timeout : null;
            AnnounceEdited();
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
