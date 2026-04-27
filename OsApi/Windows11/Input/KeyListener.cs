using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Models;
using AppManager.OsApi.Windows11.Imports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppManager.OsApi.Windows11.Input
{
    public class KeyListener : IKeyListener
    {
        private uint? MessageListenerNativeThreadId;
        private Thread? MessageListener;
        private Lock MessageListenerAndRegisteredHotkeysLock = new();
        private HotkeyModel[] RegisteredHotkeysValue = [];
        private Action<HotkeyModel>[] RegisteredHandlersValue = [];
        public Dictionary<HotkeyModel, Action<HotkeyModel>> RegisteredHotkeys
        {
            get
            {
                lock(MessageListenerAndRegisteredHotkeysLock)
                {
                    return RegisteredHotkeysValue.Zip(RegisteredHandlersValue).ToDictionary();
                }
            }
        }

        public void AddHotkey(HotkeyModel hotkey, Action<HotkeyModel> handler, bool delayStart = false)
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                if (!RegisteredHotkeysValue.Any(a => a.Equals(hotkey)))
                {
                    RegisteredHotkeysValue = [..RegisteredHotkeysValue, hotkey];
                    RegisteredHandlersValue = [..RegisteredHandlersValue, handler];

                    if (!delayStart)
                    { StartListening(); }
                }
                else
                {
                    throw new InvalidOperationException($"The keybind combination {hotkey.Modifiers} + {hotkey.MainKey} is already registered by another KeybindTrigger.");
                }
            }

            Debug.WriteLine($"Keybind {hotkey} started");
        }

        public void RemoveHotkey(HotkeyModel hotkey)
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                int index = RegisteredHotkeysValue.IndexOf(hotkey);

                if (-1 < index)
                {
                    int newLength = RegisteredHotkeysValue.Length - 1;

                    HotkeyModel[] rHotkey = new HotkeyModel[newLength];
                    Action<HotkeyModel>[] rHandler = new Action<HotkeyModel>[newLength];

                    Array.ConstrainedCopy(RegisteredHotkeysValue, 0, rHotkey, 0, index);
                    Array.ConstrainedCopy(RegisteredHandlersValue, 0, rHandler, 0, index);

                    Array.ConstrainedCopy(RegisteredHotkeysValue, index + 1, rHotkey, index, newLength - index);
                    Array.ConstrainedCopy(RegisteredHandlersValue, index + 1, rHandler, index, newLength - index);

                    RegisteredHotkeysValue = rHotkey;
                    RegisteredHandlersValue = rHandler;

                    if (0 < RegisteredHotkeysValue.Length)
                    { StartListening(); }
                    else
                    { StopListening(); }
                }
            
            }

            Debug.WriteLine($"Keybind stopped {hotkey}");
        }

        private void StartListening()
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                StopListening();

                MessageListener = new Thread(() => MessageListenerLoop((HotkeyModel[])RegisteredHotkeysValue.Clone(), (Action<HotkeyModel>[])RegisteredHandlersValue.Clone()));

                MessageListener.Start();


                Stopwatch waitedTime = Stopwatch.StartNew();

                while (null == MessageListenerNativeThreadId && waitedTime.ElapsedMilliseconds < 3000)
                {
                    Task.Delay(1).Wait();
                }

                waitedTime.Stop();
            }

            
        }

        private void MessageListenerLoop(HotkeyModel[] registeredKeys, Action<HotkeyModel>[] registeredHandlers)
        {
            Thread.CurrentThread.IsBackground = true;

            {
                int hotkeyId = 0;
                foreach (HotkeyModel hotkey in registeredKeys)
                {
                    User32Api.RegisterHotKey(IntPtr.Zero, hotkeyId, (uint)KeyboardHookConstants.ModifiersToWindowsModifiers(hotkey.Modifiers), (uint)KeyboardHookConstants.KeyToDixMap[hotkey.MainKey]);
                    hotkeyId++;
                }
            }

            FormMessage msg = new();
            int msgState = 0;

            MessageListenerNativeThreadId = OSAPI.Current.CurrentThreadId;

            while ((msgState = User32Api.GetMessage(ref msg, IntPtr.Zero, 0, 0)) != 0)
            {
                Debug.WriteLine($"Message received: {msg.message}, State: {msgState}");
                if (msg.message == KeyboardHookConstants.WM_HOTKEY)
                {
                    int hotkeyId = (int)msg.wParam.ToUInt32();
                    Debug.WriteLine($"Hotkey pressed with ID: {hotkeyId}");

                    try
                    {
                        Task.Run(() =>
                        {
                            registeredHandlers[hotkeyId].Invoke(registeredKeys[hotkeyId]);
                        });
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing hotkey ID {hotkeyId}: {ex.Message}");
                    }

                }
            }

            for(int i = 0; i < registeredKeys.Length; i++)
            {
                User32Api.UnregisterHotKey(IntPtr.Zero, i);
            }

            Debug.WriteLine($"MessageListener thread ended.");
        }

        private void StopListening()
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                if (MessageListener != null && MessageListener.IsAlive)
                {
                    if (null != MessageListenerNativeThreadId)
                    {
                        User32Api.PostThreadMessage((int)MessageListenerNativeThreadId, KeyboardHookConstants.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                        MessageListener.Join(TimeSpan.FromSeconds(3));
                    }

                    if (MessageListener.IsAlive)
                    {
                        Debug.WriteLine("Warning: MessageListener thread did not terminate in a timely manner.\nTrying interrupt");
                        MessageListener.Interrupt();
                    }

                    MessageListener = null;
                    MessageListenerNativeThreadId = null;
                }
            }
        }

        
    }
}
