using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace AppManager.Core.Models
{
    public class TriggerModel : ConditionalModel, IAppCloseTrigger, IAppLaunchTrigger, IKeybindTrigger, ISystemEventTrigger, INetworkPortTrigger, IButtonTrigger
    {
        public TriggerTypeEnum TriggerType { get; set; }
        public string Id { get; set; } = string.Empty;

        // Shortcut-specific parameters
        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
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
                Key = Key,
                Modifiers = Modifiers,
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