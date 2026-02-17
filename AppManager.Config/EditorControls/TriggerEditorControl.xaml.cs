using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Config.Interfaces;
using AppManager.Config.ParameterControls;
using AppManager.Config.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppManager.Core.Conditions;

namespace AppManager.Config.EditorControls
{
    public partial class TriggerEditorControl : UserControl, IInputEditControl
    {
        private TriggerModel CurrentTriggerModelValue;

        public event TrueEventHandler? OnEdited;

        public event TrueEventHandler? OnCancel;

        public event TrueEventHandler<InputEditEventArgs>? OnSave;

        private ObservableCollection<ModelListItem<ActionModel>> ActionListItemsValue = new();

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

                ActionsListBox.ItemsSource = ActionListItemsValue;
                ActionsListBox.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditActionButton_Click));

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
                // Clear existing parameters
                TriggerParameters.Children.Clear();

                if(CurrentTriggerModelValue.Tags is not null) { TriggerTags.Value = CurrentTriggerModelValue.Tags; }
                TriggerTags.ValueName = nameof(TriggerModel.Tags);
                TriggerTags.PropertyChanged += ParameterChanged;

                // Load conditions
                TriggerConditions.Content = new ConditionsParameter(CurrentTriggerModelValue.Conditions ?? []);
                (TriggerConditions.Content as ConditionsParameter)!.PropertyChanged += ParameterChanged;

                // Generate trigger name
                TriggerNameTextBox.Text = GenerateTriggerName(CurrentTriggerModelValue.TriggerType);

                // Add parameters based on trigger type
                if (TypeSelectionGroupBox!.Content is TypeSelectParameter { Selected: TriggerTypeEnum triggerType })
                {
                    AddParametersForTriggerType(triggerType);
                }

                RefreshActionsListBox();
                UpdatePreview();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"LoadTriggerData error: {ex.Message}");
            }
        }

        private void AddParametersForTriggerType(TriggerTypeEnum triggerType)
        {
            switch (triggerType)
            {
                case TriggerTypeEnum.Keybind:
                    AddKeybindParameters();
                    break;
                case TriggerTypeEnum.AppLaunch:
                case TriggerTypeEnum.AppClose:
                    AddAppMonitoringParameters();
                    break;
                case TriggerTypeEnum.NetworkPort:
                    AddNetworkParameters();
                    break;
                case TriggerTypeEnum.SystemEvent:
                    AddSystemEventParameters();
                    break;
            }

            // Always add timing parameters
            AddTimingParameters();
        }

        private void AddKeybindParameters()
        {
            UseParameterGroupBox("Keybind Configuration").Content = new KeybindCaptureParameter(
                CurrentTriggerModel.KeybindCombination,
                CurrentTriggerModel.Key,
                CurrentTriggerModel.Modifiers,
                ParameterChanged,
                nameof(TriggerModel.KeybindCombination));
        }

        private void AddAppMonitoringParameters()
        {
            UseParameterGroupBox("Process Name:").Content = new ProcessParameter(
                CurrentTriggerModel.ProcessName,
                ParameterChanged,
                nameof(TriggerModel.ProcessName));

            UseParameterGroupBox("Executable Path:").Content = new FilePathParameter(
                CurrentTriggerModel.ExecutablePath,
                ParameterChanged,
                nameof(TriggerModel.ExecutablePath));

            UseParameterStackPanel("App Monitoring:").Children.Add(new BooleanParameter(
                CurrentTriggerModel.MonitorChildProcesses ?? false,
                ParameterChanged,
                nameof(TriggerModel.MonitorChildProcesses),
                "App Monitoring:",
                "Monitor Child Processes:"));
        }

        private void AddNetworkParameters()
        {
            UseParameterGroupBox("IP Address:").Content = new StringParameter(
                CurrentTriggerModel.IPAddress ?? "127.0.0.1",
                ParameterChanged,
                nameof(TriggerModel.IPAddress));

            UseParameterGroupBox("Port:").Content = new IntegerParameter(
                CurrentTriggerModel.Port ?? 80,
                ParameterChanged,
                nameof(TriggerModel.Port));
        }

        private void AddSystemEventParameters()
        {
            UseParameterGroupBox("Event Name:").Content = new StringParameter(
                CurrentTriggerModel.EventName,
                ParameterChanged,
                nameof(TriggerModel.EventName));

            UseParameterGroupBox("Event Source:").Content = new StringParameter(
                CurrentTriggerModel.EventSource,
                ParameterChanged,
                nameof(TriggerModel.EventSource));
        }

        private void AddTimingParameters()
        {
            UseParameterGroupBox("Polling Interval (ms):").Content = new TimerParameter(
                CurrentTriggerModel.PollingIntervalMs ?? 1000,
                ParameterChanged,
                nameof(TriggerModel.PollingIntervalMs));

            UseParameterGroupBox("Timeout (ms):").Content = new TimerParameter(
                CurrentTriggerModel.TimeoutMs ?? 30000,
                ParameterChanged,
                nameof(TriggerModel.TimeoutMs));
        }

        private GroupBox UseParameterGroupBox(string header)
        {
            GroupBox? box = null;

            foreach (object aBox in TriggerParameters.Children)
            {
                if (aBox is GroupBox gb && gb.Header is string sh && sh == header)
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

        private StackPanel UseParameterStackPanel(string header)
        {
            GroupBox? groupBox = null;

            foreach (object aBox in TriggerParameters.Children)
            {
                if (aBox is GroupBox gb && gb.Header is string sh && sh == header)
                {
                    groupBox = gb;
                    break;
                }
            }

            if (groupBox is null)
            {
                groupBox = new GroupBox
                {
                    Header = header
                };

                TriggerParameters.Children.Add(groupBox);
            }

            if (groupBox.Content is not StackPanel stackPanel)
            {
                stackPanel = new StackPanel();
                groupBox.Content = stackPanel;
            }

            return stackPanel;
        }

        private void ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is BaseParameterControl parameter && !string.IsNullOrEmpty(e.PropertyName))
                {
                    var property = typeof(TriggerModel).GetProperty(parameter.ValueName);
                    if (property != null)
                    {
                        var parameterProperty = parameter.GetType().GetProperty(e.PropertyName);
                        if (parameterProperty != null)
                        {
                            var value = parameterProperty.GetValue(parameter);
                            
                            // Handle special cases for keybind parameters
                            if (parameter is KeybindCaptureParameter keybindParam)
                            {
                                CurrentTriggerModel.KeybindCombination = keybindParam.KeybindCombination;
                                CurrentTriggerModel.Key = keybindParam.ValueKey;
                                CurrentTriggerModel.Modifiers = keybindParam.ValueModifiers;
                            }
                            else
                            {
                                property.SetValue(CurrentTriggerModel, value);
                            }
                        }
                    }
                }

                UpdatePreview();
                AnnounceEdited();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error in ParameterChanged: {ex.Message}");
            }
        }

        private string GenerateTriggerName(TriggerTypeEnum triggerType)
        {
            return $"{triggerType}Trigger_{DateTime.Now:HHmmss}";
        }

        private void UpdatePreview()
        {
            try
            {
                StringBuilder previewBuilder = new();
                previewBuilder.AppendLine($"Trigger Type: {CurrentTriggerModelValue.TriggerType}");

                switch (CurrentTriggerModelValue.TriggerType)
                {
                    case TriggerTypeEnum.Keybind:
                        previewBuilder.AppendLine($"Keybind: {CurrentTriggerModelValue.KeybindCombination ?? "Not specified"}");
                        previewBuilder.AppendLine($"Modifiers: {CurrentTriggerModelValue.Modifiers}");
                        previewBuilder.AppendLine($"Key: {CurrentTriggerModelValue.Key}");
                        break;

                    case TriggerTypeEnum.AppLaunch:
                    case TriggerTypeEnum.AppClose:
                        previewBuilder.AppendLine($"Process Name: {CurrentTriggerModelValue.ProcessName ?? "Not specified"}");
                        previewBuilder.AppendLine($"Executable Path: {CurrentTriggerModelValue.ExecutablePath ?? "Not specified"}");
                        previewBuilder.AppendLine($"Monitor Child Processes: {CurrentTriggerModelValue.MonitorChildProcesses}");
                        break;

                    case TriggerTypeEnum.NetworkPort:
                        previewBuilder.AppendLine($"IP Address: {CurrentTriggerModelValue.IPAddress}");
                        previewBuilder.AppendLine($"Port: {CurrentTriggerModelValue.Port}");
                        break;

                    case TriggerTypeEnum.SystemEvent:
                        previewBuilder.AppendLine($"Event Name: {CurrentTriggerModelValue.EventName ?? "Not specified"}");
                        previewBuilder.AppendLine($"Event Source: {CurrentTriggerModelValue.EventSource ?? "Not specified"}");
                        break;
                }

                previewBuilder.AppendLine($"\nTiming Configuration:");
                previewBuilder.AppendLine($"Polling Interval: {CurrentTriggerModelValue.PollingIntervalMs}ms");
                previewBuilder.AppendLine($"Timeout: {CurrentTriggerModelValue.TimeoutMs}ms");

                if (CurrentTriggerModelValue.Conditions?.Length > 0)
                {
                    previewBuilder.AppendLine("\nConditions:");
                    foreach (var condition in CurrentTriggerModelValue.Conditions)
                    {
                        previewBuilder.AppendLine($"  - {condition.ConditionType}: {GetConditionDescription(condition)}");
                    }
                }

                if (CurrentTriggerModelValue.Actions?.Length > 0)
                {
                    previewBuilder.AppendLine($"\nActions: {CurrentTriggerModelValue.Actions.Length} configured");
                }

                TriggerPreviewTextBlock.Text = previewBuilder.ToString();
            }
            catch (Exception ex)
            {
                TriggerPreviewTextBlock.Text = $"Error generating preview: {ex.Message}";
            }
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

        private void TriggerTypeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeSelectParameter typeSelector && typeSelector.Selected is TriggerTypeEnum triggerType)
            {
                CurrentTriggerModel.TriggerType = triggerType;
                TriggerNameTextBox.Text = GenerateTriggerName(triggerType);
                LoadTriggerData();
                AnnounceEdited();
            }
        }

        private void TestTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            ITrigger? triggerInstance = null;

            try
            {
                triggerInstance = TriggerFactory.CreateTrigger(CurrentTriggerModelValue);

                if (!triggerInstance.CanStart())
                {
                    MessageBox.Show("✗ Trigger configuration is invalid", "Trigger validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    try
                    {
                        MessageBox.Show($"Actions executed {(triggerInstance.Execute() ? "with success" : "without complete success.")}.", "Actions Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during trigger's execution of actions:\n{ex.Message}", "Action Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation critically failed:\n{ex.Message}", "Trigger Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                triggerInstance?.Dispose();
            }
        }

        private void SaveTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            ITrigger? triggerInstance = null;
            MessageBoxResult doSave = MessageBoxResult.No;

            try
            {
                triggerInstance = TriggerFactory.CreateTrigger(CurrentTriggerModelValue);

                if (!triggerInstance.CanStart())
                {
                    doSave = MessageBox.Show("✗ Trigger configuration is invalid\n\nDo you still want to save?", "Trigger validation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                }
                else
                {
                    doSave = MessageBoxResult.Yes;
                    Log.WriteLine("✓ Trigger configuration is valid");
                }
            }
            catch (Exception ex)
            {
                doSave = MessageBox.Show($"Validation critically failed: {ex.Message}\n\nDo you still want to save?", "Trigger Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
            finally
            {
                triggerInstance?.Dispose();
            }

            if (MessageBoxResult.Yes == doSave)
            {
                AnnounceSave();
            }
        }

        private void CancelTriggerButton_Click(object sender, RoutedEventArgs e) => AnnounceCancel();

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
            OnSave?.Invoke(this, new InputEditEventArgs(CurrentTriggerModelValue));
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            var newAction = new ActionModel
            {
                AppName = ProfileManager.CurrentProfile.SelectedNav1List,
                ActionType = ActionTypeEnum.Launch
            };

            CurrentTriggerModelValue.Actions ??= [];
            CurrentTriggerModelValue.Actions = [..CurrentTriggerModelValue.Actions, newAction];

            RefreshActionsListBox();
            AnnounceEdited();
        }

        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is ModelListItem<ActionModel> actionModelListItem)
            {
                Log.WriteLine($"Edit action: {actionModelListItem.DisplayName}");

                try
                {
                    var actionEditor = new ActionEditorControl(actionModelListItem.Model.Clone());

                    actionEditor.OnSave += (s, updatedAction) =>
                    {
                        if (null == updatedAction.ActionModel)
                        {
                            return;
                        }

                        if (null != CurrentTriggerModelValue.Actions && actionModelListItem.Id < CurrentTriggerModelValue.Actions.Length)
                        {
                            CurrentTriggerModelValue.Actions[actionModelListItem.Id] = updatedAction.ActionModel;
                            RefreshActionsListBox();
                            AnnounceEdited();
                            Log.WriteLine($"Action {actionModelListItem.DisplayName} updated successfully");
                            ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                        }
                    };

                    actionEditor.OnEdited += (s, args) =>
                    {
                        Log.WriteLine($"Action edited for {actionModelListItem.DisplayName}");
                        AnnounceEdited();
                    };

                    actionEditor.OnCancel += (s, args) =>
                    {
                        Log.WriteLine($"Action editing cancelled for {actionModelListItem.DisplayName}");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    };

                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(actionEditor);
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Error opening action editor: {ex.Message}");
                    MessageBox.Show($"Error opening action editor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshActionsListBox()
        {
            ClearActions();
            CurrentTriggerModelValue.Actions ??= [];

            foreach (ActionModel action in CurrentTriggerModelValue.Actions)
            {
                ActionListItemsValue.Add(new ModelListItem<ActionModel>(CurrentTriggerModelValue.Actions.IndexOf(action), action));
            }
        }

        public void ClearActions()
        {
            ActionListItemsValue.Clear();
        }
    }
}