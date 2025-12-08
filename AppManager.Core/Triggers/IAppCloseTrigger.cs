using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers
{
    public interface IAppCloseTrigger
    {
        string? ProcessName { get; set; }
        string? ExecutablePath { get; set; }
        bool? MonitorChildProcesses { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}