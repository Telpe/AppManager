using AppManager.OsApi.Models;
using AppManager.OsApi.Windows11.Imports;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppManager.OsApi.Windows11.Input
{
    public class KeyListener
    {
        private static uint? MessageListenerNativeThreadId;
        private Thread? MessageListener;
        private object MessageListenerAndRegisteredHotkeysLock = new object();
        private HotkeyModel[] RegisteredHotkeysValue = [];
        public HotkeyModel[] RegisteredHotkeys
        {
            get
            {
                lock (MessageListenerAndRegisteredHotkeysLock)
                {
                    return (HotkeyModel[])RegisteredHotkeysValue.Clone();
                }
            }
        }
        private Action<HotkeyModel>? HotkeyEvent;

        public KeyListener(Action<HotkeyModel> HotkeyRecievedAction)
        {
            HotkeyEvent = HotkeyRecievedAction;
        }

        public void AddHotkey(HotkeyModel hotkey)
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                if (!RegisteredHotkeysValue.Any(a => a.MainKey == hotkey.MainKey && a.Modifiers == hotkey.Modifiers))
                {
                    hotkey.Id = RegisteredHotkeysValue.Length;
                    RegisteredHotkeysValue = [..RegisteredHotkeysValue, hotkey];
                }
                else
                {
                    throw new InvalidOperationException($"The keybind combination {hotkey.Modifiers} + {hotkey.MainKey} is already registered by another KeybindTrigger.");
                }

                StartListening();
            }

            Debug.WriteLine($"Keybind id {hotkey.Id} started with {hotkey.Modifiers} + {hotkey.MainKey}");
        }

        public void StartListening()
        {
            lock (MessageListenerAndRegisteredHotkeysLock)
            {
                StopListening();

                MessageListener = new Thread(() => MessageListenerLoop((HotkeyModel[])RegisteredHotkeysValue.Clone()));

                MessageListener.Start();


                Stopwatch waitedTime = Stopwatch.StartNew();

                while (null == MessageListenerNativeThreadId && waitedTime.ElapsedMilliseconds < 3000)
                {
                    Task.Delay(1).Wait();
                }

                waitedTime.Stop();
            }

            
        }

        private void MessageListenerLoop(HotkeyModel[] registeredKeys)
        {
            Thread.CurrentThread.IsBackground = true;


            foreach (HotkeyModel hotkey in registeredKeys)
            {
                User32Api.RegisterHotKey(hotkey.HWnd, hotkey.Id, (uint)KeyboardHookConstants.ModifiersToWindowsModifiers(hotkey.Modifiers), (uint)KeyboardHookConstants.KeyToDixMap[hotkey.MainKey]);
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
                            HotkeyEvent?.Invoke(registeredKeys.Where(a => a.Id == hotkeyId).First());
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing hotkey ID {hotkeyId}: {ex.Message}");
                    }

                }
            }

            foreach (HotkeyModel hotkey in registeredKeys)
            {
                User32Api.UnregisterHotKey(hotkey.HWnd, hotkey.Id);
            }

            Debug.WriteLine($"MessageListener thread ended.");
        }

        public void StopListening()
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
