using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Core.Actions
{
    public abstract class BaseAction : IAppAction
    {
        private ICondition[] _Conditions = Array.Empty<ICondition>();
        protected readonly ActionModel _Model;

        public abstract string Description { get; }
        public ActionModel Model => _Model;
        public ICondition[] Conditions 
        {
            get { return _Conditions; }
        }

        protected BaseAction(ActionModel model)
        {
            _Model = model ?? throw new ArgumentNullException(nameof(model));
            InitializeConditions();
        }

        private void InitializeConditions()
        {
            if (_Model.Conditions != null && _Model.Conditions.Length > 0)
            {
                var conditions = new List<ICondition>();
                foreach (var conditionModel in _Model.Conditions)
                {
                    var condition = ConditionFactory.CreateCondition(conditionModel);
                    if (condition != null) { conditions.Add(condition); }
                }
                _Conditions = conditions.ToArray();
            }
        }

        public bool CanExecute()
        {
            // Check all conditions first
            if (!CheckConditions()) { return false; }

            // Then check action-specific logic
            return CanExecuteAction();
        }

        public abstract Task<bool> ExecuteAsync();

        protected virtual bool CanExecuteAction() { return true; }

        protected virtual bool CheckConditions()
        {
            if (_Conditions.Length == 0) { return true; }

            foreach (var condition in _Conditions)
            {
                if (!condition.Execute())
                {
                    System.Diagnostics.Debug.WriteLine($"Condition {condition.ConditionType} failed for action {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        public void AddCondition(ConditionModel conditionModel)
        {
            if (null == conditionModel) { return; }

            var condition = ConditionFactory.CreateCondition(conditionModel);
            if (condition != null)
            {
                _Conditions = _Conditions.Append(condition).ToArray();
            }
        }

        public void AddCondition(ICondition condition)
        {
            if (condition != null && !_Conditions.Contains(condition))
            {
                _Conditions = _Conditions.Append(condition).ToArray();
            }
        }

        public void AddConditions(ConditionModel[] conditionModels)
        {
            if (null == conditionModels || conditionModels.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("conditionModels array must be provided and cannot be empty");
                return;
            }

            for (int i = 0; i < conditionModels.Length; i++)
            {
                AddCondition(conditionModels[i]);
            }
        }

        public void AddConditions(ICondition[] conditions)
        {
            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    AddCondition(condition);
                }
            }
        }

        public bool RemoveCondition(ICondition condition)
        {
            if (condition == null) { return false; }

            var conditionArray = _Conditions.ToList();
            bool removed = conditionArray.Remove(condition);
            
            if (removed)
            {
                _Conditions = conditionArray.ToArray();
                
                // Also remove from the model's conditions array
                _Model.RemoveCondition(condition.Model);
            }

            return removed;
        }

        public bool RemoveCondition(int conditionIndex)
        {
            if (conditionIndex < 0 || conditionIndex >= _Conditions.Length)
            {
                return false;
            }

            var conditionToRemove = _Conditions[conditionIndex];
            _Conditions = _Conditions.Where((c, i) => i != conditionIndex).ToArray();
            
            // Also remove from the model's conditions array
            _Model.RemoveCondition(conditionToRemove.Model);
            
            return true;
        }

        public void ClearConditions()
        {
            _Conditions = Array.Empty<ICondition>();
            _Model.ClearConditions();
        }
    }
}