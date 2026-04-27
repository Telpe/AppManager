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

    public readonly record struct HotkeyModel
    {
        public HotkeyType Type { get; }
        public ModifierKey Modifiers { get; }  // Used for Traditional type
        public HashSet<Key> Keys { get; } = new();  // Used for Multi type, Key[0] for Traditional and Single Type
        public Key MainKey { get => Keys.FirstOrDefault(); }

        // Constructor for Traditional hotkeys
        public HotkeyModel(ModifierKey modifiers, Key mainKey)
        {
            Type = HotkeyType.Traditional;
            Modifiers = modifiers;
            Keys = [mainKey];
        }

        // Constructor for Single hotkeys
        public HotkeyModel(Key singleKey)
        {
            Type = HotkeyType.Single;
            Modifiers = ModifierKey.None;
            Keys = [singleKey];
        }

        // Constructor for Multi hotkeys
        public HotkeyModel(HashSet<Key> keys)
        {
            Type = HotkeyType.Multi;
            Modifiers = ModifierKey.None;
            Keys = keys;
        }

    }
}
