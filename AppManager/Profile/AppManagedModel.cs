using AppManager.Actions;
using AppManager.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppManager.Profile
{
    public class AppManagedModel : IAppManaged
    {
        public string AppName { get; set; }
        public bool Active { get; set; }
        
        // Add missing interface properties
        public Dictionary<int, TriggerModel> AppTriggers { get; set; } = new Dictionary<int, TriggerModel>();
        public Dictionary<int, ActionModel> AppActions { get; set; } = new Dictionary<int, ActionModel>();


        public AppManagedModel() 
        { }

        public AppManagedModel Clone()
        {
            return new()
            {
                AppName = this.AppName,
                Active = this.Active,
                AppTriggers = this.AppTriggers.Select(kvp => new KeyValuePair<int, TriggerModel>(kvp.Key, kvp.Value.Clone())).ToDictionary(),
                AppActions = this.AppActions.Select(kvp => new KeyValuePair<int, ActionModel>(kvp.Key, kvp.Value.Clone())).ToDictionary()
            };
        }
    }
}
