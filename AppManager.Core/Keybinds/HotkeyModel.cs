using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.Core.Keybinds
{
    internal struct HotkeyModel
    {
        public IntPtr HWnd;
        public int Id;
        public uint Mods;
        public uint Key;
        public KeybindTrigger Trigger;

        public HotkeyModel(IntPtr hWnd, int id, uint mods, uint key, KeybindTrigger trigger)
        {
            this.HWnd = hWnd;
            this.Id = id;
            this.Mods = mods;
            this.Key = key;
            this.Trigger = trigger;
        }
    }
}
