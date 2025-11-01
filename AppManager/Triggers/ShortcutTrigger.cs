using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AppManager.Actions;
using AppManager.Shortcuts;

namespace AppManager.Triggers
{
    internal class ShortcutTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Shortcut;
        public override string Description => "Monitors global keyboard shortcuts with high compatibility using GlobalKeyboardHook";

        private TriggerModel _parameters;
        private GlobalKeyboardHook _globalKeyboardHook;
        private Key _targetKey;
        private ModifierKeys _targetModifiers;
        private bool _keyPressed;
        private bool _modifiersPressed;

        public ShortcutTrigger(string name = null) : base(name)
        {
        }

        public override bool CanStart(TriggerModel parameters = null)
        {
            return parameters?.Key != Key.None || !string.IsNullOrEmpty(parameters?.ShortcutCombination);
        }

        public override async Task<bool> StartAsync(TriggerModel parameters = null)
        {
            if (IsActive || parameters == null)
                return false;

            try
            {
                _parameters = parameters;
                _targetKey = parameters.Key;
                _targetModifiers = parameters.Modifiers;

                // Create GlobalKeyboardHook instance
                // Pass null to monitor all keys (we'll filter in the event handler)
                _globalKeyboardHook = new GlobalKeyboardHook();
                _globalKeyboardHook.KeyboardPressed += OnKeyboardPressed;

                IsActive = true;
                Debug.WriteLine($"Shortcut trigger '{Name}' started for {parameters.Key} + {parameters.Modifiers}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting shortcut trigger '{Name}': {ex.Message}");
                Cleanup();
                return false;
            }
        }

        public override async Task<bool> StopAsync()
        {
            if (!IsActive)
                return true;

            try
            {
                Cleanup();
                IsActive = false;
                Debug.WriteLine($"Shortcut trigger '{Name}' stopped");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping shortcut trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        private void Cleanup()
        {
            if (_globalKeyboardHook != null)
            {
                _globalKeyboardHook.KeyboardPressed -= OnKeyboardPressed;
                _globalKeyboardHook.Dispose();
                _globalKeyboardHook = null;
            }

            _keyPressed = false;
            _modifiersPressed = false;
        }

        private void OnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            try
            {
                var pressedKey = e.KeyboardData.Key;
                var isKeyDown = e.KeyboardState == KeyboardState.KeyDown;
                var isKeyUp = e.KeyboardState == KeyboardState.KeyUp;

                // Check if this is our target key
                if (pressedKey == _targetKey)
                {
                    if (isKeyDown)
                    {
                        _keyPressed = true;
                        CheckForShortcutActivation();
                    }
                    else if (isKeyUp)
                    {
                        _keyPressed = false;
                    }
                }
                // Check for modifier keys
                else if (IsModifierKey(pressedKey))
                {
                    if (isKeyDown)
                    {
                        UpdateModifierState();
                    }
                    else if (isKeyUp)
                    {
                        UpdateModifierState();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling keyboard event in shortcut trigger '{Name}': {ex.Message}");
            }
        }

        private bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin;
        }

        private void UpdateModifierState()
        {
            // Check current modifier state using Keyboard class
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool winPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            ModifierKeys currentModifiers = ModifierKeys.None;
            if (ctrlPressed) currentModifiers |= ModifierKeys.Control;
            if (altPressed) currentModifiers |= ModifierKeys.Alt;
            if (shiftPressed) currentModifiers |= ModifierKeys.Shift;
            if (winPressed) currentModifiers |= ModifierKeys.Windows;

            _modifiersPressed = currentModifiers == _targetModifiers;

            if (_modifiersPressed && _keyPressed)
            {
                CheckForShortcutActivation();
            }
        }

        private void CheckForShortcutActivation()
        {
            if (_keyPressed && _modifiersPressed)
            {
                Debug.WriteLine($"Shortcut trigger '{Name}' activated");

                // Trigger the configured action
                OnTriggerActivated("notepad", AppActionEnum.Launch);

                // Reset state to prevent multiple activations
                _keyPressed = false;
                _modifiersPressed = false;
            }
        }

        public override void Dispose()
        {
            Cleanup();
            base.Dispose();
        }
    }
}