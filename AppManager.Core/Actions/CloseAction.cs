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

        protected override void ExecuteAction()
        {
            try
            {
                Process[] processes = ProcessManager.FindProcesses(
                    AppName, 
                    IncludeSimilarNames ?? false, 
                    windowTitle: null, 
                    requireMainWindow: false, 
                    IncludeChildProcesses ?? true);
                
                if (!processes.Any())
                {
                    Log.WriteLine($"No processes found for: {AppName}");
                    return;
                }

                Log.WriteLine($"Closing {processes.Length} process{(processes.Length!=1?"es":"")} for: {AppName}");

                if (!ProcessManager.CloseProcesses(processes, TimeoutMs ?? 3000, ForceOperation ?? true))
                {
                    throw new Exception($"Failed to close all processes for: {AppName}");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Failed to close {AppName}: {ex.Message}");
            }
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