using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace AppManager.Core.Shortcuts
{
    public class GlobalKeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

        // EDT: Added an optional parameter (registeredKeys) that accepts keys to restict
        // the logging mechanism.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registeredKeys">Keys that should trigger logging. Pass null for full logging.</param>
        public GlobalKeyboardHook(Key[] registeredKeys = null)
        {
            RegisteredKeys = registeredKeys;
            WindowsHookHandleValue = IntPtr.Zero;
            User32LibraryHandleValue = IntPtr.Zero;
            HookProcValue = LowLevelKeyboardProc; // we must keep alive HookProcStored, because GC is not aware about SetWindowsHookEx behaviour.

            User32LibraryHandleValue = LoadLibrary("User32");
            if (User32LibraryHandleValue == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            WindowsHookHandleValue = SetWindowsHookEx(KeyboardHookConstants.WH_KEYBOARD_LL, HookProcValue, User32LibraryHandleValue, 0);
            if (WindowsHookHandleValue == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
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

                    // ReSharper disable once DelegateSubtraction
                    HookProcValue -= LowLevelKeyboardProc;
                }
            }

            if (User32LibraryHandleValue != IntPtr.Zero)
            {
                if (!FreeLibrary(User32LibraryHandleValue)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                User32LibraryHandleValue = IntPtr.Zero;
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
        private IntPtr User32LibraryHandleValue;
        private HookProc HookProcValue;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

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
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">handle to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hHook">handle to current hook</param>
        /// <param name="code">hook code passed to hook procedure</param>
        /// <param name="wParam">value passed to hook procedure</param>
        /// <param name="lParam">value passed to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        // EDT: Replaced VkSnapshot(int) with RegisteredKeys(Key[])
        public static Key[] RegisteredKeys;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object? o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                if (null == o) { throw new Exception("Failed to get PtrToStructure."); }
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

                // EDT: Removed the comparison-logic from the usage-area so the user does not need to mess around with it.
                // Either the incoming key has to be part of RegisteredKeys (see constructor on top) or RegisterdKeys
                // has to be null for the event to get fired.
                var key = (Key)p.VirtualCode;
                if (RegisteredKeys == null || RegisteredKeys.Contains(key))
                {
                    EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                    handler?.Invoke(this, eventArguments);

                    fEatKeyStroke = eventArguments.Handled;
                }
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
