using System;

namespace AppManager.OsApi.Windows11.Input
{
    [Flags]
    public enum WindowsModifierKey : uint
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008,
        NoRepeat = 0x4000
    }

    public enum WindowsKey : uint
    {
        None = 0,
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,

        // Numbers
        D0 = 0x30,
        D1 = 0x31,
        D2 = 0x32,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,

        // Function keys
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,

        // Arrow keys
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,

        // Common special keys
        Space = 0x20,
        Enter = 0x0D,
        Tab = 0x09,
        Escape = 0x1B,
        Back = 0x08,
        Delete = 0x2E,
        Insert = 0x2D,
        Home = 0x24,
        End = 0x23,
        PageUp = 0x21,
        PageDown = 0x22,

        // Numpad keys
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 0x64,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,

        // Punctuation
        OemSemicolon = 0xBA,  // ;
        OemPlus = 0xBB,       // =
        OemComma = 0xBC,      // ,
        OemMinus = 0xBD,      // -
        OemPeriod = 0xBE,     // .
        OemQuestion = 0xBF,   // /
        OemTilde = 0xC0,      // `
        OemOpenBracket = 0xDB, // [
        OemPipe = 0xDC,       // \
        OemCloseBracket = 0xDD, // ]
        OemQuotes = 0xDE,      // '

        // Additional keys
        PrintScreen = 0x2C,
        ScrollLock = 0x91,
        Pause = 0x13,
        NumLock = 0x90,
        CapsLock = 0x14,
        NumPadDivide = 0x6F,
        NumPadMultiply = 0x6A,
        NumPadSubtract = 0x6D,
        NumPadAdd = 0x6B,
        NumPadDecimal = 0x6E,
        NumPadEnter = 0x0D,  // Same as Enter
        MediaPlayPause = 0xB3,
        MediaStop = 0xB2,
        MediaNext = 0xB0,
        MediaPrevious = 0xB1,
        VolumeUp = 0xAF,
        VolumeDown = 0xAE,
        VolumeMute = 0xAD,
        LeftCtrl = 0xA2,
        RightCtrl = 0xA3,
        LeftAlt = 0xA4,
        RightAlt = 0xA5,
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LWin = 0x5B,
        RWin = 0x5C,
        ContextMenu = 0x5D,
        Paragraph = 0xB6,
    }
}