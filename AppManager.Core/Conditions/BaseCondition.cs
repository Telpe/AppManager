using AppManager.Core.Models;
using System;
using System.Reflection;

namespace AppManager.Core.Conditions
{
    public abstract class BaseCondition : ICondition
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsNot { get; set; } = false;

        public abstract ConditionTypeEnum ConditionType { get; }
        public abstract string Description { get; set; }

        public abstract bool Execute();
        public abstract ConditionModel ToModel();

        /// <summary>
        /// Helper method to convert this Condition to a ConditionModel of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new ConditionModel with values from this that match T</returns>
        protected ConditionModel ToConditionModel<T>()
        {
            var model = new ConditionModel
            {
                ConditionType = this.ConditionType,
                IsNot = this.IsNot,
            };

            if (model is not T || this is not T)
            {
                throw new Exception("ConditionModel and Condition must both inherit T");
            }

            foreach (PropertyInfo property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(model, property.GetValue(this, null), null);
            }

            return model;
        }

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
            
            Log.WriteLine(message);
        }

    }
}