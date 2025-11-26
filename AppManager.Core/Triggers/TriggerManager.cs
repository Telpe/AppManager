using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppManager.Core.Actions;

namespace AppManager.Core.Triggers
{
    public class TriggerManager : IDisposable
    {
        private readonly Dictionary<string, ITrigger> _triggers;
        private readonly ActionManager _actionManager;

        public event EventHandler<TriggerActivatedEventArgs> TriggerActivated;

        public TriggerManager(ActionManager actionManager = null)
        {
            _triggers = new Dictionary<string, ITrigger>();
            _actionManager = actionManager ?? new ActionManager();
        }

        public ITrigger CreateTrigger(TriggerModel model)
        {
            ITrigger trigger = model.TriggerType switch
            {
                TriggerTypeEnum.Shortcut => new ShortcutTrigger(model),
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

        public bool RegisterTrigger(ITrigger trigger)
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

        public bool UnregisterTrigger(string triggerName)
        {
            if (!_triggers.TryGetValue(triggerName, out ITrigger trigger))
                return false;

            trigger.TriggerActivated -= OnTriggerActivated;
            trigger.Stop(); // TODO: Make Stop async if needed
            var stopped = true;
            if (stopped)
            {
                _triggers.Remove(triggerName);
                trigger.Dispose();
                System.Diagnostics.Debug.WriteLine($"Trigger '{triggerName}' unregistered successfully");
            }

            return stopped;
        }

        public IEnumerable<ITrigger> GetActiveTriggers()
        {
            return _triggers.Values.Where(t => t.IsActive);
        }

        public IEnumerable<string> GetTriggerNames()
        {
            return _triggers.Keys;
        }

        public ITrigger GetTrigger(string name)
        {
            _triggers.TryGetValue(name, out ITrigger trigger);
            return trigger;
        }

        private void OnTriggerActivated(object? sender, TriggerActivatedEventArgs? e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Trigger '{e.TriggerName}' activated - executing action '{e.ActionToExecute}' on '{e.TargetAppName}'");

                // Execute the associated action
                _ = _actionManager.ExecuteActionAsync(e.ActionParameters);
                

                // Forward the event
                TriggerActivated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling trigger activation: {ex.Message}");
            }
        }

        public void StopAllTriggersAsync()
        {
            foreach (var trigger in _triggers.Values)
            {
                trigger.Stop();
            }

            System.Diagnostics.Debug.WriteLine("All triggers stopped");
        }

        public void Dispose()
        {
            foreach (var trigger in _triggers.Values)
            {
                trigger.Dispose();
            }

            _triggers.Clear();
        }
    }
}