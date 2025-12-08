using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppManager.Core.Triggers
{
    public interface IShortcutTrigger
    {
        Key? Key { get; set; }
        ModifierKeys? Modifiers { get; set; }
        string? ShortcutCombination { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}