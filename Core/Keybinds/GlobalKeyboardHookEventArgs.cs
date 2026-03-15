using System.ComponentModel;

namespace AppManager.Core.Keybinds
{
    public class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public KeyboardState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public GlobalKeyboardHookEventArgs(
            LowLevelKeyboardInputEvent keyboardData,
            KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }
}
