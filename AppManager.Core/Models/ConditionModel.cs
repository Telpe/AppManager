using AppManager.Core.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace AppManager.Core.Models
{
    public class ConditionModel : IFileExistsCondition, IProcessRunningCondition, IPreviousActionSuccessCondition
    {
        public ConditionTypeEnum ConditionType { get; set; }
        public bool IsNot { get; set; } = false;

        // Process-related parameters
        public string? ProcessName { get; set; }
        public string? ExecutablePath { get; set; }
        public bool? IncludeChildProcesses { get; set; }
        
        // File-related parameters
        public string? FilePath { get; set; }
        
        // Window-related parameters
        public string? WindowTitle { get; set; }
        public string? WindowClassName { get; set; }

        // Network-related parameters
        public int? Port { get; set; }
        public string? IPAddress { get; set; } 
        
        // Time-related parameters
        public System.TimeSpan? StartTime { get; set; }
        public System.TimeSpan? EndTime { get; set; }
        public System.DayOfWeek[]? AllowedDays { get; set; }
        
        // System-related parameters
        public int? MinSystemUptimeMinutes { get; set; }
        public int? MaxSystemUptimeMinutes { get; set; }
        
        // Timeout parameters
        public int? TimeoutMs { get; set; }
        
        // Additional configuration
        public Dictionary<string, object>? CustomProperties { get; set; }

        public ConditionModel()
        {
        }

        public ConditionModel Clone()
        {
            return new ConditionModel
            {
                ConditionType = this.ConditionType,
                IsNot = this.IsNot,
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
                CustomProperties = null != CustomProperties ? new Dictionary<string, object>(CustomProperties) : null
            };
        }
    }
}