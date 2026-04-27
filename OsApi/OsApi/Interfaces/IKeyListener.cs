using AppManager.OsApi.Models;
using System;

namespace AppManager.OsApi.Interfaces
{
    public interface IKeyListener
    {
        void AddHotkey(HotkeyModel hotkey, Action<HotkeyModel> handler, bool delayStart = false);

        void RemoveHotkey(HotkeyModel hotkey);
    }
}
