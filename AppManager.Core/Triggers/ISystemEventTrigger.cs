using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers
{
    public interface ISystemEventTrigger
    {
        string? EventName { get; set; }
        string? EventSource { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}