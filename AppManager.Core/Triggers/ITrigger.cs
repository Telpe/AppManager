using System;
using System.Threading.Tasks;
using AppManager.Core.Actions;

namespace AppManager.Core.Triggers
{
    public interface ITrigger : IDisposable
    {
        TriggerTypeEnum TriggerType { get; }
        string Name { get; set; }
        string Description { get; }
        bool IsActive { get; }
        
        event EventHandler<TriggerActivatedEventArgs> TriggerActivated;
        
        Task<bool> StartAsync(TriggerModel parameters = null);
        Task<bool> StopAsync();
        bool CanStart(TriggerModel parameters = null);
    }
}