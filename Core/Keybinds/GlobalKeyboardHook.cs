using System.Runtime.CompilerServices;
using System.Windows.Input;

[assembly: DisableRuntimeMarshalling]

namespace AppManager.Core.Keybinds
{
    public partial class GlobalKeyboardHook
    {
        public event TrueEventHandler<GlobalKeyboardHookEventArgs>? KeyboardPressed;

        // EDT: Replaced VkSnapshot(int) with RegisteredKeys(Key[])
        public static Key[]? RegisteredKeys;
/*
        // EDT: Added an optional parameter (registeredKeys) that accepts keys to restict
        // the logging mechanism.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registeredKeys">Keys that should trigger logging. Pass null for full logging.</param>
        public GlobalKeyboardHook(Key[]? registeredKeys = null)
        {
            RegisteredKeys = registeredKeys;
            WindowsHookHandleValue = IntPtr.Zero;
            HookProcValue = LowLevelKeyboardProc;

            // Get current process module handle for the hook
            IntPtr moduleHandle = Marshal.GetHINSTANCE(typeof(GlobalKeyboardHook).Module);
            
            WindowsHookHandleValue = SetWindowsHookEx(
                KeyboardHookConstants.WH_KEYBOARD_LL, 
                HookProcValue, 
                moduleHandle, 
                0);
                
            if (WindowsHookHandleValue == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Log.WriteLine($"SetWindowsHookEx failed with error: {errorCode}");
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(errorCode).Message}.");
            }
            else
            {
                Log.WriteLine("GlobalKeyboardHook installed successfully");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (WindowsHookHandleValue != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(WindowsHookHandleValue))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    WindowsHookHandleValue = IntPtr.Zero;
                }
            }
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IntPtr WindowsHookHandleValue;
        private HookProc HookProcValue;

        public static Task<Message> GetMessageAsync(IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            return Task.Run(() =>
            {
                Message msg = new();
                int result = GetMessage(ref msg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result == -1)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"GetMessage failed. Error {errorCode}: {new Win32Exception(errorCode).Message}");
                }
                return msg;
            });
        }

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Log.WriteLine($"Hook called: nCode={nCode}, wParam={wParam}, lParam={lParam}");
            
            bool fEatKeyStroke = false;

            // HC_ACTION = 0
            if (nCode >= 0)
            {
                var wparamTyped = wParam.ToInt32();
                Log.WriteLine($"Key event: {wparamTyped}");
                
                if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
                {
                    object? o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                    if (null == o) { throw new Exception("Failed to get PtrToStructure."); }
                    LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                    var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);
                    var key = (Key)p.VirtualCode;
                    
                    Log.WriteLine($"Key pressed: {key}, State: {(KeyboardState)wparamTyped}");

                    if (RegisteredKeys == null || RegisteredKeys.Contains(key))
                    {
                        KeyboardPressed?.Invoke(this, eventArguments);
                        fEatKeyStroke = eventArguments.Handled;
                    }
                }
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }*/

    }
}
