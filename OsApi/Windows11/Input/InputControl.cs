using AppManager.OsApi.Events;
using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Models;
using System;
using System.Diagnostics;

namespace AppManager.OsApi.Windows11.Input
{
    public class InputControl : IInputControl
    {
        private KeyListener? KeyListener;

        public ObservableEvent<object?, HotkeyModel> KeyEvent { get; } = new();

        public InputControl()
        {
            KeyEvent.CountChangedEvent += KeyEventChangedCount;
        }

        public void OberserveHotkey(HotkeyModel hotkey)
        {
            KeyListener?.AddHotkey(hotkey);
        }

        public void ForgetHotkey(HotkeyModel hotkey)
        {
            throw new NotImplementedException("Hotkey unregistration is currently not supported. Please use the same hotkey combination for the new hotkey you want to register instead.");
        }

        private void KeyEventChangedCount(object? sender, int newCount)
        {

            if (0 < KeyEvent.Count)
            {
                if (KeyListener is null)
                {
                    KeyListener = new KeyListener(KeyListener_HotkeyRecieved);
                }

                KeyListener.StartListening();

            }
            else
            {
                KeyListener?.StopListening();
                KeyListener = null;
            }
        }

        private void KeyListener_HotkeyRecieved(HotkeyModel hotkey)
        {

            Debug.WriteLine($"Hotkey pressed: {hotkey.Modifiers} + {hotkey.MainKey}");

            if (hotkey.Requester is not null)
            {
                hotkey.Requester.OnKeysClicked(hotkey);
            }
            else
            {
                KeyEvent.Invoke(this, hotkey);
            }
            
        }
    }
}
