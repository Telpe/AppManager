using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppManager.Config.ParameterControls
{
    public partial class KeybindCaptureParameter : BaseParameterControl
    {
        private string _keybindCombination = string.Empty;
        private Key? _key;
        private ModifierKeys? _modifiers;
        private bool _isCapturing = false;

        public string KeybindCombination
        {
            get => _keybindCombination;
            set
            {
                if (_keybindCombination != value)
                {
                    _keybindCombination = value;
                    KeybindTextBox.Text = value;
                    AnnouncePropertyChanged(nameof(KeybindCombination));
                }
            }
        }

        public Key? ValueKey
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    AnnouncePropertyChanged(nameof(ValueKey));
                }
            }
        }

        public ModifierKeys? ValueModifiers
        {
            get => _modifiers;
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    AnnouncePropertyChanged(nameof(ValueModifiers));
                }
            }
        }

        public KeybindCaptureParameter(string? keybindCombination = null, Key? key = null, ModifierKeys? modifiers = null, PropertyChangedEventHandler? onPropertyChanged = null, string valueName = "KeybindCombination")
        {
            InitializeComponent();
            
            ValueName = valueName;
            KeybindCombination = keybindCombination ?? string.Empty;
            ValueKey = key;
            ValueModifiers = modifiers;

            if (onPropertyChanged != null)
            {
                PropertyChanged += onPropertyChanged;
            }
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCapturing)
            {
                StartCapturing();
            }
            else
            {
                StopCapturing();
            }
        }

        private void StartCapturing()
        {
            _isCapturing = true;
            CaptureButton.Content = "Press key combination...";
            KeybindTextBox.Text = "Press key combination...";
            KeybindTextBox.Focus();
            KeybindTextBox.KeyDown += OnKeyDown;
        }

        private void StopCapturing()
        {
            _isCapturing = false;
            CaptureButton.Content = "Capture";
            KeybindTextBox.KeyDown -= OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isCapturing)
            {
                return;
            }

            var modifiers = Keyboard.Modifiers;
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Ignore modifier keys alone
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            var keybindText = BuildKeybindText(modifiers, key);
            
            KeybindCombination = keybindText;
            ValueKey = key;
            ValueModifiers = modifiers;
            
            StopCapturing();
            e.Handled = true;
        }

        private string BuildKeybindText(ModifierKeys modifiers, Key key)
        {
            var parts = new System.Collections.Generic.List<string>();
            
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                parts.Add("Ctrl");
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                parts.Add("Shift");
            }
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                parts.Add("Alt");
            }
            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                parts.Add("Win");
            }
            
            parts.Add(key.ToString());
            
            return string.Join(" + ", parts);
        }
    }
}