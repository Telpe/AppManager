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

        protected override bool ExecuteAction()
        {
            Process[]? processes = null;
            Process[]? processesTemp = null;
            try
            {
                processesTemp = ProcessManager.FindProcesses(
                    AppName!, 
                    IncludeSimilarNames ?? false, 
                    requireMainWindow: false,
                    windowTitle: null, 
                    IncludeChildProcesses ?? false,
                    4);
                
                if (processesTemp is null || 0 == processesTemp.Length)
                {
                    Log.WriteLine($"No processes found for: {AppName}");
                    return false;
                }

                processes = [];

                foreach (Process p in processesTemp)
                {
                    if (String.Equals(p.ProcessName, AppName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        processes = processes.Append(p).ToArray();
                    }
                    else
                    {
                        p.Dispose();
                    }
                    
                }

                Log.WriteLine($"Closing {processes.Length} process{(processes.Length != 1 ?"es":"")} for: {AppName}");

                if (!ProcessManager.CloseProcesses(processes, TimeoutMs ?? CoreConstants.ProcessRestartDelay, ForceOperation ?? true))
                {
                    throw new Exception($"Failed to close all processes for: {AppName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Failed to close {AppName}: {ex.Message}");
                return false;
            }
            finally
            {
                if (processes is not null)
                {
                    foreach (Process p in processes)
                    {
                        Log.WriteLine($"Disposing process: {p.ProcessName} (ID: {p.Id})");
                        p.Dispose();
                    }
                }
                if (processesTemp is not null)
                {
                    foreach (Process p in processesTemp)
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