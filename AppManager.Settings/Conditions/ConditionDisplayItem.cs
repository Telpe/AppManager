using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Settings.Conditions
{
    public class ConditionDisplayItem
    {
        public ConditionModel Model { get; }
        public string DisplayText { get; }

        public ConditionDisplayItem(ConditionModel model)
        {
            Model = model;
            DisplayText = GetDisplayText(model);
        }

        private string GetDisplayText(ConditionModel model)
        {
            return model.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process Running: {model.ProcessName}",
                ConditionTypeEnum.FileExists => $"File Exists: {model.FilePath}",
                ConditionTypeEnum.PreviousActionSuccess => $"Previous action result. Only when trigger has multiple actions.",
                _ => $"Unknown Condition: {model.ConditionType}"
            };
        }
    }
}