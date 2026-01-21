using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AppManager.Core.Models
{
    public class ActionModel : ConditionalModel, ILaunchAction, IMinimizeAction, IFocusAction, ICloseAction, IRestartAction, IBringToFrontAction
    {
        public string? AppName { get; set; }
        public AppActionTypeEnum ActionType { get; set; }
        public bool? ForceOperation { get; set; }
        public bool? IncludeChildProcesses { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public int? TimeoutMs { get; set; }
        public string? WindowTitle { get; set; }
        public string? ExecutablePath { get; set; }
        public string? Arguments { get; set; }
        public int? ProcessLastId { get; set; }

        public ActionModel Clone()
        {
            return new()
            {
                AppName = this.AppName,
                ActionType = this.ActionType,
                ForceOperation = this.ForceOperation,
                IncludeChildProcesses = this.IncludeChildProcesses,
                IncludeSimilarNames = this.IncludeSimilarNames,
                TimeoutMs = this.TimeoutMs,
                WindowTitle = this.WindowTitle,
                ExecutablePath = this.ExecutablePath,
                Arguments = this.Arguments,
                Conditions = this.Conditions?.Select(c => c.Clone()).ToArray()
            };
        }
    }
}