using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AppManager.Core.Actions;
using AppManager.Core.Shortcuts;
using System.Collections.Generic;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal class ShortcutTrigger : BaseTrigger, IShortcutTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Shortcut;

        private GlobalKeyboardHook GlobalKeyboardHookValue;
        private Key TargetKeyValue;
        private ModifierKeys TargetModifiersValue;
        private bool KeyPressedValue;
        private bool ModifiersPressedValue;

        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
        public string? ShortcutCombination { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        public ShortcutTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors global keyboard shortcuts with high compatibility using GlobalKeyboardHook";
            
            Key = model.Key;
            Modifiers = model.Modifiers;
            ShortcutCombination = model.ShortcutCombination;
            CustomProperties = model.CustomProperties ?? new Dictionary<string, object>();

            TargetKeyValue = Key ?? System.Windows.Input.Key.None;
            TargetModifiersValue = Modifiers ?? ModifierKeys.None;
        }

        public override bool CanStart()
        {
            return Key != System.Windows.Input.Key.None || !string.IsNullOrEmpty(ShortcutCombination);
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (Inactive) { return false; }

                try
                {
                    // Create GlobalKeyboardHook instance
                    // Pass null to monitor all keys (we'll filter in the event handler)
                    GlobalKeyboardHookValue = new GlobalKeyboardHook();
                    GlobalKeyboardHookValue.KeyboardPressed += OnKeyboardPressed;

                    Debug.WriteLine($"Shortcut trigger '{Name}' started for {Key} + {Modifiers}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting shortcut trigger '{Name}': {ex.Message}");
                    Cleanup();
                    return false;
                }
            });
        }

        public override void Stop()
        {
            try
            {
                Cleanup();
                Debug.WriteLine($"Shortcut trigger '{Name}' stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping shortcut trigger '{Name}': {ex.Message}");
            }
        }

        private void Cleanup()
        {
            if (GlobalKeyboardHookValue != null)
            {
                GlobalKeyboardHookValue.KeyboardPressed -= OnKeyboardPressed;
                GlobalKeyboardHookValue.Dispose();
                GlobalKeyboardHookValue = null;
            }

            KeyPressedValue = false;
            ModifiersPressedValue = false;
        }

        private void OnKeyboardPressed(object? sender, GlobalKeyboardHookEventArgs? e)
        {
            if (e == null) { return; }
            try
            {
                var pressedKey = e.KeyboardData.Key;
                var isKeyDown = e.KeyboardState == KeyboardState.KeyDown;
                var isKeyUp = e.KeyboardState == KeyboardState.KeyUp;

                // Check if this is our target key
                if (pressedKey == TargetKeyValue)
                {
                    if (isKeyDown)
                    {
                        KeyPressedValue = true;
                        CheckForShortcutActivation();
                    }
                    else if (isKeyUp)
                    {
                        KeyPressedValue = false;
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

        private bool IsModifierKey(System.Windows.Input.Key key)
        {
            return key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
                   key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt ||
                   key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
                   key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin;
        }

        private void UpdateModifierState()
        {
            // Check current modifier state using Keyboard class
            bool ctrlPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
            bool altPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt);
            bool shiftPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
            bool winPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LWin) || Keyboard.IsKeyDown(System.Windows.Input.Key.RWin);

            ModifierKeys currentModifiers = ModifierKeys.None;
            if (ctrlPressed) currentModifiers |= ModifierKeys.Control;
            if (altPressed) currentModifiers |= ModifierKeys.Alt;
            if (shiftPressed) currentModifiers |= ModifierKeys.Shift;
            if (winPressed) currentModifiers |= ModifierKeys.Windows;

            ModifiersPressedValue = currentModifiers == TargetModifiersValue;

            if (ModifiersPressedValue && KeyPressedValue)
            {
                CheckForShortcutActivation();
            }
        }

        private void CheckForShortcutActivation()
        {
            if (KeyPressedValue && ModifiersPressedValue)
            {
                Debug.WriteLine($"Shortcut trigger '{Name}' activated");

                // Trigger the configured action
                OnTriggerActivated("target_app", AppActionTypeEnum.Launch, null, new { Key = TargetKeyValue, Modifiers = TargetModifiersValue });

                // Reset state to prevent multiple activations
                KeyPressedValue = false;
                ModifiersPressedValue = false;
            }
        }

        public override void Dispose()
        {
            Cleanup();
            base.Dispose();
        }

        public override TriggerModel ToModel()
        {
            return new TriggerModel
            {
                TriggerType = TriggerType,
                Inactive = Inactive,
                Key = Key,
                Modifiers = Modifiers,
                ShortcutCombination = ShortcutCombination,
                CustomProperties = CustomProperties
            };
        }
    }
}