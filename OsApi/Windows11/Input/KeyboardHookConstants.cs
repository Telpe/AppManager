using AppManager.OsApi.Models;
using System.Collections.Generic;

namespace AppManager.OsApi.Windows11.Input
{
    public static class KeyboardHookConstants
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = (KfAltdown >> 8);

        public const uint WM_QUIT = 0x0012;

        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;
        public const int MOD_NOREPEAT = 0x4000;
        public const int WM_HOTKEY = 0x0312;
        public const int DSIX = 0x49;
        public const int F_KEY = 0x46;        // Virtual key code for F key

        public static WindowsModifierKey ModifiersToWindowsModifiers(ModifierKey modifiers)
        {
            WindowsModifierKey result = WindowsModifierKey.NoRepeat;

            if (modifiers.HasFlag(ModifierKey.Control))
            {
                result |= WindowsModifierKey.Control;
            }
            if (modifiers.HasFlag(ModifierKey.Alt))
            {
                result |= WindowsModifierKey.Alt;
            }
            if (modifiers.HasFlag(ModifierKey.Shift))
            {
                result |= WindowsModifierKey.Shift;
            }
            if (modifiers.HasFlag(ModifierKey.OS))
            {
                result |= WindowsModifierKey.Windows;
            }

            return result;
        }

