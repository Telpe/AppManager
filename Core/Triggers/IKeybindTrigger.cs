using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppManager.Core.Triggers
{
    public interface IKeybindTrigger
    {
        Key? Key { get; set; }
        ModifierKeys? Modifiers { get; set; }
        string? KeybindCombination { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}