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
    internal class ShortcutTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Shortcut;

        private GlobalKeyboardHook GlobalKeyboardHookStored;
        private Key TargetKeyStored;
        private ModifierKeys TargetModifiersStored;
        private bool KeyPressedStored;
        private bool ModifiersPressedStored;

        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
        public string? ShortcutCombination { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        public ShortcutTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors global keyboard shortcuts with high compatibility using GlobalKeyboardHook";
            
            Key = model.Key;
            Modifiers = model.Modifiers;
            ShortcutCombination = model.ShortcutCombination;
            CustomProperties = model.CustomProperties ?? new Dictionary<string, object>();

            TargetKeyStored = Key ?? System.Windows.Input.Key.None;
            TargetModifiersStored = Modifiers ?? ModifierKeys.None;
        }

        public override bool CanStart()
        {
            return Key != System.Windows.Input.Key.None || !string.IsNullOrEmpty(ShortcutCombination);
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (!IsActive) { return false; }

                try
                {
                    // Create GlobalKeyboardHook instance
                    // Pass null to monitor all keys (we'll filter in the event handler)
                    GlobalKeyboardHookStored = new GlobalKeyboardHook();
                    GlobalKeyboardHookStored.KeyboardPressed += OnKeyboardPressed;

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
            if (GlobalKeyboardHookStored != null)
            {
                GlobalKeyboardHookStored.KeyboardPressed -= OnKeyboardPressed;
                GlobalKeyboardHookStored.Dispose();
                GlobalKeyboardHookStored = null;
            }

            KeyPressedStored = false;
            ModifiersPressedStored = false;
        }

        private void OnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            try
            {
                var pressedKey = e.KeyboardData.Key;
                var isKeyDown = e.KeyboardState == KeyboardState.KeyDown;
                var isKeyUp = e.KeyboardState == KeyboardState.KeyUp;

                // Check if this is our target key
                if (pressedKey == TargetKeyStored)
                {
                    if (isKeyDown)
                    {
                        KeyPressedStored = true;
                        CheckForShortcutActivation();
                    }
                    else if (isKeyUp)
                    {
                        KeyPressedStored = false;
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

            ModifiersPressedStored = currentModifiers == TargetModifiersStored;

            if (ModifiersPressedStored && KeyPressedStored)
            {
                CheckForShortcutActivation();
            }
        }

        private void CheckForShortcutActivation()
        {
            if (KeyPressedStored && ModifiersPressedStored)
            {
                Debug.WriteLine($"Shortcut trigger '{Name}' activated");

                // Trigger the configured action
                OnTriggerActivated("target_app", AppActionTypeEnum.Launch, null, new { Key = TargetKeyStored, Modifiers = TargetModifiersStored });

                // Reset state to prevent multiple activations
                KeyPressedStored = false;
                ModifiersPressedStored = false;
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
                IsActive = IsActive,
                Key = Key,
                Modifiers = Modifiers,
                ShortcutCombination = ShortcutCombination,
                CustomProperties = CustomProperties
            };
        }
    }
}