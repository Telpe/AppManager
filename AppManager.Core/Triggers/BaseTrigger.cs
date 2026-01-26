using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Keybinds;
using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace AppManager.Core.Triggers
{
    public abstract class BaseTrigger : ITrigger
    {
        public abstract TriggerTypeEnum TriggerType { get; }
        public string Name { get; set; }
        public virtual string Description { get; set; } = "Trigger from outside events";
        public bool Inactive { get; set; }

        protected ICondition[] ConditionsValue { get; set; } = [];
        public ICondition[]? Conditions 
        {
            get => 0 == ConditionsValue.Length ? null : ConditionsValue;
            set => ConditionsValue = value ?? [];
        }
        protected IAction[] ActionsValue { get; set; } = [];
        public IAction[] Actions 
        {
            get => ActionsValue;
            set => ActionsValue = value;
        }

        public event EventHandler? TriggerActivated;

        protected BaseTrigger(TriggerModel model)
        {
            if (model.TriggerType != TriggerType)
            {
                throw new ArgumentException($"model type '{model.TriggerType}' does not match trigger type '{TriggerType}'.");
            }

            Name = TriggerType.ToString();
            Inactive = model.Inactive ?? false;
            InitializeConditions(model);
            InitializeActions(model);
        }

        protected void InitializeConditions(TriggerModel model)
        {
            if (model.Conditions != null && model.Conditions.Length > 0)
            {
                var conditions = new List<ICondition>();
                foreach (var conditionModel in model.Conditions)
                {
                    ICondition condition = ConditionFactory.CreateCondition(conditionModel);
                    conditions.Add(condition);
                }
                ConditionsValue = conditions.ToArray();
            }
        }

        protected void InitializeActions(TriggerModel model)
        {
            if (null == model.Actions) { return; }

            ActionsValue = model.Actions.Select(a => ActionFactory.CreateAction(a)).ToArray();
        }

        public abstract TriggerModel ToModel();
        public abstract void Start();
        public abstract void Stop();
        protected virtual bool CanStartTrigger() { return true; }
        public bool CanStart() 
        {
            return !Inactive && CanStartTrigger();
        }
        protected virtual bool CanExecuteTrigger() { return true; }
        public bool CanExecute()
        {
            if (Inactive || !CheckConditions()) { return false; }

            return CanExecuteTrigger();
        }

        public bool Execute()
        {
            if (!CanExecute()) { return false; }
            Task.Run(ExecuteActionsAsync).Wait();
            return true;
        }

        protected async void ExecuteActionsAsync()
        {
            Log.WriteLine($"Thread id: {GlobalKeyboardHook.CurrentThreadId}");
            if (!CanExecute()) { return; }

            bool actionSuccess;
            int nextIndex = 1;

            foreach (var action in ActionsValue)
            {
                Log.WriteLine($"Executing action {action.ActionType} for trigger {GetType().Name}");
                try
                {
                    actionSuccess = action.Execute();
                }
                catch(Exception e)
                {
                    Log.WriteLine($"Action {action.ActionType} execution failed for trigger {GetType().Name}.\n{e.Message}\n");
                    actionSuccess = false;
                }

                if (nextIndex < ActionsValue.Length )
                {
                    SetActionSuccessForActionConditions(actionSuccess, ActionsValue[nextIndex]);
                }

                nextIndex++;
            }

        }

        /// <summary>
        /// Helper method to convert this Trigger to a TriggerModel of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new ConditionModel with values from this that match T</returns>
        protected TriggerModel ToTriggerModel<T>()
        {
            var model = new TriggerModel
            {
                TriggerType = this.TriggerType,
                Inactive = this.Inactive ? this.Inactive : null,
                Conditions = Conditions?.Select(c => c.ToModel()).ToArray(),
                Actions = Actions.Select(a => a.ToModel()).ToArray()
            };

            if (model is not T || this is not T)
            {
                throw new Exception("TriggerModel and Trigger must both inherit T");
            }

            foreach (PropertyInfo property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(model, property.GetValue(this, null), null);
            }

            return model;
        }

        private void SetActionSuccessForActionConditions(bool actionSuccess, IAction action)
        {
            foreach (var condition in action.Conditions)
            {
                if (condition is PreviousActionSuccessCondition pasc)
                {
                    pasc.ActionSucceeded = actionSuccess;
                }
            }
        }

        protected virtual bool CheckConditions()
        {
            foreach (var condition in ConditionsValue)
            {
                if (condition.IsNot ? condition.Execute() : !condition.Execute())
                {
                    Log.WriteLine($"Condition {condition.ConditionType} failed for trigger {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        protected void ActivateTrigger()
        {
            ExecuteActionsAsync();
            TriggerActivated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            Stop();
        }

    }
}