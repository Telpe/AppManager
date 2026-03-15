using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Conditions
{
    public interface ICondition
    {
        string Id { get; }
        ConditionTypeEnum ConditionType { get; }
        bool IsNot { get; set; }
        string Description { get; set; }
        bool Execute();
        ConditionModel ToModel();
    }
}