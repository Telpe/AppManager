using AppManager.Core.Models;
using AppManager.Core.Utils;

namespace AppManager.Core.Conditions
{
    public class ProcessRunningCondition : BaseCondition, IProcessRunningCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.ProcessRunning;
        public override string Description { get; set; } = "Checks if a specific process is currently running";
        public string? ProcessName { get; set; }

        public ProcessRunningCondition(ConditionModel model) : base(model)
        {
            ProcessName = model.ProcessName;
        }

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrEmpty(ProcessName))
                {
                    LogConditionResult(false, "No process name specified");
                    return false;
                }

                bool isRunning = ProcessManager.IsProcessRunning(ProcessName);

                //LogConditionResult(isRunning, $"Process '{ProcessName}' running: {isRunning}");
                return isRunning;
            }
            catch (System.Exception ex)
            {
                LogConditionResult(false, $"Error checking process: '{ProcessName}'\n{ex.Message}");
                return false;
            }
        }

        public override ConditionModel ToModel()
        {
            return new ConditionModel
            {
                ConditionType = ConditionTypeEnum.ProcessRunning,
                IsNot = IsNot,
                ProcessName = ProcessName
            };
        }
    }
}