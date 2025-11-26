using System;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    public abstract class BaseTrigger : ITrigger
    {
        public abstract TriggerTypeEnum TriggerType { get; }
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public bool IsActive { get; set; }

        public event EventHandler<TriggerActivatedEventArgs> TriggerActivated;

        protected BaseTrigger(TriggerModel model)
        {
            if (model.TriggerType != TriggerType)
            {
                throw new ArgumentException($"model type '{model.TriggerType}' does not match trigger type '{TriggerType}'.");
            }

            Name = TriggerType.ToString();
            IsActive = model.IsActive;
            TriggerActivated = new EventHandler<TriggerActivatedEventArgs>((s, e) => { });
        }

        public abstract TriggerModel ToModel();
        public abstract Task<bool> StartAsync();
        public abstract void Stop();
        public abstract bool CanStart();

        protected virtual void OnTriggerActivated(TriggerActivatedEventArgs args)
        {
            args.TriggerName = Name;
            args.TriggerType = TriggerType;
            TriggerActivated?.Invoke(this, args);
        }

        protected virtual void OnTriggerActivated(string targetAppName, AppActionTypeEnum action, ActionModel actionParams = null, object triggerData = null)
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
            Stop();
        }

    }
}