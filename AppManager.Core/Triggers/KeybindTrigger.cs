using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using AppManager.Core.Actions;
using System.Collections.Generic;
using AppManager.Core.Models;
using AppManager.Core.Keybinds;
using System.Windows;

namespace AppManager.Core.Triggers
{
    internal class KeybindTrigger : BaseTrigger, IKeybindTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Keybind;

        private GlobalKeyboardHook? GlobalKeyboardHookValue;
        private Key TargetKeyValue;
        private ModifierKeys TargetModifiersValue;
        private bool KeyPressedValue;
        private bool ModifiersPressedValue;
        private static Thread? MessageListener;
        private static readonly object MessageListenerLock = new object();
        private static readonly Dictionary<int, KeybindTrigger> RegisteredHotkeys = new Dictionary<int, KeybindTrigger>();
        private static int NextHotkeyId = 1;
        private int MyHotkeyId;

        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
        public string? KeybindCombination { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        public KeybindTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors global keyboard shortcuts with high compatibility using GlobalKeyboardHook";
            
            //Key = model.Key;
            //Modifiers = model.Modifiers;
            KeybindCombination = model.KeybindCombination;
            CustomProperties = model.CustomProperties ?? new Dictionary<string, object>();

            TargetKeyValue = model.Key ?? System.Windows.Input.Key.None;
            TargetModifiersValue = model.Modifiers ?? ModifierKeys.None;
        }

        protected override bool CanStartTrigger()
        {
            return Key != System.Windows.Input.Key.None || !string.IsNullOrEmpty(KeybindCombination);
        }

        public override void Start()
        {
            lock (MessageListenerLock)
            {
                // Ensure MessageListener thread is created and started
                if (null == MessageListener)
                {
                    MessageListener = new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        GlobalKeyboardHook.RegisterHotKey(IntPtr.Zero, MyHotkeyId, KeyboardHookConstants.MOD_NOREPEAT, KeyboardHookConstants.F_KEY);

                        Message msg = new();
                        int msgState = 0;

                        while ((msgState = GlobalKeyboardHook.GetMessage(ref msg, IntPtr.Zero, 0, 0)) != 0)
                        {
                            Debug.WriteLine($"Message received: {msg.Msg}, State: {msgState}");
                            if (msg.Msg == KeyboardHookConstants.WM_HOTKEY)
                            {
                                int hotkeyId = msg.WParam.ToInt32();
                                Debug.WriteLine($"Hotkey pressed with ID: {hotkeyId}");
                                
                                lock (MessageListenerLock)
                                {
                                    if (RegisteredHotkeys.TryGetValue(hotkeyId, out KeybindTrigger? trigger))
                                    {
                                        Debug.WriteLine($"Hotkey pressed: {trigger.TargetModifiersValue} + {trigger.TargetKeyValue}");
                                        trigger.TriggerActivated();
                                    }
                                }
                            }
                        }

                        Debug.WriteLine($"MessageListener thread ended.");
                    });

                    MessageListener.Start();
                }

                // Register this trigger's hotkey
                MyHotkeyId = NextHotkeyId++;
                RegisteredHotkeys[MyHotkeyId] = this;
                
                // Convert TargetModifiersValue and TargetKeyValue to appropriate constants
                uint modifiers = ConvertToHotkeyModifiers(TargetModifiersValue);
                uint vk = ConvertToVirtualKey(TargetKeyValue);
                
                //GlobalKeyboardHook.RegisterHotKey(IntPtr.Zero, MyHotkeyId, modifiers, vk);
            }

            Debug.WriteLine($"Shortcut trigger '{Name}' started for {TargetModifiersValue} + {TargetKeyValue} with ID {MyHotkeyId}");
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
            if (null != GlobalKeyboardHookValue)
            {
                GlobalKeyboardHookValue.KeyboardPressed -= OnKeyboardPressed;
                GlobalKeyboardHookValue.Dispose();
                GlobalKeyboardHookValue = null;
            }

            lock (MessageListenerLock)
            {
                if (MyHotkeyId != 0)
                {
                    GlobalKeyboardHook.UnregisterHotKey(IntPtr.Zero, MyHotkeyId);
                    RegisteredHotkeys.Remove(MyHotkeyId);
                    MyHotkeyId = 0;
                }
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
            // You'll need to implement this conversion based on your KeyboardHookConstants
            // For now, returning DSIX as placeholder
            return KeyboardHookConstants.DSIX;
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
                Debug.WriteLine($"Error handling keyboard event in keybind trigger '{Name}': {ex.Message}");
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
                TriggerActivated();

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
            TriggerModel model = base.ToModel();
            model.Key = Key;
            model.Modifiers = Modifiers;
            model.KeybindCombination = KeybindCombination;
            model.CustomProperties = CustomProperties;

            return model;
        }
    }
}