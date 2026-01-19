using AppManager.Core.Actions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppManager.Core.Triggers
{
    public static class TriggerManager
    {
        private static ITrigger[] TriggersValue = [];
        public static ITrigger[] Triggers { get => TriggersValue; }

        public static ITrigger CreateTrigger(TriggerModel model)
        {
            return model.TriggerType switch
            {
                TriggerTypeEnum.Keybind => new KeybindTrigger(model),
                TriggerTypeEnum.Button => new ButtonTrigger(model),
                TriggerTypeEnum.AppLaunch => new AppLaunchTrigger(model),
                TriggerTypeEnum.AppClose => new AppCloseTrigger(model),
                TriggerTypeEnum.SystemEvent => new SystemEventTrigger(model),
                TriggerTypeEnum.NetworkPort => new NetworkPortTrigger(model),
                _ => throw new ArgumentException($"Unsupported trigger type: {model.TriggerType}")
            };
        }

        public static bool RegisterTrigger(TriggerModel trigger)
        {
            return RegisterTrigger(CreateTrigger(trigger));
        }

        public static bool RegisterTrigger(ITrigger trigger)
        { try{
                if (TriggersValue.Contains(trigger)) { throw new Exception("Error: Attempt to add duplicate trigger."); }

                if (trigger.CanStart())
                {
                    //trigger.TriggerActivated += OnTriggerActivated;
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
                //trigger.TriggerActivated -= OnTriggerActivated;
                trigger.Dispose();
                Log.WriteLine($"Trigger '{triggerName}' unregistered successfully");
            }
        }

        public static IEnumerable<ITrigger> GetActiveTriggers()
        {
            return TriggersValue.Where(t => !t.Inactive);
        }

        public static IEnumerable<string> GetTriggerNames()
        {
            return TriggersValue.Select(t => t.Name).ToArray();
        }

        public static ITrigger? GetTrigger(string name)
        {
            return TriggersValue.Where(t => t.Name == name).FirstOrDefault();
        }

        private static void OnTriggerActivated(object? sender, EventArgs eve)
        {
            if (sender is BaseTrigger trigger)
            {
                Log.WriteLine($"Trigger '{trigger.Name}' activated");
                if (trigger.CanExecute())
                {
                    _ = Task.Run(() => { 
                    foreach (IAction action in trigger.Actions)
                    {
                        try
                        {
                            action.Execute();
                            Log.WriteLine($"Action '{action.ActionType}' executed successfully for trigger '{trigger.Name}'.");  
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine($"Error executing action '{action.ActionType}' for trigger '{trigger.Name}': {ex.Message}");
                        }
                    }
                    });
                }
                else
                {
                    Log.WriteLine($"Trigger '{trigger.Name}' conditions not met; actions will not be executed");
                }
            }
        }

        public static void Dispose()
        {
            foreach (ITrigger trigger in TriggersValue)
            {
                //trigger.TriggerActivated -= OnTriggerActivated; // In case the trigger is kept alive elsewhere.
                trigger.Dispose();
            }

            TriggersValue = [];
        }
    }
}