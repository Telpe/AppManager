using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers.AppLaunched
{
    public interface IAppLaunchTrigger
    {
        string? ProcessName { get; set; }
        string? ExecutablePath { get; set; }
        bool? MonitorChildProcesses { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}