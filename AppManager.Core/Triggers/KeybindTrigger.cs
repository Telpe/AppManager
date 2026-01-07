using AppManager.Core.Actions;
using AppManager.Core.Keybinds;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace AppManager.Core.Triggers
{
    internal class KeybindTrigger : BaseTrigger, IKeybindTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Keybind;

        //private GlobalKeyboardHook? GlobalKeyboardHookValue;
        protected Key TargetKey { get => Key ?? System.Windows.Input.Key.None; }
        protected ModifierKeys TargetModifiers { get => Modifiers ?? ModifierKeys.None; }
        private bool KeyPressedValue;
        private bool ModifiersPressedValue;
        private static Thread? MessageListener;
        private static object MessageListenerLock = new object();
        private static HotkeyModel[] RegisteredHotkeysValue = [];
        public static HotkeyModel[] RegisteredHotkeys { get => RegisteredHotkeysValue; }
        //private static int NextHotkeyId = 1;
        private int MyHotkeyId;

        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
        public string? KeybindCombination { get; set; }
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
            lock (MessageListenerLock)
            {
                // Ensure MessageListener thread is created and started
                if (null != MessageListener)
                {
                    StopMessageListener();
                }

                // Convert TargetModifiersValue and TargetKeyValue to appropriate constants
                uint modifiers = ConvertToHotkeyModifiers(TargetModifiers);
                uint vk = ConvertToVirtualKey(TargetKey);
                HotkeyModel newHotkey = new HotkeyModel(IntPtr.Zero, MyHotkeyId, modifiers, vk, this);

                if (!RegisteredHotkeys.Contains(newHotkey))
                {
                    
                    RegisteredHotkeysValue = [..RegisteredHotkeys, newHotkey];
                    MyHotkeyId = RegisteredHotkeys.Length - 1;
                }

                MessageListener = new Thread(()=>MessageListenerLoop((HotkeyModel[])RegisteredHotkeys.Clone()));

                MessageListener.Start();


            }

            Debug.WriteLine($"Keybind trigger '{Name}' started for {TargetModifiers} + {TargetKey} with ID {MyHotkeyId}");
        }

        private void MessageListenerLoop(HotkeyModel[] registeredKeys)
        {
            Thread.CurrentThread.IsBackground = true;

            foreach (HotkeyModel hotkey in registeredKeys)
            {
                GlobalKeyboardHook.RegisterHotKey(hotkey.HWnd, hotkey.Id, hotkey.Mods, hotkey.Key);
            }

            Message msg = new();
            int msgState = 0;

            while ((msgState = GlobalKeyboardHook.GetMessage(ref msg, IntPtr.Zero, 0, 0)) != 0)
            {
                Debug.WriteLine($"Message received: {msg.Msg}, State: {msgState}");
                if (msg.Msg == KeyboardHookConstants.WM_HOTKEY)
                {
                    int hotkeyId = msg.WParam.ToInt32();
                    Debug.WriteLine($"Hotkey pressed with ID: {hotkeyId}");

                    try
                    {
                        HotkeyModel hotkey = registeredKeys.Where(a => a.Id == hotkeyId).First();

                        Debug.WriteLine($"Hotkey pressed: {hotkey.Mods} + {hotkey.Key}");

                        _ = System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => hotkey.Trigger.TriggerActivated());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing hotkey ID {hotkeyId}: {ex.Message}");
                    }

                }
            }

            foreach (HotkeyModel hotkey in registeredKeys)
            {
                GlobalKeyboardHook.UnregisterHotKey(hotkey.HWnd, hotkey.Id);
            } 

            Debug.WriteLine($"MessageListener thread ended.");
        }

        public override void Stop()
        {
            StopMessageListener();
        }

        protected static void StopMessageListener()
        {
            lock (MessageListenerLock)
            {
                if (MessageListener != null && MessageListener.IsAlive)
                {
                    GlobalKeyboardHook.PostThreadMessage(MessageListener.ManagedThreadId, KeyboardHookConstants.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                    MessageListener.Join(TimeSpan.FromSeconds(3));
                    MessageListener = null;
                }
            }
        }

        private void Cleanup()
        {
            //if (null != GlobalKeyboardHookValue)
            //{
            //    GlobalKeyboardHookValue.KeyboardPressed -= OnKeyboardPressed;
            //    GlobalKeyboardHookValue.Dispose();
            //    GlobalKeyboardHookValue = null;
            //}

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
            return KeyboardHookConstants.KeyToDixMap[key];
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