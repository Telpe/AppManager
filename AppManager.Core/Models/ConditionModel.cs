using AppManager.Core.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace AppManager.Core.Models
{
    public class ConditionModel
    {
        public ConditionTypeEnum ConditionType { get; set; }
        public bool IsNot { get; set; } = false;

        // Process-related parameters
        public string ProcessName { get; set; }
        public string ExecutablePath { get; set; }
        public bool IncludeChildProcesses { get; set; } = false;
        
        // File-related parameters
        public string FilePath { get; set; }
        
        // Window-related parameters
        public string WindowTitle { get; set; }
        public string WindowClassName { get; set; }

        // Network-related parameters
        public int Port { get; set; } = 9011;
        public string IPAddress { get; set; } = "127.0.0.1";
        
        // Time-related parameters
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public System.DayOfWeek[] AllowedDays { get; set; }
        
        // System-related parameters
        public int MinSystemUptimeMinutes { get; set; }
        public int MaxSystemUptimeMinutes { get; set; }
        
        // Timeout parameters
        public int TimeoutMs { get; set; } = 5000;
        
        // Additional configuration
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

        public ConditionModel()
        {
        }

        public ConditionModel Clone()
        {
            return new ConditionModel
            {
                ConditionType = this.ConditionType,
                ProcessName = this.ProcessName,
                ExecutablePath = this.ExecutablePath,
                IncludeChildProcesses = this.IncludeChildProcesses,
                FilePath = this.FilePath,
                WindowTitle = this.WindowTitle,
                WindowClassName = this.WindowClassName,
                Port = this.Port,
                IPAddress = this.IPAddress,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                AllowedDays = this.AllowedDays?.ToArray(),
                MinSystemUptimeMinutes = this.MinSystemUptimeMinutes,
                MaxSystemUptimeMinutes = this.MaxSystemUptimeMinutes,
                TimeoutMs = this.TimeoutMs,
                CustomProperties = new Dictionary<string, object>(this.CustomProperties)
            };
        }
    }
}