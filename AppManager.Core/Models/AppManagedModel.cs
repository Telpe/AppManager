using AppManager.Core.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppManager.Core.Models
{
    public class AppManagedModel
    {
        public string AppName { get; set; }
        public bool Active { get; set; }
        
        public Dictionary<int, TriggerModel>? Triggers { get; set; }


        public AppManagedModel(string appName, bool active)
        {
            AppName = appName;
            Active = active;
        }

        public AppManagedModel Clone()
        {
            return new(this.AppName, this.Active)
            {
                Triggers = this.Triggers?.Select(kvp => new KeyValuePair<int, TriggerModel>(kvp.Key, kvp.Value.Clone())).ToDictionary()
            };
        }

        /// <summary>
        /// Adds a trigger to the action. Initializes the triggers if null.
        /// </summary>
        /// <param name="trigger">The trigger to add</param>
        public int AddTrigger(TriggerModel trigger)
        {
            if (null == Triggers || Triggers.Count == 0)
            {
                Triggers = new() { { 1, trigger } };
            }
            else
            {
                Triggers.Add(Triggers.Keys.Max() + 1, trigger);
            }

            return Triggers.Keys.Max();
        }

        /// <summary>
        /// Removes the first occurrence of the specified trigger from the action.
        /// </summary>
        /// <param name="trigger">The trigger to remove</param>
        /// <returns>True if the trigger was found and removed, false otherwise</returns>
        public bool RemoveTrigger(TriggerModel trigger)
        {
            if (null == Triggers || Triggers.Count == 0) { return false; }

            int initialCount = Triggers.Count;

            Triggers = Triggers.Where(c => c.Value != trigger).ToDictionary();

            NullifyZeroLengthTriggers();
            return (Triggers?.Count ?? 0) < initialCount;
        }

        /// <summary>
        /// Removes a trigger at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the trigger to remove</param>
        /// <returns>True if the trigger was removed, false if the index was invalid</returns>
        public bool RemoveTriggerAt(int index)
        {
            if (null == Triggers || index < 0 || index >= Triggers.Count) { return false; }

            int initialCount = Triggers.Count;
            Triggers = Triggers.Where(a => a.Key != index).ToDictionary();

            NullifyZeroLengthTriggers();
            return (Triggers?.Count ?? 0) < initialCount;
        }

        /// <summary>
        /// Removes all triggers that match the specified trigger type.
        /// </summary>
        /// <param name="triggerType">The type of triggers to remove</param>
        /// <returns>The number of triggers removed</returns>
        public int RemoveTriggersByType(TriggerTypeEnum triggerType)
        {
            if (null == Triggers || Triggers.Count == 0) { return 0; }

            int initialCount = Triggers.Count;

            Triggers = Triggers.Where(c => c.Value.TriggerType != triggerType).ToDictionary();

            NullifyZeroLengthTriggers();
            return initialCount - (Triggers?.Count ?? 0);
        }

        /// <summary>
        /// Removes all triggers from the model.
        /// </summary>
        public void ClearTriggers()
        {
            Triggers = null;
        }

        protected void NullifyZeroLengthTriggers()
        {
            if (false != (Triggers?.Count == 0))
            {
                Triggers = null;
            }
        }
    }
}
