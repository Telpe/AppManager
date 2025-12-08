using AppManager.Core.Models;
using AppManager.Core.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class CloseAction : BaseAction, ICloseAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Close;
        public override string Description => "Closes an application gracefully";

        public int? TimeoutMs { get; set; }
        public bool? ForceOperation { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public bool? IncludeChildProcesses { get; set; }

        public CloseAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeChildProcesses = model.IncludeChildProcesses;
            IncludeSimilarNames = model.IncludeSimilarNames;
            TimeoutMs = model.TimeoutMs;
            ForceOperation = model.ForceOperation;
        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    var processes = ProcessManager.FindProcesses(
                        AppName, 
                        IncludeSimilarNames ?? false, 
                        windowTitle: null, 
                        requireMainWindow: false, 
                        IncludeChildProcesses ?? false);
                
                    if (!processes.Any())
                    {
                        Debug.WriteLine($"No processes found for: {AppName}");
                        return false;
                    }

                    return ProcessManager.CloseProcesses(processes, TimeoutMs ?? 1000, ForceOperation ?? false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to close {AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        public override ActionModel ToModel()
        {
            return new ActionModel()
            {
                AppName = AppName,
                ActionType = ActionType,
                IncludeChildProcesses = IncludeChildProcesses,
                IncludeSimilarNames = IncludeSimilarNames,
                TimeoutMs = TimeoutMs,
                ForceOperation = ForceOperation,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}