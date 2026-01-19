using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Keybinds;
using AppManager.Core.Models;
using System;
using System.Diagnostics;
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
        public ICondition[] Conditions 
        {
            get => ConditionsValue;
            set => ConditionsValue = value;
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

            ActionsValue = model.Actions.Select(a => ActionManager.CreateAction(a)).ToArray();
        }

        public virtual TriggerModel ToModel()
        {
            var model = new TriggerModel
            {
                TriggerType = TriggerType,
                Inactive = Inactive,
                Conditions = ConditionsValue.Select(c => c.ToModel()).ToArray(),
                Actions = ActionsValue.Select(a => a.ToModel()).ToArray()
            };
            return model;
        }
        public abstract void Start();
        public abstract void Stop();
        protected virtual bool CanStartTrigger() { return true; }
        public bool CanStart() 
        {
            if (Inactive) { return false; }

            return CanStartTrigger();
        }
        protected virtual bool CanExecuteTrigger() { return true; }
        public bool CanExecute()
        {
            if (Inactive || !CheckConditions()) { return false; }

            return CanExecuteTrigger();
        }

        protected async void ExecuteActions()
        {
            Log.WriteLine($"Thread id should be: {GlobalKeyboardHook.CurrentThreadId}");
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
                if (!condition.Execute())
                {
                    Log.WriteLine($"Condition {condition.ConditionType} failed for trigger {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        protected void ActivateTrigger()
        {
            ExecuteActions();
            TriggerActivated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            Stop();
        }

    }
}