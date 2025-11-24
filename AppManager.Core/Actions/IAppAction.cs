using System.Threading.Tasks;
using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Core.Actions
{
    public interface IAppAction
    {
        string Description { get; }
        ActionModel Model { get; }
        Task<bool> ExecuteActionAsync();
        bool CanExecute();
        
        // Condition management
        ICondition[] Conditions { get; }
        void AddCondition(ConditionModel conditionModel);
        void AddCondition(ICondition condition);
        void AddConditions(ConditionModel[] conditionModels);
        void AddConditions(ICondition[] conditions);
        bool RemoveCondition(ICondition condition);
        void ClearConditions();
    }
}