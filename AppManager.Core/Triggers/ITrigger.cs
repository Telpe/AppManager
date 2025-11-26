using AppManager.Core.Actions;
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
        bool IsActive { get; set; }
        
        event EventHandler<TriggerActivatedEventArgs> TriggerActivated;
        
        Task<bool> StartAsync();
        void Stop();
        bool CanStart();
        TriggerModel ToModel();
    }
}