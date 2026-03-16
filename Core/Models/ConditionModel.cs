using AppManager.Core.Conditions;
using AppManager.Core.Conditions.FileExists;
using AppManager.Core.Conditions.PreviousActionSucceeded;
using AppManager.Core.Conditions.ProcessIsRunning;
using System.Collections.Generic;
using System.Linq;

namespace AppManager.Core.Models
{
    public class ConditionModel : IFileExistsCondition, IProcessIsRunningCondition, IPreviousActionSucceededCondition
    {
        public ConditionTypeEnum ConditionType { get; set; }
        public bool IsNot { get; set; } = false;

        public string Id { get; set; } = string.Empty;

        // Process-related parameters
        public string? ProcessName { get; set; }
        public bool? IncludeChildProcesses { get; set; }
        
        // File-related parameters
        public string? FilePath { get; set; }
        
        // Window-related parameters
        public string? WindowTitle { get; set; }

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
                Id = this.Id,
                ProcessName = this.ProcessName,
                IncludeChildProcesses = this.IncludeChildProcesses,
                FilePath = this.FilePath,
                WindowTitle = this.WindowTitle,
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