        public static Dictionary<Key, WindowsKey> KeyToDixMap = new()
        {
            { Key.A, WindowsKey.A },
            { Key.B, WindowsKey.B },
            { Key.C, WindowsKey.C },
            { Key.D, WindowsKey.D },
            { Key.E, WindowsKey.E },
            { Key.F, WindowsKey.F },
            { Key.G, WindowsKey.G },
            { Key.H, WindowsKey.H },
            { Key.I, WindowsKey.I },
            { Key.J, WindowsKey.J },
            { Key.K, WindowsKey.K },
            { Key.L, WindowsKey.L },
            { Key.M, WindowsKey.M },
            { Key.N, WindowsKey.N },
            { Key.O, WindowsKey.O },
            { Key.P, WindowsKey.P },
            { Key.Q, WindowsKey.Q },
            { Key.R, WindowsKey.R },
            { Key.S, WindowsKey.S },
            { Key.T, WindowsKey.T },
            { Key.U, WindowsKey.U },
            { Key.V, WindowsKey.V },
            { Key.W, WindowsKey.W },
            { Key.X, WindowsKey.X },
            { Key.Y, WindowsKey.Y },
            { Key.Z, WindowsKey.Z },

            // Numbers
            { Key.D0, WindowsKey.D0 },
            { Key.D1, WindowsKey.D1 },
            { Key.D2, WindowsKey.D2 },
            { Key.D3, WindowsKey.D3 },
            { Key.D4, WindowsKey.D4 },
            { Key.D5, WindowsKey.D5 },
            { Key.D6, WindowsKey.D6 },
            { Key.D7, WindowsKey.D7 },
            { Key.D8, WindowsKey.D8 },
            { Key.D9, WindowsKey.D9 },

            // Function keys
            { Key.F1, WindowsKey.F1 },
            { Key.F2, WindowsKey.F2 },
            { Key.F3, WindowsKey.F3 },
            { Key.F4, WindowsKey.F4 },
            { Key.F5, WindowsKey.F5 },
            { Key.F6, WindowsKey.F6 },
            { Key.F7, WindowsKey.F7 },
            { Key.F8, WindowsKey.F8 },
            { Key.F9, WindowsKey.F9 },
            { Key.F10, WindowsKey.F10 },
            { Key.F11, WindowsKey.F11 },
            { Key.F12, WindowsKey.F12 },

            // Arrow keys
            { Key.Left, WindowsKey.Left },
            { Key.Up, WindowsKey.Up },
            { Key.Right, WindowsKey.Right },
            { Key.Down, WindowsKey.Down },

            // Common special keys
            { Key.Paragraph, WindowsKey.Paragraph },
            { Key.Space, WindowsKey.Space },
            { Key.Enter, WindowsKey.Enter },
            { Key.Tab, WindowsKey.Tab },
            { Key.Escape, WindowsKey.Escape },
            { Key.Back, WindowsKey.Back },
            { Key.Delete, WindowsKey.Delete },
            { Key.Insert, WindowsKey.Insert },
            { Key.Home, WindowsKey.Home },
            { Key.End, WindowsKey.End },
            { Key.PageUp, WindowsKey.PageUp },
            { Key.PageDown, WindowsKey.PageDown },
            { Key.ContextMenu, WindowsKey.ContextMenu },

            // Numpad keys
            { Key.NumPad0, WindowsKey.NumPad0 },
            { Key.NumPad1, WindowsKey.NumPad1 },
            { Key.NumPad2, WindowsKey.NumPad2 },
            { Key.NumPad3, WindowsKey.NumPad3 },
            { Key.NumPad4, WindowsKey.NumPad4 },
            { Key.NumPad5, WindowsKey.NumPad5 },
            { Key.NumPad6, WindowsKey.NumPad6 },
            { Key.NumPad7, WindowsKey.NumPad7 },
            { Key.NumPad8, WindowsKey.NumPad8 },
            { Key.NumPad9, WindowsKey.NumPad9 },

            // Punctuation
            { Key.OemSemicolon, WindowsKey.OemSemicolon },
            { Key.OemPlus, WindowsKey.OemPlus },
            { Key.OemComma, WindowsKey.OemComma },
            { Key.OemMinus, WindowsKey.OemMinus },
            { Key.OemPeriod, WindowsKey.OemPeriod },
            { Key.OemQuestion, WindowsKey.OemQuestion },
            { Key.OemTilde, WindowsKey.OemTilde },
            { Key.OemOpenBracket, WindowsKey.OemOpenBracket },
            { Key.OemPipe, WindowsKey.OemPipe },
            { Key.OemCloseBracket, WindowsKey.OemCloseBracket },
            { Key.OemQuotes, WindowsKey.OemQuotes },

            // Additional common keys
            { Key.PrintScreen, WindowsKey.PrintScreen },
            { Key.ScrollLock, WindowsKey.ScrollLock },
            { Key.Pause, WindowsKey.Pause },
            { Key.NumLock, WindowsKey.NumLock },
            { Key.CapsLock, WindowsKey.CapsLock },

            // Keypad operators
            { Key.NumPadDivide, WindowsKey.NumPadDivide },
            { Key.NumPadMultiply, WindowsKey.NumPadMultiply },
            { Key.NumPadSubtract, WindowsKey.NumPadSubtract },
            { Key.NumPadAdd, WindowsKey.NumPadAdd },
            { Key.NumPadDecimal, WindowsKey.NumPadDecimal },
            { Key.NumPadEnter, WindowsKey.NumPadEnter },

            // Media keys
            { Key.MediaPlayPause, WindowsKey.MediaPlayPause },
            { Key.MediaStop, WindowsKey.MediaStop },
            { Key.MediaNext, WindowsKey.MediaNext },
            { Key.MediaPrevious, WindowsKey.MediaPrevious },

            // Volume keys
            { Key.VolumeUp, WindowsKey.VolumeUp },
            { Key.VolumeDown, WindowsKey.VolumeDown },
            { Key.VolumeMute, WindowsKey.VolumeMute },

            // Modifier keys
            { Key.LeftShift, WindowsKey.LeftShift },
            { Key.LeftCtrl, WindowsKey.LeftCtrl },
            { Key.LeftOs, WindowsKey.LWin },
            { Key.LeftAlt, WindowsKey.LeftAlt },
            { Key.RightAlt, WindowsKey.RightAlt },
            { Key.RightOs, WindowsKey.RWin },
            { Key.RightCtrl, WindowsKey.RightCtrl },
            { Key.RightShift, WindowsKey.RightShift }
        };

        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LeftOs || key == Key.RightOs;
        }
    }
}