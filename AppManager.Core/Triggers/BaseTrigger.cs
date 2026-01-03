using System;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    public abstract class BaseTrigger : ITrigger
    {
        public abstract TriggerTypeEnum TriggerType { get; }
        public string Name { get; set; }
        public string Description { get; set; } = "";
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

        public event EventHandler? OnTriggerActivated;

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

        protected void ExecuteActions()
        {
            foreach (var action in ActionsValue)
            {
                if (action.CanExecute())
                {
                    System.Diagnostics.Debug.WriteLine($"Executing action {action.ActionType} for trigger {GetType().Name}");
                    _ = action.ExecuteAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Action {action.ActionType} cannot be executed for trigger {GetType().Name}");
                }
            }

        }

        protected virtual bool CheckConditions()
        {
            foreach (var condition in ConditionsValue)
            {
                if (!condition.Execute())
                {
                    System.Diagnostics.Debug.WriteLine($"Condition {condition.ConditionType} failed for trigger {GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        protected void TriggerActivated()
        {
            if (CanExecute()) { ExecuteActions(); }

            OnTriggerActivated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            Stop();
        }

    }
}