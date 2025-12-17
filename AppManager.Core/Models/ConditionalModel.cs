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

        /// <summary>
        /// Adds a condition to the action. Initializes the Conditions array if null.
        /// </summary>
        /// <param name="condition">The condition to add</param>
        public void AddCondition(ConditionModel condition)
        {
            if (condition == null)
                return;

            if (Conditions == null)
            {
                Conditions = new[] { condition };
            }
            else
            {
                Conditions = Conditions.Append(condition).ToArray();
            }
        }

        /// <summary>
        /// Removes the first occurrence of the specified condition from the action.
        /// </summary>
        /// <param name="condition">The condition to remove</param>
        /// <returns>True if the condition was found and removed, false otherwise</returns>
        public bool RemoveCondition(ConditionModel condition)
        {
            if (condition == null || Conditions == null || Conditions.Length == 0) { return false; }

            int initialCount = Conditions.Length;

            Conditions = Conditions.Where(c => c != condition).ToArray();

            return Conditions.Length < initialCount;
        }

        /// <summary>
        /// Removes a condition at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the condition to remove</param>
        /// <returns>True if the condition was removed, false if the index was invalid</returns>
        public bool RemoveConditionAt(int index)
        {
            if (Conditions == null || index < 0 || index >= Conditions.Length) { return false; }

            int initialCount = Conditions.Length;
            Conditions = Conditions.Where((_, i) => i != index).ToArray();

            return Conditions.Length < initialCount;
        }

        /// <summary>
        /// Removes all conditions that match the specified condition type.
        /// </summary>
        /// <param name="conditionType">The type of conditions to remove</param>
        /// <returns>The number of conditions removed</returns>
        public int RemoveConditionsByType(ConditionTypeEnum conditionType)
        {
            if (Conditions == null || Conditions.Length == 0)
                return 0;

            var initialCount = Conditions.Length;
            Conditions = Conditions.Where(c => c.ConditionType != conditionType).ToArray();

            return initialCount - Conditions.Length;
        }

        /// <summary>
        /// Removes all conditions from the action.
        /// </summary>
        public void ClearConditions()
        {
            Conditions = Array.Empty<ConditionModel>();
        }


    }
}