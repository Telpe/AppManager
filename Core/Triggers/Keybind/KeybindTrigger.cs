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

        private Dispatcher V_CurrentDispatcherValue = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        private HotkeyModel V_Keybind;
        public HotkeyModel? Keybind 
        { 
            get 
            {
                if (Key.None == V_Keybind.MainKey) 
                { return null; }
                return V_Keybind;
            }
            set 
            { 
                if(V_Running){ throw new InvalidOperationException("Error, can not change keybind while trigger is running"); }
                V_Keybind = value ?? new HotkeyModel(Key.None);
            } 
        }

        public string? KeybindCombination { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        protected bool V_Running = false;
        public bool Running { get => V_Running; }

        public KeybindTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors global keyboard shortcuts.";

            Keybind = model.Keybind;
            KeybindCombination = model.KeybindCombination;

        }

        protected override bool CanStartTrigger()
        {
            return V_Keybind.MainKey != Key.None && !Running;
        }

        public override void Start()
        {
            if (V_Running) { throw new InvalidOperationException($"Trigger {TriggerType.ToString()} can not Start when Running."); }

            V_Running = true;

            OSAPI.Current.Input.KeyListener.AddHotkey(V_Keybind, OnKeyboardPressed);

            Log.WriteLine($"Keybind trigger '{Name}' started for {V_Keybind.ToString()}");
        }

        

        public override void Stop()
        {
            OSAPI.Current.Input.KeyListener.RemoveHotkey(V_Keybind);

            V_Running = false;
        }

/*
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
*/
        private void OnKeyboardPressed(HotkeyModel keyCombo)
        {
            try
            {
                V_CurrentDispatcherValue.Invoke(ActivateTrigger);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error handling keyboard event in keybind trigger '{Name}': {ex.Message}");
            }
        }
/*
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
*/
        

        public override void Dispose()
        {
            Stop();
            base.Dispose();
        }

        public override TriggerModel ToModel()
        {
            return ToTriggerModel<IKeybindTrigger>();
        }
    }
}
