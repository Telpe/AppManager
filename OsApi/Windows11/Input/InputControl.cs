using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Models;
using System;

namespace AppManager.OsApi.Windows11.Input
{
    public class InputControl : IInputControl
    {
        public IKeyListener KeyListener { get; } = new KeyListener();

        public InputControl()
        {
            
        }

        public void OberserveHotkey(HotkeyModel hotkey, Action<HotkeyModel> handler)
        {
            KeyListener.AddHotkey(hotkey, handler);
        }

        public void ForgetHotkey(HotkeyModel hotkey)
        {
            KeyListener.RemoveHotkey(hotkey);

        }

    }
}
