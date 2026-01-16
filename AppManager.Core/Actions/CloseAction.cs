using AppManager.Core.Models;
using AppManager.Core.Utilities;
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
            Process[]? processes = null;
            try
            {
                processes = ProcessManager.FindProcesses(
                    AppName!, 
                    IncludeSimilarNames ?? false, 
                    requireMainWindow: false,
                    windowTitle: null, 
                    IncludeChildProcesses ?? true,
                    4);
                
                if (null == processes || 0 == processes.Length)
                {
                    Log.WriteLine($"No processes found for: {AppName}");
                    return;
                }

                foreach(Process p in processes.Where(a => !String.Equals(a.ProcessName, AppName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    p.Dispose();
                }

                processes = processes.Where(a=> String.Equals(a.ProcessName, AppName, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                Log.WriteLine($"Closing {processes.Length} process{(processes.Length!=1?"es":"")} for: {AppName}");

                if (!ProcessManager.CloseProcesses(processes, TimeoutMs ?? CoreConstants.DefaultActionDelay, ForceOperation ?? true))
                {
                    throw new Exception($"Failed to close all processes for: {AppName}");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Failed to close {AppName}: {ex.Message}");
            }
            finally
            {
                if (null != processes)
                {
                    foreach (Process p in processes)
                    {
                        p.Dispose();
                    }
                }
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