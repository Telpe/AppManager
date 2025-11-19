using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppManager.Triggers;
using AppManager.Utils;
using AppManager.UI;

namespace AppManager.AppUI
{
    public partial class TriggerEditorControl : OverlayContent
    {
        private TriggerModel _currentTrigger;
        private TriggerManager _triggerManager;
        private bool _isCapturingShortcut = false;

        public event EventHandler<TriggerModel> TriggerSaved;
        public event EventHandler TriggerCancelled;

        public TriggerModel CurrentTrigger
        {
            get => _currentTrigger;
            set
            {
                _currentTrigger = value;
                LoadTriggerData();
            }
        }

        public TriggerEditorControl()
        {
            InitializeComponent();
            _triggerManager = new TriggerManager();
            
            InitializeComboBoxes();
            UpdatePreview();
        }

        private void InitializeComboBoxes()
        {
            // Populate Trigger Type ComboBox
            TriggerTypeComboBox.ItemsSource = Enum.GetValues(typeof(TriggerTypeEnum)).Cast<TriggerTypeEnum>();
            TriggerTypeComboBox.SelectedIndex = 0;
        }

        private void LoadTriggerData()
        {
            if (_currentTrigger == null) return;

            TriggerTypeComboBox.SelectedItem = _currentTrigger.TriggerType;
            TriggerNameTextBox.Text = GenerateTriggerName(_currentTrigger.TriggerType);
            
            // Load shortcut data
            ShortcutTextBox.Text = _currentTrigger.ShortcutCombination ?? string.Empty;
            CtrlModifierCheckBox.IsChecked = _currentTrigger.Modifiers.HasFlag(ModifierKeys.Control);
            ShiftModifierCheckBox.IsChecked = _currentTrigger.Modifiers.HasFlag(ModifierKeys.Shift);
            AltModifierCheckBox.IsChecked = _currentTrigger.Modifiers.HasFlag(ModifierKeys.Alt);
            WinModifierCheckBox.IsChecked = _currentTrigger.Modifiers.HasFlag(ModifierKeys.Windows);

            // Load app monitoring data
            ProcessNameTextBox.Text = _currentTrigger.ProcessName ?? string.Empty;
            ExecutablePathMonitorTextBox.Text = _currentTrigger.ExecutablePath ?? string.Empty;
            MonitorChildProcessesCheckBox.IsChecked = _currentTrigger.MonitorChildProcesses;

            // Load network/system data
            IPAddressTextBox.Text = _currentTrigger.IPAddress ?? "127.0.0.1";
            PortTextBox.Text = _currentTrigger.Port.ToString();
            EventNameTextBox.Text = _currentTrigger.EventName ?? string.Empty;
            EventSourceTextBox.Text = _currentTrigger.EventSource ?? string.Empty;

            // Load timing data
            PollingIntervalTextBox.Text = _currentTrigger.PollingIntervalMs.ToString();
            TriggerTimeoutTextBox.Text = _currentTrigger.TimeoutMs.ToString();

            UpdatePreview();
        }

        private string GenerateTriggerName(TriggerTypeEnum triggerType)
        {
            return $"{triggerType}Trigger_{DateTime.Now:HHmmss}";
        }

