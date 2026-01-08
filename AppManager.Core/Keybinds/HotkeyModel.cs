using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace AppManager.Core.Keybinds
{
    internal struct HotkeyModel
    {
        public IntPtr HWnd;
        public int Id;
        public uint Mods;
        public uint Key;
        public KeybindTrigger Trigger;
        public Dispatcher Dispatcher;

        public HotkeyModel(IntPtr hWnd, int id, uint mods, uint key, KeybindTrigger trigger, Dispatcher dispatcher)
        {
            this.HWnd = hWnd;
            this.Id = id;
            this.Mods = mods;
            this.Key = key;
            this.Trigger = trigger;
            this.Dispatcher = dispatcher;
        }
    }
}
