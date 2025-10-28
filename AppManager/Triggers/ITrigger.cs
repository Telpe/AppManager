using System;
using System.Threading.Tasks;
using AppManager.Actions;

namespace AppManager.Triggers
{
    public interface ITrigger : IDisposable
    {
        TriggerTypeEnum TriggerType { get; }
        string Name { get; set; }
        string Description { get; }
        bool IsActive { get; }
        
        event EventHandler<TriggerActivatedEventArgs> TriggerActivated;
        
        Task<bool> StartAsync(TriggerParameters parameters = null);
        Task<bool> StopAsync();
        bool CanStart(TriggerParameters parameters = null);
    }
}