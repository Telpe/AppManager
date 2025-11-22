using System.Diagnostics;
using System.Linq;

namespace AppManager.Core.Conditions
{
    public class ProcessNotRunningCondition : BaseCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.ProcessNotRunning;
        public override string Description => "Checks if a specific process is NOT currently running";

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
                bool isNotRunning = !processes.Any();

                LogConditionResult(isNotRunning, $"Process '{targetProcess}' not running: {isNotRunning}");
                return isNotRunning;
            }
            catch (System.Exception ex)
            {
                LogConditionResult(false, $"Error checking process: {ex.Message}");
                return false;
            }
        }
    }
}