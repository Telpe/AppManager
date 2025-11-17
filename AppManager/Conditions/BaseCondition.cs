using System;

namespace AppManager.Conditions
{
    public abstract class BaseCondition : ICondition
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public abstract ConditionTypeEnum ConditionType { get; }
        public abstract string Description { get; }
        public ConditionModel Model { get; set; }

        public abstract bool Execute();

        protected virtual void LogConditionResult(bool result, string details = null)
        {
            var message = $"Condition {ConditionType} result: {result}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" - {details}";
            }
            
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}