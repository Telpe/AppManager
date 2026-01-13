using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

[assembly: DisableRuntimeMarshalling]

namespace AppManager.Core.Keybinds
{
    public partial class GlobalKeyboardHook
    {
        public event EventHandler<GlobalKeyboardHookEventArgs>? KeyboardPressed;

        // EDT: Replaced VkSnapshot(int) with RegisteredKeys(Key[])
        public static Key[]? RegisteredKeys;

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

        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events. These events are
        /// associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="idHook">hook type</param>
        /// <param name="lpfn">hook procedure</param>
        /// <param name="hMod">handle to application instance</param>
        /// <param name="dwThreadId">thread identifier</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
        [LibraryImport("USER32", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
        public static partial IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [LibraryImport("USER32", EntryPoint = "RegisterHotKey", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [LibraryImport("USER32", EntryPoint = "UnregisterHotKey", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UnregisterHotKey(IntPtr hWnd, int id);

        [LibraryImport("USER32", EntryPoint = "PostThreadMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool PostThreadMessage(int idThread, uint Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("User32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static partial int GetMessage(ref Message msg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

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

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">handle to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [LibraryImport("USER32", EntryPoint = "UnhookWindowsHookExW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hHook">handle to current hook</param>
        /// <param name="code">hook code passed to hook procedure</param>
        /// <param name="wParam">value passed to hook procedure</param>
        /// <param name="lParam">value passed to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [LibraryImport("USER32", EntryPoint = "CallNextHookExW", SetLastError = true)]
        public static partial IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
        private static partial uint GetCurrentThreadId();

        public static uint CurrentThreadId => GetCurrentThreadId();

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
        }

    }
}
