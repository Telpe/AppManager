using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Settings.Interfaces;
using AppManager.Settings.ParameterControls;
using AppManager.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AppManager.Settings.EditorControls
{
    /// <summary>
    /// Interaction logic for ConditionEditorControl.xaml
    /// </summary>
    public partial class ConditionEditorControl : UserControl, IInputEditControl
    {
        private readonly string AllowedDaysPanelName = "Allowed Days:";

        public event EventHandler? OnEdited;

        public event EventHandler? OnCancel;

        public event EventHandler<InputEditEventArgs>? OnSave;

        private ConditionModel ConditionModelValue;

        public ConditionEditorControl(ConditionModel conditionModel)
        {
            ConditionModelValue = conditionModel;

            InitializeComponent();

            InitTypeSelector();

            LoadFromModel();

        }

        private void InitTypeSelector()
        {
            int tspI = TypeSelectionPanel!.Children.Add(new TypeSelectParameter(typeof(ConditionTypeEnum), null, null, "Choose:"));
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
                    foreach (PropertyInfo prop in GetConditionType(conditionType).GetProperties())
                    {
                        AddParameterFromProperty(prop);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine($"Loading Condition data error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private Type GetConditionType(ConditionTypeEnum conditionType)
        {
            switch (conditionType)
            {
                case ConditionTypeEnum.DayOfWeek:
                    //AddParametersFromInterface(typeof(IDayOfWeekCondition));
                    break;

                case ConditionTypeEnum.FileExists:
                    return typeof(IFileExistsCondition);

                case ConditionTypeEnum.NetworkPortOpen:
                    //AddParametersFromInterface(typeof(INetworkPortOpenCondition));
                    break;

                case ConditionTypeEnum.PreviousActionSuccess:
                    return typeof(IPreviousActionSuccessCondition);

                case ConditionTypeEnum.ProcessRunning:
                    return typeof(IProcessRunningCondition);

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

            throw new InvalidOperationException($"Unsupported {nameof(ConditionTypeEnum)}: {conditionType}");
        }

        private void AddParameterFromProperty(PropertyInfo prop)
        {
            switch (prop.Name)
            {
                case nameof(ConditionModel.AllowedDays):

                    var allowedDays = ConditionModelValue.AllowedDays ?? Array.Empty<DayOfWeek>();

                    var daysPanel = UseParameterStackPanel(AllowedDaysPanelName, Orientation.Horizontal);

                    foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
                    {
                        daysPanel.Children.Add(new BooleanParameter(
                            allowedDays.Contains(day),
                            (s, e) => { 
                                UpdateAllowedDays(); 
                                AnnounceEdited(); 
                            },
                            prop.Name,
                            null,
                            day.ToString()));
                    }

                    break;
                /*
                                case nameof(ConditionModel.ConditionType):

                                    UseParameterGroupBox("Condition Type:").Content = new StringParameter(
                                        ConditionModelValue.ConditionType.ToString(),
                                        (s, e) =>
                                        {
                                            if (s is StringParameter { Value: string pv })
                                            {
                                                if (Enum.TryParse<ConditionTypeEnum>(pv, out var conditionType))
                                                {
                                                    ConditionModelValue.ConditionType = conditionType;
                                                    AnnounceEdited();
                                                }
                                            }
                                        },
                                        prop.Name);
                                    break;
                */
                case nameof(ConditionModel.CustomProperties):

                    UseParameterGroupBox("Custom Properties:").Content = new StringParameter(
                        ConditionModelValue.CustomProperties?.ToString(),
                        (s, e) => {
                            if (s is StringParameter { Value: string pv })
                            {
                                // Handle CustomProperties as needed
                                AnnounceEdited();
                            }
                        },
                        prop.Name);
                    break;

                case nameof(ConditionModel.EndTime):

                    UseParameterGroupBox("End Time:").Content = new StringParameter(
                        ConditionModelValue.EndTime?.ToString(@"hh\:mm\:ss"),
                        (s, e) => {
                            if (s is StringParameter { Value: string pv })
                            {
                                if (TimeSpan.TryParse(pv, out var endTime))
                                {
                                    ConditionModelValue.EndTime = endTime;
                                }
                                else
                                {
                                    ConditionModelValue.EndTime = null;
                                }
                                AnnounceEdited();
                            }
                        },
                        prop.Name);
                    break;

                case nameof(ConditionModel.FilePath):

                    UseParameterGroupBox("Executable Path:").Content = new FilePathParameter(
                        ConditionModelValue.FilePath,
                        (s,e)=> {
                            if (s is FilePathParameter { Value: string pv })
                            {
                                ConditionModelValue.FilePath = string.IsNullOrWhiteSpace(pv) ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name);
                    break;

                case nameof(ConditionModel.IncludeChildProcesses):

                    UseParameterStackPanel("Process / App:").Children.Add(new BooleanParameter(
                        ConditionModelValue.IncludeChildProcesses,
                        (s,e)=> {
                            if (s is BooleanParameter { Value: bool pv })
                            {
                                ConditionModelValue.IncludeChildProcesses = pv is not true ? null : true;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Include child processes"));
                    break;

                case nameof(ConditionModel.IPAddress):

                    UseParameterStackPanel("Network:").Children.Add(new MultiIntParameter(
                        ConditionModelValue.IPAddress,
                        (s, e) => {
                            if (s is MultiIntParameter { Value: string pv })
                            {
                                ConditionModelValue.IPAddress = string.IsNullOrWhiteSpace(pv) ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "IP Address:"));
                    break;
                /*
                                case nameof(ConditionModel.IsNot):

                                    UseParameterGroupBox("Invert Condition:").Content = new BooleanParameter(
                                        ConditionModelValue.IsNot,
                                        (s, e) => {
                                            if (s is BooleanParameter { Value: bool pv })
                                            {
                                                ConditionModelValue.IsNot = pv;
                                                AnnounceEdited();
                                            }
                                        },
                                        prop.Name,
                                        null,
                                        "NOT condition");
                                    break;
                */
                case nameof(ConditionModel.MaxSystemUptimeMinutes):

                    UseParameterStackPanel("System Uptime:").Children.Add(new IntegerParameter(
                        ConditionModelValue.MaxSystemUptimeMinutes,
                        (s, e) => {
                            if (s is IntegerParameter { Value: int pv })
                            {
                                ConditionModelValue.MaxSystemUptimeMinutes = pv < 1 ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Max Minutes:"));
                    break;

                case nameof(ConditionModel.MinSystemUptimeMinutes):

                    UseParameterStackPanel("System Uptime:").Children.Add(new IntegerParameter(
                        ConditionModelValue.MinSystemUptimeMinutes,
                        (s, e) => {
                            if (s is IntegerParameter { Value: int pv })
                            {
                                ConditionModelValue.MinSystemUptimeMinutes = pv < 1 ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Min Minutes:"));
                    break;

                case nameof(ConditionModel.Port):

                    UseParameterStackPanel("Network:").Children.Add(new IntegerParameter(
                        ConditionModelValue.Port,
                        (s,e)=> {
                            if (s is IntegerParameter { Value: int pv })
                            {
                                ConditionModelValue.Port = pv < 1 ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Port:"));
                    break;

                case nameof(ConditionModel.ProcessName):

                    UseParameterStackPanel("Process / App:").Children.Add(new ProcessParameter(
                        ConditionModelValue.ProcessName,
                        (s,e) => {
                            if (s is ProcessParameter { Value: string pv })
                            {
                                ConditionModelValue.ProcessName = string.IsNullOrWhiteSpace(pv) ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Name:"));
                    break;

                case nameof(ConditionModel.StartTime):

                    UseParameterGroupBox("Start Time:").Content = new StringParameter(
                        ConditionModelValue.StartTime?.ToString(@"hh\:mm\:ss"),
                        (s, e) => {
                            if (s is StringParameter { Value: string pv })
                            {
                                if (TimeSpan.TryParse(pv, out var startTime))
                                {
                                    ConditionModelValue.StartTime = startTime;
                                }
                                else
                                {
                                    ConditionModelValue.StartTime = null;
                                }
                                AnnounceEdited();
                            }
                        },
                        prop.Name);
                    break;

                case nameof(ConditionModel.TimeoutMs):

                    UseParameterGroupBox("Timeout:").Content = new IntegerParameter(
                        ConditionModelValue.TimeoutMs,
                        (s, e) => {
                            if (s is IntegerParameter { Value: int pv })
                            {
                                ConditionModelValue.TimeoutMs = pv < 1 ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name,
                        null,
                        "Milliseconds:");
                    break;

                case nameof(ConditionModel.WindowTitle):

                    UseParameterGroupBox("Window Title:").Content = new StringParameter(
                        ConditionModelValue.WindowTitle,
                        (s, e) => {
                            if (s is StringParameter { Value: string pv })
                            {
                                ConditionModelValue.WindowTitle = string.IsNullOrWhiteSpace(pv) ? null : pv;
                                AnnounceEdited();
                            }
                        },
                        prop.Name );
                    break;
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

        private void UpdateAllowedDays()
        {
            var selectedDays = new List<DayOfWeek>();
            StackPanel panel = UseParameterStackPanel(AllowedDaysPanelName);

            foreach(object child in panel.Children)
            {
                    if (child is CheckBox cb && cb.Content is string dayStr && Enum.TryParse<DayOfWeek>(dayStr, out var day))
                    {
                        if (cb.IsChecked == true)
                        {
                            selectedDays.Add(day);
                        }
                }
            }

            ConditionModelValue.AllowedDays = 0 < selectedDays.Count ? selectedDays.ToArray() : null;
            
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e) 
        { 
            AnnounceSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AnnounceCancel();
        }

        protected void AnnounceEdited()
        {
            OnEdited?.Invoke(this, EventArgs.Empty);
        }

        protected void AnnounceCancel()
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
        }

        protected void AnnounceSave()
        {
            OnSave?.Invoke(this, new InputEditEventArgs(ConditionModelValue.Clone()));
        }
    }
}
