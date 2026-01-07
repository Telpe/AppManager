namespace AppManager.Core.Keybinds
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
        public static Dictionary<System.Windows.Input.Key, uint> KeyToDixMap = new()
        {
            { System.Windows.Input.Key.A, 0x41 },
            { System.Windows.Input.Key.B, 0x42 },
            { System.Windows.Input.Key.C, 0x43 },
            { System.Windows.Input.Key.D, 0x44 },
            { System.Windows.Input.Key.E, 0x45 },
            { System.Windows.Input.Key.F, 0x46 },
            { System.Windows.Input.Key.G, 0x47 },
            { System.Windows.Input.Key.H, 0x48 },
            { System.Windows.Input.Key.I, 0x49 },
            { System.Windows.Input.Key.J, 0x4A },
            { System.Windows.Input.Key.K, 0x4B },
            { System.Windows.Input.Key.L, 0x4C },
            { System.Windows.Input.Key.M, 0x4D },
            { System.Windows.Input.Key.N, 0x4E },
            { System.Windows.Input.Key.O, 0x4F },
            { System.Windows.Input.Key.P, 0x50 },
            { System.Windows.Input.Key.Q, 0x51 },
            { System.Windows.Input.Key.R, 0x52 },
            { System.Windows.Input.Key.S, 0x53 },
            { System.Windows.Input.Key.T, 0x54 },
            { System.Windows.Input.Key.U, 0x55 },
            { System.Windows.Input.Key.V, 0x56 },
            { System.Windows.Input.Key.W, 0x57 },
            { System.Windows.Input.Key.X, 0x58 },
            { System.Windows.Input.Key.Y, 0x59 },
            { System.Windows.Input.Key.Z, 0x5A },
            
            // Numbers
            { System.Windows.Input.Key.D0, 0x30 },
            { System.Windows.Input.Key.D1, 0x31 },
            { System.Windows.Input.Key.D2, 0x32 },
            { System.Windows.Input.Key.D3, 0x33 },
            { System.Windows.Input.Key.D4, 0x34 },
            { System.Windows.Input.Key.D5, 0x35 },
            { System.Windows.Input.Key.D6, 0x36 },
            { System.Windows.Input.Key.D7, 0x37 },
            { System.Windows.Input.Key.D8, 0x38 },
            { System.Windows.Input.Key.D9, 0x39 },
            
            // Function keys
            { System.Windows.Input.Key.F1, 0x70 },
            { System.Windows.Input.Key.F2, 0x71 },
            { System.Windows.Input.Key.F3, 0x72 },
            { System.Windows.Input.Key.F4, 0x73 },
            { System.Windows.Input.Key.F5, 0x74 },
            { System.Windows.Input.Key.F6, 0x75 },
            { System.Windows.Input.Key.F7, 0x76 },
            { System.Windows.Input.Key.F8, 0x77 },
            { System.Windows.Input.Key.F9, 0x78 },
            { System.Windows.Input.Key.F10, 0x79 },
            { System.Windows.Input.Key.F11, 0x7A },
            { System.Windows.Input.Key.F12, 0x7B },
            
            // Arrow keys
            { System.Windows.Input.Key.Left, 0x25 },
            { System.Windows.Input.Key.Up, 0x26 },
            { System.Windows.Input.Key.Right, 0x27 },
            { System.Windows.Input.Key.Down, 0x28 },
            
            // Common special keys
            { System.Windows.Input.Key.Space, 0x20 },
            { System.Windows.Input.Key.Enter, 0x0D },
            { System.Windows.Input.Key.Tab, 0x09 },
            { System.Windows.Input.Key.Escape, 0x1B },
            { System.Windows.Input.Key.Back, 0x08 },
            { System.Windows.Input.Key.Delete, 0x2E },
            { System.Windows.Input.Key.Insert, 0x2D },
            { System.Windows.Input.Key.Home, 0x24 },
            { System.Windows.Input.Key.End, 0x23 },
            { System.Windows.Input.Key.PageUp, 0x21 },
            { System.Windows.Input.Key.PageDown, 0x22 },
            
            // Numpad keys
            { System.Windows.Input.Key.NumPad0, 0x60 },
            { System.Windows.Input.Key.NumPad1, 0x61 },
            { System.Windows.Input.Key.NumPad2, 0x62 },
            { System.Windows.Input.Key.NumPad3, 0x63 },
            { System.Windows.Input.Key.NumPad4, 0x64 },
            { System.Windows.Input.Key.NumPad5, 0x65 },
            { System.Windows.Input.Key.NumPad6, 0x66 },
            { System.Windows.Input.Key.NumPad7, 0x67 },
            { System.Windows.Input.Key.NumPad8, 0x68 },
            { System.Windows.Input.Key.NumPad9, 0x69 },
            
            // Punctuation
            { System.Windows.Input.Key.OemSemicolon, 0xBA },  // ;
            { System.Windows.Input.Key.OemPlus, 0xBB },       // =
            { System.Windows.Input.Key.OemComma, 0xBC },      // ,
            { System.Windows.Input.Key.OemMinus, 0xBD },      // -
            { System.Windows.Input.Key.OemPeriod, 0xBE },     // .
            { System.Windows.Input.Key.OemQuestion, 0xBF },   // /
            { System.Windows.Input.Key.OemTilde, 0xC0 },      // `
            { System.Windows.Input.Key.OemOpenBrackets, 0xDB }, // [
            { System.Windows.Input.Key.OemPipe, 0xDC },       // \
            { System.Windows.Input.Key.OemCloseBrackets, 0xDD }, // ]
            { System.Windows.Input.Key.OemQuotes, 0xDE }      // '
        };

        public static bool IsModifierKey(System.Windows.Input.Key key)
        {
            return key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
                   key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt ||
                   key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
                   key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin;
        }
    }
}