using System.Collections.Generic;
using System.Windows.Input;

namespace AppManager.Core.Triggers
{
    public class TriggerModel
    {

        public TriggerTypeEnum TriggerType { get; set; }

        // Shortcut-specific parameters
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public string ShortcutCombination { get; set; }
        
        // App monitoring parameters
        public string ProcessName { get; set; }
        public string ExecutablePath { get; set; }
        public bool MonitorChildProcesses { get; set; } = false;
        
        // System event parameters
        public string EventName { get; set; }
        public string EventSource { get; set; }
        
        // Network parameters
        public int Port { get; set; }
        public string IPAddress { get; set; } = "127.0.0.1";
        
        // Polling parameters
        public int PollingIntervalMs { get; set; } = 1000;
        public int TimeoutMs { get; set; } = 30000;
        
        // Additional configuration
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

        public TriggerModel Clone()
        {
            return new()
            {
                TriggerType = this.TriggerType,
                Key = this.Key,
                Modifiers = this.Modifiers,
                ShortcutCombination = this.ShortcutCombination,
                ProcessName = this.ProcessName,
                ExecutablePath = this.ExecutablePath,
                MonitorChildProcesses = this.MonitorChildProcesses,
                EventName = this.EventName,
                EventSource = this.EventSource,
                Port = this.Port,
                IPAddress = this.IPAddress,
                PollingIntervalMs = this.PollingIntervalMs,
                TimeoutMs = this.TimeoutMs,
                CustomProperties = new Dictionary<string, object>(this.CustomProperties)
            };

        }
    }
}