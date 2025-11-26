using AppManager.Core.Models;
using System;

namespace AppManager.Core.Conditions
{
    public abstract class BaseCondition : ICondition
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsNot { get; set; }

        public abstract ConditionTypeEnum ConditionType { get; }
        public abstract string Description { get; set; }

        public abstract bool Execute();
        public abstract ConditionModel ToModel();

        public BaseCondition(ConditionModel model)
        {
            if (model.ConditionType != ConditionType)
            {
                throw new ArgumentException($"model type '{model.ConditionType}' does not match trigger type '{ConditionType}'.");
            }
            IsNot = model.IsNot;
        }

        protected virtual void LogConditionResult(bool result, string? details = null)
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