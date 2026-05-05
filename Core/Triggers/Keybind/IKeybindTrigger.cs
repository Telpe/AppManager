using System.Collections.Generic;
using AppManager.OsApi.Models;

namespace AppManager.Core.Triggers.Keybind
{
    public interface IKeybindTrigger
    {
        HotkeyModel? Keybind { get; set; }
        string? KeybindCombination { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}
