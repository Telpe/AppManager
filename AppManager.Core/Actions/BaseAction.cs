using AppManager.Core.Conditions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public abstract class BaseAction : IAction
    {
        public abstract AppActionTypeEnum ActionType { get; }

        private ICondition[] ConditionsValue = Array.Empty<ICondition>();

        public abstract string Description { get; }
        public ICondition[] Conditions 
        {
            get => ConditionsValue;
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
                    conditions.Add(ConditionFactory.CreateCondition(conditionModel)); 
                }
                ConditionsValue = conditions.ToArray();
            }
        }

        /// <summary>
        /// <para>Make sure to create this to convert your action back to its model representation.<br />
        /// Use ToActionModel<T> helper class to simplify the process.<br />
        /// Default values should mostly be set to null to reduce storage size and readability, but not if default is likely to change.</para>
        /// The model is used for serialization and storage and should not stay loaded in memory.
        /// </summary>
        public abstract ActionModel ToModel();

        /// <summary>
        /// Helper method to convert this Action to an ActionModel of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new ActionModel with values from this that match T</returns>
        protected ActionModel ToActionModel<T>()
        {
            var model = new ActionModel
            {
                ActionType = this.ActionType,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };

            if (model is not T || this is not T)
            {
                throw new Exception("ActionModel and Action must both inherit T");
            }

            foreach (PropertyInfo property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(model, property.GetValue(this, null), null);
            }

            return model;
        }

        /// <summary>
        /// Write your action execution code here.
        /// </summary>
        /// <returns>bool indicating if action completed as expected.<br />
        /// Value can be used as Condition for next action in line.</returns>
        protected abstract bool ExecuteAction();

        /// <summary>
        /// Optional override to predetermine if action can execute.<br />
        /// Typically used to validate parameters.
        /// </summary>
        /// <returns>Always true, if not overriding.</returns>
        protected virtual bool CanExecuteAction() { return true; }

        /// <summary>
        /// <para>! DO NOT OVERRIDE !</para>
        /// Unless you really need to.<br />
        /// This method check all conditions and is called just before executing the action.
        /// </summary>
        /// <returns>true if all conditions are true, false is any condition is false.</returns>
        protected virtual bool CheckConditions()
        {
            foreach (var condition in ConditionsValue)
            {
                if (condition.IsNot ? condition.Execute() : !condition.Execute())
                {
                    Log.WriteLine($"Condition {condition.ConditionType} failed for action {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if action can be executed by validating user and actionspecific conditions.
        /// </summary>
        /// <returns>bool</returns>
        public bool CanExecute()
        {
            return CheckConditions() && CanExecuteAction();
        }

        /// <summary>
        /// Validates CanExecute and then executes the action.
        /// </summary>
        /// <returns>bool, value can be used as Condition for next action in line.</returns>
        public bool Execute()
        {
            if (!CanExecute())
            {
                Log.WriteLine($"Action {ActionType} cannot be executed due to failing conditions.");
                return false;
            }

            return ExecuteAction();
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

            if (ConditionsValue.Contains(condition))
            {
                throw new InvalidOperationException("Condition already exists in the action.");
            }

            ConditionsValue = [..ConditionsValue, condition];
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
            int conditions = ConditionsValue.Length;
            ConditionsValue = ConditionsValue.Where(c => c != condition).ToArray();
            return ConditionsValue.Length < conditions;
        }

        public bool RemoveCondition(int conditionIndex)
        {
            int conditions = ConditionsValue.Length;
            ConditionsValue = ConditionsValue.Where((c,i) => i != conditionIndex).ToArray();
            return ConditionsValue.Length < conditions;
        }

        public void ClearConditions()
        {
            ConditionsValue = Array.Empty<ICondition>();
        }

    }
}