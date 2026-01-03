namespace AppManager.Core.Keybinds
{
    public static class KeyboardHookConstants
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = (KfAltdown >> 8);

        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;
        public const int MOD_NOREPEAT = 0x4000;
        public const int WM_HOTKEY = 0x0312;
        public const int DSIX = 0x49;
        public const int F_KEY = 0x46;        // Virtual key code for F key

        public static bool IsModifierKey(System.Windows.Input.Key key)
        {
            return key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
                   key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt ||
                   key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
                   key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin;
        }
    }
}