        private TriggerModel CreateTriggerModel()
        {
            var trigger = new TriggerModel
            {
                TriggerType = (TriggerTypeEnum)(TriggerTypeComboBox.SelectedItem ?? TriggerTypeEnum.Shortcut),
                ShortcutCombination = ShortcutTextBox.Text?.Trim(),
                Key = ParseKey(ShortcutTextBox.Text),
                Modifiers = GetModifierKeys(),
                ProcessName = ProcessNameTextBox.Text?.Trim(),
                ExecutablePath = ExecutablePathMonitorTextBox.Text?.Trim(),
                MonitorChildProcesses = MonitorChildProcessesCheckBox.IsChecked ?? false,
                IPAddress = IPAddressTextBox.Text?.Trim() ?? "127.0.0.1",
                Port = int.TryParse(PortTextBox.Text, out int port) ? port : 0,
                EventName = EventNameTextBox.Text?.Trim(),
                EventSource = EventSourceTextBox.Text?.Trim(),
                PollingIntervalMs = int.TryParse(PollingIntervalTextBox.Text, out int polling) ? polling : 1000,
                TimeoutMs = int.TryParse(TriggerTimeoutTextBox.Text, out int timeout) ? timeout : 30000
            };

            return trigger;
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
                var trigger = CreateTriggerModel();
                var preview = $"Trigger Type: {trigger.TriggerType}\n";

                switch (trigger.TriggerType)
                {
                    case TriggerTypeEnum.Shortcut:
                        preview += $"Shortcut: {trigger.ShortcutCombination ?? "Not specified"}\n";
                        preview += $"Modifiers: {trigger.Modifiers}\n";
                        preview += $"Key: {trigger.Key}\n";
                        break;

                    case TriggerTypeEnum.AppLaunch:
                    case TriggerTypeEnum.AppClose:
                        preview += $"Process Name: {trigger.ProcessName ?? "Not specified"}\n";
                        preview += $"Executable Path: {trigger.ExecutablePath ?? "Not specified"}\n";
                        preview += $"Monitor Child Processes: {trigger.MonitorChildProcesses}\n";
                        break;

                    case TriggerTypeEnum.NetworkPort:
                        preview += $"IP Address: {trigger.IPAddress}\n";
                        preview += $"Port: {trigger.Port}\n";
                        break;

                    case TriggerTypeEnum.SystemEvent:
                        preview += $"Event Name: {trigger.EventName ?? "Not specified"}\n";
                        preview += $"Event Source: {trigger.EventSource ?? "Not specified"}\n";
                        break;
                }

                preview += $"\nTiming Configuration:\n";
                preview += $"Polling Interval: {trigger.PollingIntervalMs}ms\n";
                preview += $"Timeout: {trigger.TimeoutMs}ms\n";

                TriggerPreviewTextBlock.Text = preview;
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
                ShortcutConfigGroup.Visibility = triggerType == TriggerTypeEnum.Shortcut ? Visibility.Visible : Visibility.Collapsed;
                
                AppMonitoringGroup.Visibility = (triggerType == TriggerTypeEnum.AppLaunch || triggerType == TriggerTypeEnum.AppClose) 
                    ? Visibility.Visible : Visibility.Collapsed;
                
                NetworkSystemGroup.Visibility = (triggerType == TriggerTypeEnum.NetworkPort || triggerType == TriggerTypeEnum.SystemEvent) 
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            UpdatePreview();
        }

        private void CaptureShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCapturingShortcut)
            {
                _isCapturingShortcut = true;
                ((Button)sender).Content = "Press key combination...";
                ShortcutTextBox.Text = "Press key combination...";
                ShortcutTextBox.Focus();
            }
            else
            {
                StopCapturing();
            }
        }

        private void StopCapturing()
        {
            _isCapturingShortcut = false;
            var button = this.FindName("CaptureShortcutButton") as Button;
            if (button != null)
                button.Content = "Capture";
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_isCapturingShortcut)
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

                var shortcutText = BuildShortcutText(modifiers, key);
                ShortcutTextBox.Text = shortcutText;
                
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

        private string BuildShortcutText(ModifierKeys modifiers, Key key)
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
                var trigger = CreateTriggerModel();
                var triggerInstance = _triggerManager.CreateTrigger(trigger.TriggerType, TriggerNameTextBox.Text);
                var canStart = triggerInstance.CanStart(trigger);
                
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
                var trigger = CreateTriggerModel();
                _currentTrigger = trigger;
                TriggerSaved?.Invoke(this, trigger);
                DisableOverlay(); // Close the overlay after saving
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
    }
}