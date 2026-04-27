using AppManager.Core.Keybinds;
using AppManager.Core.Models;
using AppManager.OsApi;
using AppManager.OsApi.Models;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace AppManager.Core.Triggers.Keybind
{
    public class KeybindTrigger : BaseTrigger, IKeybindTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Keybind;

        private Dispatcher CurrentDispatcherValue = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        protected HotkeyModel TargetKey { get => Key ?? OsApi.Models.Key.None; }
        private bool KeyPressedValue;
        private bool ModifiersPressedValue;
        public Dictionary<string, object>? CustomProperties { get; set; }

        public KeybindTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors global keyboard shortcuts with high compatibility using GlobalKeyboardHook";
            
            KeybindCombination = model.KeybindCombination;
            CustomProperties = model.CustomProperties ?? new Dictionary<string, object>();

            Key = model.Key;
            Modifiers = model.Modifiers;
        }

        protected override bool CanStartTrigger()
        {
            return TargetKey != System.Windows.Input.Key.None;
        }

        public override void Start()
        {

                uint modifiers = ConvertToHotkeyModifiers(TargetModifiers);
                uint vk = ConvertToVirtualKey(TargetKey);

                var hk = new OsApi.Models.HotkeyModel(Enum.Parse(typeof(OsApi.Models.ModifierKey), modifiers), vk);


                OSAPI.Current.Input.KeyListener.AddHotkey(TargetKey, OnKeyboardPressed);
                    (OnKeyboardPressed);


            Log.WriteLine($"Keybind trigger '{Name}' started for {TargetModifiers} + {TargetKey} with ID {MyHotkeyId}");
        }

        

        public override void Stop()
        {
            OSAPI.Current.Input.KeyListener.RemoveHotkey(TargetKey);
        }

        

        private void Cleanup()
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                StopMessageListener();
                RegisteredHotkeysValue = [];
                MyHotkeyId = -1;
            }

            KeyPressedValue = false;
            ModifiersPressedValue = false;
        }

        private uint ConvertToHotkeyModifiers(ModifierKeys modifiers)
        {
            uint result = KeyboardHookConstants.MOD_NOREPEAT;
            
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                result |= KeyboardHookConstants.MOD_CONTROL;
            }
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                result |= KeyboardHookConstants.MOD_ALT;
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                result |= KeyboardHookConstants.MOD_SHIFT;
            }
            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                result |= KeyboardHookConstants.MOD_WIN;
            }
                
            return result;
        }

        private uint ConvertToVirtualKey(Key key)
        {
            return KeyboardHookConstants.KeyToDixMap[key];
        }

        private void OnKeyboardPressed(HotkeyModel keyCombo)
        {
            if (e == null) { return; }
            try
            {
                var pressedKey = e.KeyboardData.Key;
                var isKeyDown = e.KeyboardState == KeyboardState.KeyDown;
                var isKeyUp = e.KeyboardState == KeyboardState.KeyUp;

                // Check if this is our target key
                if (pressedKey == TargetKey)
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
                else if (KeyboardHookConstants.IsModifierKey(pressedKey))
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
                Log.WriteLine($"Error handling keyboard event in keybind trigger '{Name}': {ex.Message}");
            }
        }

        private void UpdateModifierState()
        {
            // Check current modifier state using Keyboard class
            bool ctrlPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
            bool altPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt);
            bool shiftPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
            bool winPressed = Keyboard.IsKeyDown(System.Windows.Input.Key.LWin) || Keyboard.IsKeyDown(System.Windows.Input.Key.RWin);

            ModifierKeys currentModifiers = ModifierKeys.None;
            if (ctrlPressed)
            {
                currentModifiers |= ModifierKeys.Control;
            }

            if (altPressed)
            {
                currentModifiers |= ModifierKeys.Alt;
            }

            if (shiftPressed)
            {
                currentModifiers |= ModifierKeys.Shift;
            }

            if (winPressed)
            {
                currentModifiers |= ModifierKeys.Windows;
            }

            ModifiersPressedValue = currentModifiers == TargetModifiers;

            if (ModifiersPressedValue && KeyPressedValue)
            {
                CheckForShortcutActivation();
            }
        }

        private void CheckForShortcutActivation()
        {
            if (KeyPressedValue && ModifiersPressedValue)
            {
                Log.WriteLine($"Shortcut trigger '{Name}' activated");

                // Trigger the configured action
                ActivateTrigger();

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
            return ToTriggerModel<IKeybindTrigger>();
        }
    }
}