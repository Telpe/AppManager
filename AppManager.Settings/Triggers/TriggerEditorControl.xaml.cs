using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Core.Utils;
using AppManager.Settings.Actions;
using AppManager.Settings.Conditions;
using AppManager.Settings.UI;
using AppManager.Settings.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppManager.Settings.Triggers
{
    public partial class TriggerEditorControl : UserControl, IInputEditControl
    {
        private TriggerModel CurrentTriggerModelValue;
        private bool IsCapturingKeybindValue = false;
        private ObservableCollection<ConditionDisplayItem> _conditions = new ObservableCollection<ConditionDisplayItem>();

        public event EventHandler? Edited;

        public event EventHandler? Cancel;

        public event EventHandler<InputEditEventArgs>? Save;
        //public event EventHandler<TriggerModel>? TriggerSaved;
        //public event EventHandler? TriggerCancelled;

        private ObservableCollection<ModelListItem<ActionModel>> ActionListItemsValue = new();

        public TriggerModel CurrentTrigger
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

            InitializeComboBoxes();
            LoadTriggerData();
            UpdatePreview();
        }

        private void InitializeComboBoxes()
        {
            // Populate Trigger Type ComboBox
            TriggerTypeComboBox.ItemsSource = Enum.GetValues<TriggerTypeEnum>();
            TriggerTypeComboBox.SelectedIndex = 0;

            ActionsListBox.ItemsSource = ActionListItemsValue;
            ActionsListBox.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditActionButton_Click));
        }

        private void LoadTriggerData()
        {
            if (CurrentTriggerModelValue == null) return;

            TriggerTypeComboBox.SelectedItem = CurrentTriggerModelValue.TriggerType;
            TriggerNameTextBox.Text = GenerateTriggerName(CurrentTriggerModelValue.TriggerType);
            
            // Load shortcut data
            KeybindTextBox.Text = CurrentTriggerModelValue.KeybindCombination ?? string.Empty;
            CtrlModifierCheckBox.IsChecked = CurrentTriggerModelValue.Modifiers?.HasFlag(ModifierKeys.Control);
            ShiftModifierCheckBox.IsChecked = CurrentTriggerModelValue.Modifiers?.HasFlag(ModifierKeys.Shift);
            AltModifierCheckBox.IsChecked = CurrentTriggerModelValue.Modifiers?.HasFlag(ModifierKeys.Alt);
            WinModifierCheckBox.IsChecked = CurrentTriggerModelValue.Modifiers?.HasFlag(ModifierKeys.Windows);

            // Load app monitoring data
            ProcessNameTextBox.Text = CurrentTriggerModelValue.ProcessName ?? string.Empty;
            ExecutablePathMonitorTextBox.Text = CurrentTriggerModelValue.ExecutablePath ?? string.Empty;
            MonitorChildProcessesCheckBox.IsChecked = CurrentTriggerModelValue.MonitorChildProcesses;

            // Load network/system data
            IPAddressTextBox.Text = CurrentTriggerModelValue.IPAddress ?? "127.0.0.1";
            PortTextBox.Text = CurrentTriggerModelValue.Port.ToString();
            EventNameTextBox.Text = CurrentTriggerModelValue.EventName ?? string.Empty;
            EventSourceTextBox.Text = CurrentTriggerModelValue.EventSource ?? string.Empty;

            // Load timing data
            PollingIntervalTextBox.Text = CurrentTriggerModelValue.PollingIntervalMs.ToString();
            TriggerTimeoutTextBox.Text = CurrentTriggerModelValue.TimeoutMs.ToString();

            RefreshActionsListBox();

            UpdatePreview();
        }

        private string GenerateTriggerName(TriggerTypeEnum triggerType)
        {
            return $"{triggerType}Trigger_{DateTime.Now:HHmmss}";
        }


        private Key ParseKey(string shortcutText)
        {
            if (string.IsNullOrEmpty(shortcutText)) return Key.None;
            
            var parts = shortcutText.Split('+');
            var keyPart = parts.LastOrDefault()?.Trim();
            
            if (Enum.TryParse<Key>(keyPart, true, out Key key))
                return key;
            
            return Key.None;
        }

        private ModifierKeys GetModifierKeys()
        {
            ModifierKeys modifiers = ModifierKeys.None;
            
            if (CtrlModifierCheckBox.IsChecked == true)
                modifiers |= ModifierKeys.Control;
            if (ShiftModifierCheckBox.IsChecked == true)
                modifiers |= ModifierKeys.Shift;
            if (AltModifierCheckBox.IsChecked == true)
                modifiers |= ModifierKeys.Alt;
            if (WinModifierCheckBox.IsChecked == true)
                modifiers |= ModifierKeys.Windows;
            
            return modifiers;
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

                TriggerPreviewTextBlock.Text = previewBuilder.ToString();
            }
            catch (Exception ex)
            {
                TriggerPreviewTextBlock.Text = $"Error generating preview: {ex.Message}";
            }
        }

        private void TriggerTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TriggerTypeComboBox.SelectedItem is TriggerTypeEnum triggerType)
            {
                TriggerNameTextBox.Text = GenerateTriggerName(triggerType);
                
                // Show/hide relevant configuration groups
                KeybindConfigGroup.Visibility = triggerType == TriggerTypeEnum.Keybind ? Visibility.Visible : Visibility.Collapsed;
                
                AppMonitoringGroup.Visibility = (triggerType == TriggerTypeEnum.AppLaunch || triggerType == TriggerTypeEnum.AppClose) 
                    ? Visibility.Visible : Visibility.Collapsed;
                
                NetworkSystemGroup.Visibility = (triggerType == TriggerTypeEnum.NetworkPort || triggerType == TriggerTypeEnum.SystemEvent) 
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            UpdatePreview();
        }

        private void CaptureKeybindButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCapturingKeybindValue)
            {
                IsCapturingKeybindValue = true;
                KeybindCaptureButton.Content = "Press key combination...";
                KeybindTextBox.Text = "Press key combination...";
                //KeybindTextBox.Focus();

                // Add key event handler to capture keys
                KeybindTextBox.KeyDown += OnKeyDown;
                KeybindTextBox.Focus();
            }
            else
            {
                StopCapturing();
            }
        }

        private void StopCapturing()
        {
            IsCapturingKeybindValue = false;

            KeybindCaptureButton.Content = "Capture";

            // Remove key event handler
            KeybindTextBox.KeyDown -= OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (IsCapturingKeybindValue)
            {
                var modifiers = Keyboard.Modifiers;
                var key = e.Key == Key.System ? e.SystemKey : e.Key;
                
                // Ignore modifier keys alone
                if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                var keybindText = BuildKeybindText(modifiers, key);
                KeybindTextBox.Text = keybindText;

                // Update CurrentTriggerValue
                CurrentTriggerModelValue.KeybindCombination = keybindText;
                CurrentTriggerModelValue.Key = key;
                CurrentTriggerModelValue.Modifiers = modifiers;

                // Update checkboxes
                CtrlModifierCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Control);
                ShiftModifierCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Shift);
                AltModifierCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Alt);
                WinModifierCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Windows);
                
                StopCapturing();
                UpdatePreview();
                e.Handled = true;
            }
            
            base.OnKeyDown(e);
        }

        private string BuildKeybindText(ModifierKeys modifiers, Key key)
        {
            var parts = new System.Collections.Generic.List<string>();
            
            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");
            
            parts.Add(key.ToString());
            
            return string.Join(" + ", parts);
        }

        private void BrowseExecutableMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            // Use centralized FileManager for file dialog
            string selectedFile = FileManager.ShowOpenFileDialog(
                "Executable files (*.exe)|*.exe|All files (*.*)|*.*", 
                "Select Executable to Monitor");

            if (!string.IsNullOrEmpty(selectedFile))
            {
                ExecutablePathMonitorTextBox.Text = selectedFile;
                
                // Auto-populate process name if empty
                if (string.IsNullOrEmpty(ProcessNameTextBox.Text))
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(selectedFile);
                    ProcessNameTextBox.Text = fileName;
                }
                
                UpdatePreview();
            }
        }

        private void TestTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var triggerInstance = TriggerManager.CreateTrigger(CurrentTriggerModelValue);
                var canStart = triggerInstance.CanStart();
                
                var result = canStart ? "✓ Trigger configuration is valid" : "✗ Trigger configuration is invalid";
                MessageBox.Show(result, "Test Result", MessageBoxButton.OK, 
                    canStart ? MessageBoxImage.Information : MessageBoxImage.Warning);
                
                triggerInstance.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test failed: {ex.Message}", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTriggerButton_Click(object sender, RoutedEventArgs e) => DoSave();

        private void CancelTriggerButton_Click(object sender, RoutedEventArgs e) => DoCancel();

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

            Save?.Invoke(this, new InputEditEventArgs(CurrentTriggerModelValue));
        }

        // Event handlers for text changes to update preview
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) 
        {
            AnnounceEdited();
            UpdatePreview();
        }
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            AnnounceEdited();
            UpdatePreview();
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConditionTypeComboBox?.SelectedItem is ConditionTypeEnum conditionType)
                {
                    var conditionModel = new ConditionModel { ConditionType = conditionType };

                    var conditionDialog = new ConditionConfigDialog(conditionModel);
                    if (conditionDialog.ShowDialog() == true)
                    {
                        _conditions.Add(new ConditionDisplayItem(conditionDialog.ConditionModel));
                        AnnounceEdited();
                        UpdatePreview();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ConditionDisplayItem condition)
                {
                    _conditions.Remove(condition);
                    AnnounceEdited();
                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"RemoveConditionButton_Click error: {ex.Message}");
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            var newAction = new ActionModel
            {
                AppName = ProfileManager.CurrentProfile.SelectedNav1List, // TODO: Consider purpose.
                ActionType = AppActionTypeEnum.Launch // Default action
            };

            CurrentTriggerModelValue.Actions ??= [];
            CurrentTriggerModelValue.Actions = CurrentTriggerModelValue.Actions.Append(newAction).ToArray();

            RefreshActionsListBox();
            AnnounceEdited();
        }

        private void EditActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is ModelListItem<ActionModel> actionModelListItem)
            {
                Log.WriteLine($"Edit action: {actionModelListItem.DisplayName}");

                try
                {
                    // Create action editor control directly
                    var actionEditor = new ActionEditorControl(actionModelListItem.Model.Clone());

                    // Subscribe to save event
                    actionEditor.Save += (s, updatedAction) =>
                    {
                        if (null == updatedAction.ActionModel) { return; }

                        // Update the model in the dictionary
                        if (null != CurrentTriggerModelValue.Actions && actionModelListItem.Id < CurrentTriggerModelValue.Actions.Length)
                        {
                            CurrentTriggerModelValue.Actions[actionModelListItem.Id] = updatedAction.ActionModel;

                            // Refresh the UI to show changes
                            RefreshActionsListBox();

                            // Mark as edited
                            AnnounceEdited();

                            Log.WriteLine($"Action {actionModelListItem.DisplayName} updated successfully");
                            ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                        }
                        
                    };

                    actionEditor.Edited += (s, args) =>
                    {
                        Log.WriteLine($"Action edited for {actionModelListItem.DisplayName}");
                        AnnounceEdited();
                    };

                    actionEditor.Cancel += (s, args) =>
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
            //ActionsListBox.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(EditActionButton_Click));
            ActionListItemsValue.Clear();
        }

    }
}