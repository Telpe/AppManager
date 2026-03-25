using AppManager.Core.Actions;
using AppManager.Core.Actions.BringToFront;
using AppManager.Core.Actions.Close;
using AppManager.Core.Actions.Focus;
using AppManager.Core.Actions.Launch;
using AppManager.Core.Actions.Minimize;
using AppManager.Core.Actions.Restart;
using AppManager.Core.Conditions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AppManager.Core.Models
{
    public class ActionModel : ConditionalModel, ILaunchAction, IMinimizeAction, IFocusAction, ICloseAction, IRestartAction, IBringToFrontAction
    {
        public ActionTypeEnum ActionType { get; set; }
        public string Id { get; set; } = string.Empty;
        public string? AppName { get; set; }
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
                ActionType = this.ActionType,
                Id = this.Id,
                AppName = this.AppName,
                ForceOperation = this.ForceOperation,
                IncludeChildProcesses = this.IncludeChildProcesses,
                IncludeSimilarNames = this.IncludeSimilarNames,
                TimeoutMs = this.TimeoutMs,
                WindowTitle = this.WindowTitle,
                ExecutablePath = this.ExecutablePath,
                Arguments = this.Arguments,
                ProcessLastId = this.ProcessLastId,
                Conditions = this.Conditions?.Select(c => c.Clone()).ToArray()
            };
        }
    }
}