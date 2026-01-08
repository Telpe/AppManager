using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using System;
using System.Threading.Tasks;

namespace AppManager.Core.Triggers
{
    public interface ITrigger : IDisposable
    {
        TriggerTypeEnum TriggerType { get; }
        string Name { get; set; }
        string Description { get; set; }
        bool Inactive { get; set; }

        ICondition[] Conditions { get; set; }

        IAction[] Actions { get; set; }
        
        event EventHandler? TriggerActivated;
        
        void Start();
        void Stop();
        bool CanStart();
        bool CanExecute();
        TriggerModel ToModel();
    }
}