using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppManager.Actions;

namespace AppManager.Triggers
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

        public ITrigger CreateTrigger(TriggerTypeEnum triggerType, string name = null)
        {
            ITrigger trigger = triggerType switch
            {
                TriggerTypeEnum.Shortcut => new ShortcutTrigger(name),
                TriggerTypeEnum.Button => new ButtonTrigger(name),
                TriggerTypeEnum.AppLaunch => new AppLaunchTrigger(name),
                TriggerTypeEnum.AppClose => new AppCloseTrigger(name),
                TriggerTypeEnum.SystemEvent => new SystemEventTrigger(name),
                TriggerTypeEnum.NetworkPort => new NetworkPortTrigger(name),
                _ => throw new ArgumentException($"Unsupported trigger type: {triggerType}")
            };

            // Subscribe to trigger activation events
            trigger.TriggerActivated += OnTriggerActivated;
            
            return trigger;
        }

        public async Task<bool> RegisterTriggerAsync(ITrigger trigger, TriggerModel parameters = null)
        {
            if (trigger == null || _triggers.ContainsKey(trigger.Name))
                return false;

            if (!trigger.CanStart(parameters))
                return false;

            bool started = await trigger.StartAsync(parameters);
            if (started)
            {
                _triggers[trigger.Name] = trigger;
                trigger.TriggerActivated += OnTriggerActivated;
                System.Diagnostics.Debug.WriteLine($"Trigger '{trigger.Name}' registered successfully");
            }

            return started;
        }

        public async Task<bool> UnregisterTriggerAsync(string triggerName)
        {
            if (!_triggers.TryGetValue(triggerName, out ITrigger trigger))
                return false;

            trigger.TriggerActivated -= OnTriggerActivated;
            bool stopped = await trigger.StopAsync();
            
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

        private async void OnTriggerActivated(object sender, TriggerActivatedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Trigger '{e.TriggerName}' activated - executing action '{e.ActionToExecute}' on '{e.TargetAppName}'");

                // Execute the associated action
                bool success = await _actionManager.ExecuteActionAsync(e.ActionToExecute, e.TargetAppName, e.ActionParameters);
                
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Action '{e.ActionToExecute}' executed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to execute action '{e.ActionToExecute}'");
                }

                // Forward the event
                TriggerActivated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling trigger activation: {ex.Message}");
            }
        }

        public async Task StopAllTriggersAsync()
        {
            var tasks = _triggers.Values.Select(t => t.StopAsync());
            await Task.WhenAll(tasks);

            foreach (var trigger in _triggers.Values)
            {
                trigger.Dispose();
            }
            
            _triggers.Clear();
            System.Diagnostics.Debug.WriteLine("All triggers stopped");
        }

        public void Dispose()
        {
            _ = StopAllTriggersAsync();
        }
    }
}