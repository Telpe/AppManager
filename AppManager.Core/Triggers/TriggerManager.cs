using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    public static class TriggerManager
    {
        private static readonly Dictionary<string, ITrigger> _triggers = new Dictionary<string, ITrigger>();

        //public event EventHandler<TriggerActivatedEventArgs> TriggerActivated = new EventHandler<TriggerActivatedEventArgs>((s, e) => { });

        public static ITrigger CreateTrigger(TriggerModel model)
        {
            ITrigger trigger = model.TriggerType switch
            {
                TriggerTypeEnum.Keybind => new KeybindTrigger(model),
                TriggerTypeEnum.Button => new ButtonTrigger(model),
                TriggerTypeEnum.AppLaunch => new AppLaunchTrigger(model),
                TriggerTypeEnum.AppClose => new AppCloseTrigger(model),
                TriggerTypeEnum.SystemEvent => new SystemEventTrigger(model),
                TriggerTypeEnum.NetworkPort => new NetworkPortTrigger(model),
                _ => throw new ArgumentException($"Unsupported trigger type: {model.TriggerType}")
            };

            // Subscribe to trigger activation events
            trigger.TriggerActivated += OnTriggerActivated;
            
            return trigger;
        }

        public static bool RegisterTrigger(ITrigger trigger)
        {
            if (trigger == null || _triggers.ContainsKey(trigger.Name))
                return false;

            if (!trigger.CanStart())
                return false;

            _ = trigger.StartAsync();

                
            _triggers[trigger.Name] = trigger;
            trigger.TriggerActivated += OnTriggerActivated;
            System.Diagnostics.Debug.WriteLine($"Trigger '{trigger.Name}' registered successfully");
                

            return true;
        }

        public static bool UnregisterTrigger(string triggerName)
        {
            if (!_triggers.TryGetValue(triggerName, out ITrigger? trigger)){ return false; }

            trigger.TriggerActivated -= OnTriggerActivated;
            trigger.Stop(); // TODO: Make Stop async if needed
            var stopped = true; // trigger.StopAsync().Wait();
            if (stopped)
            {
                _triggers.Remove(triggerName);
                trigger.Dispose();
                System.Diagnostics.Debug.WriteLine($"Trigger '{triggerName}' unregistered successfully");
            }

            return stopped;
        }

        public static IEnumerable<ITrigger> GetActiveTriggers()
        {
            return _triggers.Values.Where(t => !t.Inactive);
        }

        public static IEnumerable<string> GetTriggerNames()
        {
            return _triggers.Keys;
        }

        public static ITrigger? GetTrigger(string name)
        {
            _triggers.TryGetValue(name, out ITrigger? trigger);
            return trigger;
        }

        private static void OnTriggerActivated(object? sender, TriggerActivatedEventArgs? eve)
        {
            if (null == sender || null == eve) { return; }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Trigger '{eve.TriggerName}' activated - executing action '{eve.ActionToExecute}' on '{eve.TargetAppName}'");

                
                _ = null == eve.Model ? null : ActionManager.ExecuteActionAsync(eve.Model);

                // Forward the event
                //TriggerActivated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling trigger activation: {ex.Message}");
            }
        }

        public static void StopAllTriggersAsync()
        {
            foreach (var trigger in _triggers.Values)
            {
                trigger.Stop();
            }

            System.Diagnostics.Debug.WriteLine("All triggers stopped");
        }

        public static void Dispose()
        {
            foreach (var trigger in _triggers.Values)
            {
                trigger.Dispose();
            }

            _triggers.Clear();
        }
    }
}