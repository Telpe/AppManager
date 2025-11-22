using System;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal abstract class BaseTrigger : ITrigger
    {
        public abstract TriggerTypeEnum TriggerType { get; }
        public string Name { get; set; }
        public abstract string Description { get; }
        public bool IsActive { get; protected set; }

        public event EventHandler<TriggerActivatedEventArgs> TriggerActivated;

        protected BaseTrigger(string name = null)
        {
            Name = name ?? GetType().Name;
        }

        public abstract Task<bool> StartAsync(TriggerModel parameters = null);
        public abstract Task<bool> StopAsync();
        public abstract bool CanStart(TriggerModel parameters = null);

        protected virtual void OnTriggerActivated(TriggerActivatedEventArgs args)
        {
            args.TriggerName = Name;
            args.TriggerType = TriggerType;
            TriggerActivated?.Invoke(this, args);
        }

        protected virtual void OnTriggerActivated(string targetAppName, AppActionEnum action, ActionModel actionParams = null, object triggerData = null)
        {
            var args = new TriggerActivatedEventArgs
            {
                TriggerName = Name,
                TriggerType = TriggerType,
                TargetAppName = targetAppName,
                ActionToExecute = action,
                ActionParameters = actionParams,
                TriggerData = triggerData
            };
            
            OnTriggerActivated(args);
        }

        public virtual void Dispose()
        {
            if (IsActive)
            {
                _ = StopAsync();
            }
        }
    }
}