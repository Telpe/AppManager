using AppManager.Core.Conditions;

namespace AppManager.Core.Models
{
    /// <summary>
    /// Interface for models that can have conditions and inactive status.
    /// </summary>
    public class ConditionalModel
    {
        public bool? Inactive { get; set; }
        public ConditionModel[]? Conditions { get; set; }

        public static string GetDisplayText(ConditionModel model)
        {
            return model.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process Running: {model.ProcessName}",
                ConditionTypeEnum.FileExists => $"File Exists: {model.FilePath}",
                ConditionTypeEnum.PreviousActionSuccess => $"Previous action result. Only when trigger has multiple actions.",
                _ => $"Unknown Condition: {model.ConditionType}"
            };
        }

        /// <summary>
        /// Adds a condition to the action. Initializes the Conditions array if null.
        /// </summary>
        /// <param name="condition">The condition to add</param>
        public void AddCondition(ConditionModel condition)
        {
            Conditions = Conditions is null ? [condition] : [..Conditions, condition];
        }

        /// <summary>
        /// Removes the first occurrence of the specified condition from the action.
        /// </summary>
        /// <param name="condition">The condition to remove</param>
        /// <returns>True if the condition was found and removed, false otherwise</returns>
        public bool RemoveCondition(ConditionModel condition)
        {
            if (Conditions is null || Conditions.Length == 0) { return false; }

            int initialCount = Conditions.Length;

            Conditions = Conditions.Where(c => c != condition).ToArray();

            NullifyZeroLengthConditions();
            return (Conditions?.Length ?? 0) < initialCount;
        }

        /// <summary>
        /// Removes a condition at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the condition to remove</param>
        /// <returns>True if the condition was removed, false if the index was invalid</returns>
        public bool RemoveConditionAt(int index)
        {
            if (null == Conditions || index < 0 || index >= Conditions.Length) { return false; }

            int initialCount = Conditions.Length;
            Conditions = Conditions.Where((_, i) => i != index).ToArray();

            NullifyZeroLengthConditions();
            return (Conditions?.Length ?? 0) < initialCount;
        }

        /// <summary>
        /// Removes all conditions that match the specified condition type.
        /// </summary>
        /// <param name="conditionType">The type of conditions to remove</param>
        /// <returns>The number of conditions removed</returns>
        public int RemoveConditionsByType(ConditionTypeEnum conditionType)
        {
            if (null == Conditions || Conditions.Length == 0) { return 0; }

            int initialCount = Conditions.Length;

            Conditions = Conditions.Where(c => c.ConditionType != conditionType).ToArray();

            NullifyZeroLengthConditions();
            return initialCount - (Conditions?.Length ?? 0);
        }

        /// <summary>
        /// Removes all conditions from the model.
        /// </summary>
        public void ClearConditions()
        {
            Conditions = null;
        }

        protected void NullifyZeroLengthConditions()
        {
            if (false != (Conditions?.Length == 0))
            {
                Conditions = null;
            }
        }
    }
}