using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AppManager.Core.Triggers
{
    public static class TriggerManager
    {
        private static ITrigger[] TriggersValue = [];
        public static ITrigger[] Triggers { get => TriggersValue; }

        public static bool RegisterTrigger(TriggerModel trigger)
        {
            return RegisterTrigger(TriggerFactory.CreateTrigger(trigger));
        }

        public static bool RegisterTrigger(ITrigger trigger)
        { 
            try{
                if (TriggersValue.Contains(trigger)) { throw new Exception("Error: Attempt to add duplicate trigger."); }

                if (trigger.CanStart())
                {
                    trigger.Start();
                }

                TriggersValue = TriggersValue.Append(trigger).ToArray();

                Log.WriteLine($"Trigger '{trigger.Name}' registered successfully");

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error registering trigger '{trigger.Name}': {ex.Message}");
                return false;
            }
        }

        public static void UnregisterTrigger(string triggerName)
        {
            IEnumerable<ITrigger> triggers = TriggersValue.Where(a=> a.Name == triggerName);
            TriggersValue = TriggersValue.Where(a => a.Name != triggerName).ToArray();

            foreach (ITrigger trigger in triggers)
            {
                trigger.Dispose();
                Log.WriteLine($"Trigger '{triggerName}' unregistered successfully");
            }
        }

        public static void UnregisterTrigger(ITrigger trigger)
        {
            TriggersValue = TriggersValue.Where(a => a != trigger).ToArray();
            trigger.Dispose();
            Log.WriteLine($"Trigger '{trigger.Name}' unregistered successfully");
        }

        public static IEnumerable<ITrigger> GetActiveTriggers()
        {
            return TriggersValue.Where(t => !t.Inactive);
        }

        public static IEnumerable<string> GetTriggerNames()
        {
            return TriggersValue.Select(t => t.Name);
        }

        public static ITrigger? GetTrigger(string name)
        {
            return TriggersValue.Where(t => t.Name == name).FirstOrDefault();
        }

        private static void OnTriggerActivated(object? sender, EventArgs eve)
        {
            if (sender is ITrigger trigger)
            {
                Log.WriteLine($"Trigger '{trigger.Name}' activated");
            }
        }

        public static void Dispose()
        {
            foreach (ITrigger trigger in TriggersValue)
            {
                trigger.Dispose();
            }

            TriggersValue = [];
        }
    }
}