using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AppManager.Core.Models
{
    public class ActionModel : ILaunchAction, IMinimizeAction, IFocusAction, ICloseAction, IRestartAction, IBringToFrontAction
    {
        public string? AppName { get; set; }
        public AppActionTypeEnum ActionType { get; set; }
        public bool? Inactive { get; set; }
        public bool? ForceOperation { get; set; }
        public bool? IncludeChildProcesses { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public int? TimeoutMs { get; set; }
        public string? WindowTitle { get; set; }
        public string? ExecutablePath { get; set; }
        public string? Arguments { get; set; }

        public ConditionModel[]? Conditions { get; set; }

        public ActionModel() { }

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

        

        public ActionModel Clone()
        {
            return new()
            {
                AppName = this.AppName,
                ActionType = this.ActionType,
                ForceOperation = this.ForceOperation,
                IncludeChildProcesses = this.IncludeChildProcesses,
                IncludeSimilarNames = this.IncludeSimilarNames,
                TimeoutMs = this.TimeoutMs,
                WindowTitle = this.WindowTitle,
                ExecutablePath = this.ExecutablePath,
                Arguments = this.Arguments,
                Conditions = this.Conditions?.Select(c => c.Clone()).ToArray()
            };
        }
    }
}