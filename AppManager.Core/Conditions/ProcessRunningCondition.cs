using System.Diagnostics;
using System.Linq;

namespace AppManager.Core.Conditions
{
    public class ProcessRunningCondition : BaseCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.ProcessRunning;
        public override string Description => "Checks if a specific process is currently running";

        public override bool Execute()
        {
            try
            {
                var targetProcess = Model?.ProcessName;
                if (string.IsNullOrEmpty(targetProcess))
                {
                    LogConditionResult(false, "No process name specified");
                    return false;
                }

                var processes = Process.GetProcessesByName(targetProcess);
                bool isRunning = processes.Any();

                if (isRunning && Model.IncludeChildProcesses)
                {
                    // Additional logic for child processes could be added here
                }

                LogConditionResult(isRunning, $"Process '{targetProcess}' running: {isRunning}");
                return isRunning;
            }
            catch (System.Exception ex)
            {
                LogConditionResult(false, $"Error checking process: {ex.Message}");
                return false;
            }
        }
    }
}