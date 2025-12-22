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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppManager.Settings.Triggers
{
    public partial class TriggerEditorControl : OverlayContent
    {
        private TriggerModel CurrentTriggerValue;
        private bool IsCapturingKeybindValue = false;
        private ObservableCollection<ConditionDisplayItem> _conditions = new ObservableCollection<ConditionDisplayItem>();

        public event EventHandler<TriggerModel>? TriggerSaved;
        public event EventHandler? TriggerCancelled;

        private ObservableCollection<ModelListItem<ActionModel>> ActionListItemsValue = new();

        public TriggerModel CurrentTrigger
        {
            get => CurrentTriggerValue;
            set
            {
                CurrentTriggerValue = value;
                LoadTriggerData();
            }
        }

        public TriggerEditorControl(TriggerModel triggerModel)
        {
            CurrentTriggerValue = triggerModel;

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
            if (CurrentTriggerValue == null) return;

            TriggerTypeComboBox.SelectedItem = CurrentTriggerValue.TriggerType;
            TriggerNameTextBox.Text = GenerateTriggerName(CurrentTriggerValue.TriggerType);
            
            // Load shortcut data
            KeybindTextBox.Text = CurrentTriggerValue.KeybindCombination ?? string.Empty;
            CtrlModifierCheckBox.IsChecked = CurrentTriggerValue.Modifiers?.HasFlag(ModifierKeys.Control);
            ShiftModifierCheckBox.IsChecked = CurrentTriggerValue.Modifiers?.HasFlag(ModifierKeys.Shift);
            AltModifierCheckBox.IsChecked = CurrentTriggerValue.Modifiers?.HasFlag(ModifierKeys.Alt);
            WinModifierCheckBox.IsChecked = CurrentTriggerValue.Modifiers?.HasFlag(ModifierKeys.Windows);

            // Load app monitoring data
            ProcessNameTextBox.Text = CurrentTriggerValue.ProcessName ?? string.Empty;
            ExecutablePathMonitorTextBox.Text = CurrentTriggerValue.ExecutablePath ?? string.Empty;
            MonitorChildProcessesCheckBox.IsChecked = CurrentTriggerValue.MonitorChildProcesses;

            // Load network/system data
            IPAddressTextBox.Text = CurrentTriggerValue.IPAddress ?? "127.0.0.1";
            PortTextBox.Text = CurrentTriggerValue.Port.ToString();
            EventNameTextBox.Text = CurrentTriggerValue.EventName ?? string.Empty;
            EventSourceTextBox.Text = CurrentTriggerValue.EventSource ?? string.Empty;

            // Load timing data
            PollingIntervalTextBox.Text = CurrentTriggerValue.PollingIntervalMs.ToString();
            TriggerTimeoutTextBox.Text = CurrentTriggerValue.TimeoutMs.ToString();

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
                previewBuilder.AppendLine($"Trigger Type: {CurrentTriggerValue.TriggerType}");

                switch (CurrentTriggerValue.TriggerType)
                {
                    case TriggerTypeEnum.Keybind:
                        previewBuilder.AppendLine($"Keybind: {CurrentTriggerValue.KeybindCombination ?? "Not specified"}");
                        previewBuilder.AppendLine($"Modifiers: {CurrentTriggerValue.Modifiers}");
                        previewBuilder.AppendLine($"Key: {CurrentTriggerValue.Key}");
                        break;

                    case TriggerTypeEnum.AppLaunch:
                    case TriggerTypeEnum.AppClose:
                        previewBuilder.AppendLine($"Process Name: {CurrentTriggerValue.ProcessName ?? "Not specified"}");
                        previewBuilder.AppendLine($"Executable Path: {CurrentTriggerValue.ExecutablePath ?? "Not specified"}");
                        previewBuilder.AppendLine($"Monitor Child Processes: {CurrentTriggerValue.MonitorChildProcesses}");
                        break;

                    case TriggerTypeEnum.NetworkPort:
                        previewBuilder.AppendLine($"IP Address: {CurrentTriggerValue.IPAddress}");
                        previewBuilder.AppendLine($"Port: {CurrentTriggerValue.Port}");
                        break;

                    case TriggerTypeEnum.SystemEvent:
                        previewBuilder.AppendLine($"Event Name: {CurrentTriggerValue.EventName ?? "Not specified"}");
                        previewBuilder.AppendLine($"Event Source: {CurrentTriggerValue.EventSource ?? "Not specified"}");
                        break;
                }

                previewBuilder.AppendLine($"\nTiming Configuration:");
                previewBuilder.AppendLine($"Polling Interval: {CurrentTriggerValue.PollingIntervalMs}ms");
                previewBuilder.AppendLine($"Timeout: {CurrentTriggerValue.TimeoutMs}ms");

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
                CurrentTriggerValue.KeybindCombination = keybindText;
                CurrentTriggerValue.Key = key;
                CurrentTriggerValue.Modifiers = modifiers;

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
                var triggerInstance = TriggerManager.CreateTrigger(CurrentTriggerValue);
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

        private void SaveTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TriggerSaved?.Invoke(this, CurrentTriggerValue.Clone());
                DisableOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void CancelTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerCancelled?.Invoke(this, EventArgs.Empty);
            DisableOverlay(); // Close the overlay when cancelling
        }

        // Event handlers for text changes to update preview
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdatePreview();
        private void CheckBox_Changed(object sender, RoutedEventArgs e) => UpdatePreview();

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
                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveConditionButton_Click error: {ex.Message}");
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            var newAction = new ActionModel
            {
                AppName = ProfileManager.CurrentProfile.SelectedNav1List, // TODO: Consider purpose.
                ActionType = AppActionTypeEnum.Launch // Default action
            };

            CurrentTriggerValue.Actions ??= [];
            CurrentTriggerValue.Actions.Add(0 < CurrentTriggerValue.Actions.Count ? CurrentTriggerValue.Actions.Keys.Max() + 1 : 1, newAction);

            RefreshActionsListBox();
            //Edited();
        }

        private void EditActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is ModelListItem<ActionModel> viewModel)
            {
                Debug.WriteLine($"Edit action: {viewModel.DisplayName}");

                try
                {
                    // Create action editor control directly
                    var actionEditor = new ActionEditorControl(viewModel.Model.Clone());

                    // Subscribe to save event
                    actionEditor.ActionSaved += (s, updatedAction) =>
                    {
                        // Update the model in the dictionary
                        CurrentTriggerValue.Actions?[viewModel.Id] = updatedAction;

                        // Refresh the UI to show changes
                        RefreshActionsListBox();

                        // Mark as edited
                        //Edited();

                        Debug.WriteLine($"Action {viewModel.DisplayName} updated successfully");
                    };

                    actionEditor.ActionCancelled += (s, args) =>
                    {
                        Debug.WriteLine($"Action editing cancelled for {viewModel.DisplayName}");
                    };

                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(actionEditor, 80, 70, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening action editor: {ex.Message}");
                    MessageBox.Show($"Error opening action editor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshActionsListBox()
        {
            ClearActions();
            foreach (var kvp in CurrentTriggerValue.Actions??[])
            {
                ActionListItemsValue.Add(new ModelListItem<ActionModel>(kvp.Key, kvp.Value));
            }
        }

        public void ClearActions()
        {
            //ActionsListBox.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(EditActionButton_Click));
            ActionListItemsValue.Clear();
        }

    }
}