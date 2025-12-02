using AppManager.Core.Triggers;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppManager.Core.Models
{
    public class TriggerModel
    {
        public TriggerTypeEnum TriggerType { get; set; }
        public bool IsActive { get; set; } = false;

        // Shortcut-specific parameters
        public Key? Key { get; set; }
        public ModifierKeys? Modifiers { get; set; }
        public string? ShortcutCombination { get; set; }
        
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
        
        // Additional configuration
        public Dictionary<string, object>? CustomProperties { get; set; }

        public TriggerModel Clone()
        {
            return new()
            {
                TriggerType = TriggerType,
                Key = Key,
                Modifiers = Modifiers,
                ShortcutCombination = ShortcutCombination,
                ProcessName = ProcessName,
                ExecutablePath = ExecutablePath,
                MonitorChildProcesses = MonitorChildProcesses,
                EventName = EventName,
                EventSource = EventSource,
                Port = Port,
                IPAddress = IPAddress,
                PollingIntervalMs = PollingIntervalMs,
                TimeoutMs = TimeoutMs,
                CustomProperties = null != CustomProperties ? new Dictionary<string, object>(CustomProperties) : null
            };

        }
    }
}