using AppManager.Config.Interfaces;
using AppManager.Config.ParameterControls;
using AppManager.Config.Utilities;
using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppManager.Config.EditorControls
{
    public partial class TriggerEditorControl : UserControl, IInputEditControl
    {
        private TriggerModel CurrentTriggerModelValue;

        public event TrueEventHandler? OnEdited;

        public event TrueEventHandler? OnCancel;

        public event TrueEventHandler<InputEditEventArgs>? OnSave;

        public TriggerModel CurrentTriggerModel
        {
            get => CurrentTriggerModelValue;
            set
            {
                CurrentTriggerModelValue = value;
                LoadTriggerData();
            }
        }

        public TriggerEditorControl(TriggerModel triggerModel)
        {
            CurrentTriggerModelValue = triggerModel;

            InitializeComponent();
            this.DataContext = this;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Initialize trigger type selector using TypeSelectParameter
                TypeSelectionGroupBox!.Content = new TypeSelectParameter(typeof(TriggerTypeEnum), null, null, "Choose:");
                (TypeSelectionGroupBox!.Content as TypeSelectParameter)!.PropertyChanged += TriggerTypeChanged;
                (TypeSelectionGroupBox!.Content as TypeSelectParameter)!.Selected = CurrentTriggerModelValue.TriggerType;

                LoadTriggerData();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"TriggerEditorControl initialization error: {ex.Message}");
                InitializeManually();
            }
        }

        private void InitializeManually()
        {
            var grid = new Grid();
            var textBlock = new TextBlock
            {
                Text = "TriggerEditorControl - XAML Loading Failed",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(textBlock);
            Content = grid;
        }

        private void LoadTriggerData()
        {
            try
            {
                TriggerParameters.Children.Clear();

                TriggerTags.Content = new TagsParameter(CurrentTriggerModelValue.Tags, ParameterChanged);

                TriggerConditions.Content = new ConditionsParameter(CurrentTriggerModelValue.Conditions, ParameterChanged);

                TriggerActions.Content = new ActionsParameter(CurrentTriggerModelValue.Actions, ParameterChanged);

                if (TypeSelectionGroupBox!.Content is TypeSelectParameter { Selected: TriggerTypeEnum triggerType })
                {
                    switch (triggerType)
                    {
                        case TriggerTypeEnum.Keybind:
                            AddParametersFromInterface(typeof(IKeybindTrigger));
                            break;
                        case TriggerTypeEnum.AppLaunch:
                            AddParametersFromInterface(typeof(IAppLaunchTrigger));
                            break;
                        case TriggerTypeEnum.AppClose:
                            AddParametersFromInterface(typeof(IAppCloseTrigger));
                            break;
                        case TriggerTypeEnum.SystemEvent:
                            AddParametersFromInterface(typeof(ISystemEventTrigger));
                            break;
                        case TriggerTypeEnum.NetworkPort:
                            AddParametersFromInterface(typeof(INetworkPortTrigger));
                            break;
                        case TriggerTypeEnum.Button:
                            AddParametersFromInterface(typeof(IButtonTrigger));
                            break;
                    }
                }

                UpdatePreview();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"{nameof(LoadTriggerData)} error: {ex.Message}");
            }
        }

        private void UpdatePreview()
        {
            try
            {
                if (PreviewTextBlock == null)
                { return; }

                var preview = $"Trigger Type: {CurrentTriggerModelValue.TriggerType}\n";

                switch (CurrentTriggerModelValue.TriggerType)
                {
                    case TriggerTypeEnum.Keybind:
                        preview += $"Keybind: {CurrentTriggerModelValue.KeybindCombination ?? "Not specified"}\n";
                        break;
                    case TriggerTypeEnum.AppLaunch:
                    case TriggerTypeEnum.AppClose:
                        preview += $"Process Name: {CurrentTriggerModelValue.ProcessName ?? "Not specified"}\n";
                        if (!string.IsNullOrEmpty(CurrentTriggerModelValue.ExecutablePath))
                        {
                            preview += $"Executable: {CurrentTriggerModelValue.ExecutablePath}\n";
                        }
                        preview += $"Monitor Child Processes: {CurrentTriggerModelValue.MonitorChildProcesses}\n";
                        break;
                    case TriggerTypeEnum.SystemEvent:
                        preview += $"Event Name: {CurrentTriggerModelValue.EventName ?? "Not specified"}\n";
                        preview += $"Event Source: {CurrentTriggerModelValue.EventSource ?? "Not specified"}\n";
                        break;
                    case TriggerTypeEnum.NetworkPort:
                        preview += $"Port: {CurrentTriggerModelValue.Port}\n";
                        preview += $"IP Address: {CurrentTriggerModelValue.IPAddress ?? "Any"}\n";
                        break;
                }

                if (CurrentTriggerModelValue.PollingIntervalMs.HasValue)
                {
                    preview += $"Polling Interval: {CurrentTriggerModelValue.PollingIntervalMs}ms\n";
                }

                if (CurrentTriggerModelValue.TimeoutMs.HasValue)
                {
                    preview += $"Timeout: {CurrentTriggerModelValue.TimeoutMs}ms\n";
                }

                preview += "\n";

                if (CurrentTriggerModelValue.Actions?.Length > 0)
                {
                    preview += "Actions:\n";
                    foreach (var action in CurrentTriggerModelValue.Actions)
                    {
                        preview += $"  - {action.ActionType}: {GetActionDescription(action)}\n";
                    }
                }
                else
                {
                    preview += "No actions specified\n";
                }

                preview += "\n";

                if (CurrentTriggerModelValue.Conditions?.Length > 0)
                {
                    preview += "Conditions:\n";
                    foreach (var condition in CurrentTriggerModelValue.Conditions)
                    {
                        preview += $"  - {condition.ConditionType}: {GetConditionDescription(condition)}\n";
                    }
                }
                else
                {
                    preview += "No conditions specified";
                }

                PreviewTextBlock.Text = preview;
            }
            catch (Exception ex)
            {
                if (PreviewTextBlock != null)
                {
                    PreviewTextBlock.Text = $"Error generating preview: {ex.Message}";
                }
            }
        }

        private string GetActionDescription(ActionModel action)
        {
            return action.ActionType switch
            {
                ActionTypeEnum.Launch => $"Launch '{action.AppName ?? action.ExecutablePath ?? "Unknown"}'",
                ActionTypeEnum.Close => $"Close '{action.AppName ?? "Unknown"}'",
                ActionTypeEnum.Focus => $"Focus '{action.AppName ?? "Unknown"}'",
                _ => "Unknown action"
            };
        }

        private string GetConditionDescription(ConditionModel condition)
        {
            return condition.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process '{condition.ProcessName}' is running",
                ConditionTypeEnum.FileExists => $"File '{condition.FilePath}' exists",
                _ => "Unknown condition"
            };
        }

        private void AddParametersFromInterface(Type modelType)
        {
            foreach (PropertyInfo prop in modelType.GetProperties())
            {
                switch (prop.Name)
                {
                    case nameof(TriggerModel.ProcessName):
                        UseParameterGroupBox("Process Name:").Content = new ProcessParameter(
                            CurrentTriggerModelValue.ProcessName,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.ExecutablePath):
                        UseParameterGroupBox("Executable Path:").Content = new FilePathParameter(
                            CurrentTriggerModelValue.ExecutablePath,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.KeybindCombination):
                        UseParameterGroupBox("Keybind:").Content = new KeybindCaptureParameter(
                            CurrentTriggerModelValue.KeybindCombination,
                            null,null,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.EventName):
                        UseParameterGroupBox("Event Name:").Content = new StringParameter(
                            CurrentTriggerModelValue.EventName,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.EventSource):
                        UseParameterGroupBox("Event Source:").Content = new StringParameter(
                            CurrentTriggerModelValue.EventSource,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.Port):
                        UseParameterGroupBox("Port:").Content = new IntegerParameter(
                            CurrentTriggerModelValue.Port,
                            ParameterChanged,
                            prop.Name,
                            null,null,
                            0, 65535);
                        break;

                    case nameof(TriggerModel.IPAddress):
                        UseParameterGroupBox("IP Address:").Content = new StringParameter(
                            CurrentTriggerModelValue.IPAddress,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.MonitorChildProcesses):
                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentTriggerModelValue.MonitorChildProcesses,
                            ParameterChanged,
                            prop.Name,
                            "Advanced:",
                            "Monitor Child Processes:"));
                        break;

                    case nameof(TriggerModel.PollingIntervalMs):
                        UseParameterGroupBox("Polling Interval (ms):").Content = new TimerParameter(
                            CurrentTriggerModelValue.PollingIntervalMs,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.TimeoutMs):
                        UseParameterGroupBox("Timeout (ms):").Content = new TimerParameter(
                            CurrentTriggerModelValue.TimeoutMs,
                            ParameterChanged,
                            prop.Name);
                        break;

                    case nameof(TriggerModel.Tags):
                        UseParameterGroupBox("Tags:").Content = new TagsParameter(
                            CurrentTriggerModelValue.Tags,
                            ParameterChanged,
                            prop.Name);
                        break;
                }
            }
        }

        private GroupBox UseParameterGroupBox(string header)
        {
            GroupBox? box = null;

            foreach (object aBox in TriggerParameters.Children)
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

                TriggerParameters.Children.Add(box);
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
            OnSave?.Invoke(this, new InputEditEventArgs(CurrentTriggerModelValue.Clone()));
        }

        private void TestTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Trigger test functionality not yet implemented", "Test Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test failed: {ex.Message}", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = new List<string>();

                // Add trigger-specific validation logic here
                if (CurrentTriggerModelValue.TriggerType == TriggerTypeEnum.Keybind && string.IsNullOrEmpty(CurrentTriggerModelValue.KeybindCombination))
                {
                    errors.Add("Keybind combination is required for keybind triggers");
                }

                if ((CurrentTriggerModelValue.TriggerType == TriggerTypeEnum.AppLaunch || CurrentTriggerModelValue.TriggerType == TriggerTypeEnum.AppClose) 
                    && string.IsNullOrEmpty(CurrentTriggerModelValue.ProcessName) && string.IsNullOrEmpty(CurrentTriggerModelValue.ExecutablePath))
                {
                    errors.Add("Process name or executable path is required for app triggers");
                }

                var message = errors.Any() ?
                    $"Validation failed:\n{string.Join("\n", errors)}" :
                    "✓ Validation passed";

                var icon = errors.Any() ? MessageBoxImage.Warning : MessageBoxImage.Information;
                MessageBox.Show(message, "Validation Result", MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation failed: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AnnounceSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AnnounceCancel();
        }

        private void TriggerTypeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeSelectParameter tsp && tsp.Selected is TriggerTypeEnum typeEnum)
            {
                CurrentTriggerModelValue.TriggerType = typeEnum;

                LoadTriggerData();

                UpdatePreview();
                AnnounceEdited();

                return;
            }

            throw new InvalidOperationException($"{nameof(TriggerTypeChanged)}: {nameof(sender)} is not {nameof(TypeSelectParameter)} or {nameof(TypeSelectParameter.Selected)} is not {nameof(TriggerTypeEnum)}");
        }

        private void ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not BaseParameterControl) 
            { 
                throw new InvalidOperationException($"{nameof(ParameterChanged)}: {nameof(sender)} is not {nameof(BaseParameterControl)}"); 
            }

            switch (e.PropertyName)
            {
                case nameof(TriggerModel.TriggerType):
                    TriggerTypeChanged(sender, e);
                    break;

                case nameof(TriggerModel.ProcessName):
                    CurrentTriggerModelValue.ProcessName = (sender as ProcessParameter)!.Value;
                    break;

                case nameof(TriggerModel.ExecutablePath):
                    CurrentTriggerModelValue.ExecutablePath = (sender as FilePathParameter)!.Value;
                    break;

                case nameof(TriggerModel.KeybindCombination):
                    CurrentTriggerModelValue.KeybindCombination = (sender as KeybindCaptureParameter)!.KeybindCombination;
                    break;

                case nameof(TriggerModel.EventName):
                    CurrentTriggerModelValue.EventName = (sender as StringParameter)!.Value;
                    break;

                case nameof(TriggerModel.EventSource):
                    CurrentTriggerModelValue.EventSource = (sender as StringParameter)!.Value;
                    break;

                case nameof(TriggerModel.Port):
                    CurrentTriggerModelValue.Port = (sender as IntegerParameter)!.Value;
                    break;

                case nameof(TriggerModel.IPAddress):
                    CurrentTriggerModelValue.IPAddress = (sender as StringParameter)!.Value;
                    break;

                case nameof(TriggerModel.MonitorChildProcesses):
                    CurrentTriggerModelValue.MonitorChildProcesses = (sender as BooleanParameter)!.Value;
                    break;

                case nameof(TriggerModel.PollingIntervalMs):
                    CurrentTriggerModelValue.PollingIntervalMs = (sender as TimerParameter)!.Value;
                    break;

                case nameof(TriggerModel.TimeoutMs):
                    CurrentTriggerModelValue.TimeoutMs = (sender as TimerParameter)!.Value;
                    break;

                case nameof(TriggerModel.Tags):
                    CurrentTriggerModelValue.Tags = (sender as TagsParameter)!.Value;
                    break;

                case nameof(TriggerModel.Conditions):
                    CurrentTriggerModelValue.Conditions = (sender as ConditionsParameter)!.Value;
                    break;

                case nameof(TriggerModel.Actions):
                    CurrentTriggerModelValue.Actions = (sender as ActionsParameter)!.Value;
                    break;
            }

            UpdatePreview();
            AnnounceEdited();
        }
    }
}