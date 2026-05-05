using AppManager.Core.Triggers;
using AppManager.Core.Triggers.AppClosed;
using AppManager.Core.Triggers.AppLaunched;
using AppManager.Core.Triggers.Button;
using AppManager.Core.Triggers.Keybind;
using AppManager.Core.Triggers.NetworkPort;
using AppManager.Core.Triggers.SystemEvent;
using AppManager.OsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppManager.Core.Models
{
    public class TriggerModel : ConditionalModel, IAppCloseTrigger, IAppLaunchTrigger, IKeybindTrigger, ISystemEventTrigger, INetworkPortTrigger, IButtonTrigger
    {
        public TriggerTypeEnum TriggerType { get; set; }
        public string Id { get; set; } = string.Empty;

        // Shortcut-specific parameters
        public HotkeyModel? Keybind { get; set; }
        public string? KeybindCombination { get; set; }
        
        // App monitoring parameters
        public string? ProcessName { get; set; }
        public string? ExecutablePath { get; set; }
        public bool? MonitorChildProcesses { get; set; } = false;
        
        // System event parameters
        public string? EventName { get; set; }
        public string? EventSource { get; set; }
        
        // Network parameters
        public int? Port { get; set; }
        public string? IPAddress { get; set; }
        
        // Polling parameters
        public int? PollingIntervalMs { get; set; }
        public int? TimeoutMs { get; set; }
        
        [Obsolete("All properties should be defined.")]
        public Dictionary<string, object>? CustomProperties { get; set; }

        public Dictionary<string,string>? Tags { get; set; }

        public ActionModel[]? Actions { get; set; }

        public TriggerModel Clone()
        {
            return new()
            {
                TriggerType = TriggerType,
                Id = Id,
                Keybind = Keybind,
                KeybindCombination = KeybindCombination,
                ProcessName = ProcessName,
                ExecutablePath = ExecutablePath,
                MonitorChildProcesses = MonitorChildProcesses,
                EventName = EventName,
                EventSource = EventSource,
                Port = Port,
                IPAddress = IPAddress,
                PollingIntervalMs = PollingIntervalMs,
                TimeoutMs = TimeoutMs,
                Conditions = this.Conditions?.Select(c => c.Clone()).ToArray(),
                Actions = this.Actions?.Select(a => a.Clone()).ToArray(),
                Tags = this.Tags is not null ? new(this.Tags) : null
            };
        }
    }
}
