using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Core.Actions
{
    public abstract class BaseAction : IAction
    {
        public abstract AppActionTypeEnum ActionType { get; }

        private ICondition[] _Conditions = Array.Empty<ICondition>();

        public abstract string Description { get; }
        public ICondition[] Conditions 
        {
            get => _Conditions;
        }

        public BaseAction(ActionModel model)
        {
            if (model.ActionType != ActionType)
            {
                throw new ArgumentException($"model type '{model.ActionType}' does not match trigger type '{ActionType}'.");
            }

            InitializeConditions(model);
        }

        protected void InitializeConditions(ActionModel model)
        {
            if (model.Conditions != null && model.Conditions.Length > 0)
            {
                var conditions = new List<ICondition>();
                foreach (var conditionModel in model.Conditions)
                {
                    var condition = ConditionFactory.CreateCondition(conditionModel);
                    if (condition != null) { conditions.Add(condition); }
                }
                _Conditions = conditions.ToArray();
            }
        }

        public bool CanExecute()
        {
            return CheckConditions() && CanExecuteAction();
        }

        public abstract ActionModel ToModel();

        protected abstract void ExecuteAction();

        protected virtual bool CanExecuteAction() { return true; }

        protected virtual bool CheckConditions()
        {
            foreach (var condition in _Conditions)
            {
                if (!condition.Execute())
                {
                    Log.WriteLine($"Condition {condition.ConditionType} failed for action {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        public void Execute()
        {
            if (!CanExecute())
            {
                Log.WriteLine($"Action {ActionType} cannot be executed due to failing conditions.");
            }
            else
            {
                ExecuteAction();
            }
        }

        public void AddCondition(ConditionModel conditionModel)
        {
            if (null == conditionModel) { return; }

            var condition = ConditionFactory.CreateCondition(conditionModel);

            AddCondition(condition);
        }

        public void AddCondition(ICondition condition)
        {
            if (null == condition) { return; }

            if (_Conditions.Contains(condition))
            {
                throw new InvalidOperationException("Condition already exists in the action.");
            }

            _Conditions = [.._Conditions, condition];
        }

        public void AddConditions(ConditionModel[] conditionModels)
        {
            if (null == conditionModels) { return; }
            
            foreach (var conditionModel in conditionModels)
            {
                AddCondition(conditionModel);
            }
            
        }

        public void AddConditions(ICondition[] conditions)
        {
            if (null == conditions) { return; }
            
            foreach(ICondition condition in conditions)
            {
                AddCondition(condition);
            }
            
        }

        public bool RemoveCondition(ICondition condition)
        {
            int conditions = _Conditions.Length;
            _Conditions = _Conditions.Where(c => c != condition).ToArray();
            return _Conditions.Length < conditions;
        }

        public bool RemoveCondition(int conditionIndex)
        {
            int conditions = _Conditions.Length;
            _Conditions = _Conditions.Where((c,i) => i != conditionIndex).ToArray();
            return _Conditions.Length < conditions;
        }

        public void ClearConditions()
        {
            _Conditions = Array.Empty<ICondition>();
        }

    }
}