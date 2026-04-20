using System;

namespace AppManager.OsApi.Models
{
    [Flags]
    public enum ModifierKey
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        OS = 8
    }

    public enum Key
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7,
        H = 8,
        I = 9,
        J = 10,
        K = 11,
        L = 12,
        M = 13,
        N = 14,
        O = 15,
        P = 16,
        Q = 17,
        R = 18,
        S = 19,
        T = 20,
        U = 21,
        V = 22,
        W = 23,
        X = 24,
        Y = 25,
        Z = 26,

        // Numbers
        D0 = 27,
        D1 = 28,
        D2 = 29,
        D3 = 30,
        D4 = 31,
        D5 = 32,
        D6 = 33,
        D7 = 34,
        D8 = 35,
        D9 = 36,

        // Function keys
        F1 = 37,
        F2 = 38,
        F3 = 39,
        F4 = 40,
        F5 = 41,
        F6 = 42,
        F7 = 43,
        F8 = 44,
        F9 = 45,
        F10 = 46,
        F11 = 47,
        F12 = 48,

        // Arrow keys
        Left = 49,
        Up = 50,
        Right = 51,
        Down = 52,

        // Common special keys
        Paragraph = 112,
        Space = 53,
        Enter = 54,
        Tab = 55,
        Escape = 56,
        Back = 57,
        Delete = 58,
        Insert = 59,
        Home = 60,
        End = 61,
        PageUp = 62,
        PageDown = 63,
        ContextMenu = 111,

        // Numpad keys
        NumPad0 = 64,
        NumPad1 = 65,
        NumPad2 = 66,
        NumPad3 = 67,
        NumPad4 = 68,
        NumPad5 = 69,
        NumPad6 = 70,
        NumPad7 = 71,
        NumPad8 = 72,
        NumPad9 = 73,

        // Punctuation
        OemSemicolon = 74,  // ;
        OemPlus = 75,       // =
        OemComma = 76,      // ,
        OemMinus = 77,      // -
        OemPeriod = 78,     // .
        OemQuestion = 79,   // /
        OemTilde = 80,      // `
        OemOpenBracket = 81, // [
        OemPipe = 82,       // \
        OemCloseBracket = 83, // ]
        OemQuotes = 84,      // '

        // Additional common keys
        PrintScreen = 85,
        ScrollLock = 86,
        Pause = 87,
        NumLock = 88,
        CapsLock = 89,

        // Keypad operators
        NumPadDivide = 90,   // /
        NumPadMultiply = 91, // *
        NumPadSubtract = 92, // -
        NumPadAdd = 93,       // +
        NumPadDecimal = 94,   // .
        NumPadEnter = 95,

        // Media keys
        MediaPlayPause = 96,
        MediaStop = 97,
        MediaNext = 98,
        MediaPrevious = 99,

        // Volume keys
        VolumeUp = 100,
        VolumeDown = 101,
        VolumeMute = 102,

        // Modifier keys left
        
        LeftShift = 107,
        LeftCtrl = 103,
        LeftOs = 109,
        LeftAlt = 105,

        // Modifier keys right
        RightAlt = 106,
        RightOs = 110,
        RightCtrl = 104,
        RightShift = 108
    }
}
