using AppManager.Actions;

namespace AppManager.Conditions
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