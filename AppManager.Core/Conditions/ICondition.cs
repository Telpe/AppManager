using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Conditions
{
    public interface ICondition
    {
        string Id { get; }
        ConditionTypeEnum ConditionType { get; }
        string Description { get; }
        ConditionModel Model { get; set; }
        bool Execute();
    }
}