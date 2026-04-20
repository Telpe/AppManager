using System;
using System.Collections.Generic;
using System.Linq;

namespace AppManager.OsApi.Models
{
    public enum HotkeyType
    {
        Traditional,  // ModifierKey + Key (e.g., Ctrl + A)
        Single,       // Single Key (e.g., LeftCtrl)
        Multi         // Any combination of keys (e.g., A + S + D)
    }

    public class HotkeyModel
    {
        public HotkeyType Type { get; }
        public ModifierKey Modifiers { get; }  // Used for Traditional type
        public HashSet<Key> Keys { get; } = new();  // Used for Multi type, Key[0] for Traditional and Single Type
        public Key MainKey { get => Keys.FirstOrDefault(); }
        public IntPtr HWnd { get; }

        private int IdValue = -1;
        public int Id 
        { 
            get => IdValue;
            set 
            {
                if (IdValue != -1)
                {
                    throw new InvalidOperationException("ID has already been set for this hotkey.");
                }

                IdValue = value;
            }
        }
        public IHotkeyRequester Requester { get; }

        // Constructor for Traditional hotkeys
        public HotkeyModel(IntPtr hWnd, ModifierKey modifiers, Key mainKey, IHotkeyRequester requester)
        {
            Type = HotkeyType.Traditional;
            Modifiers = modifiers;
            Keys = [mainKey];
            HWnd = hWnd;
            Requester = requester;
        }

        // Constructor for Single hotkeys
        public HotkeyModel(IntPtr hWnd, Key singleKey, IHotkeyRequester requester)
        {
            Type = HotkeyType.Single;
            Keys = [singleKey];
            HWnd = hWnd;
            Requester = requester;
        }

        // Constructor for Multi hotkeys
        public HotkeyModel(IntPtr hWnd, HashSet<Key> keys, IHotkeyRequester requester)
        {
            Type = HotkeyType.Multi;
            Keys = keys;
            HWnd = hWnd;
            Requester = requester;
        }

    }
}
