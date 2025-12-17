using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppManager.Core.Models
{
    public class AppManagedModel
    {
        public string AppName { get; set; }
        public bool Active { get; set; }
        
        public Dictionary<int, TriggerModel> AppTriggers { get; set; } = new Dictionary<int, TriggerModel>();
        public Dictionary<int, ActionModel> AppActions { get; set; } = new Dictionary<int, ActionModel>();


        public AppManagedModel(string appName, bool active)
        {
            AppName = appName;
            Active = active;
        }

        public AppManagedModel Clone()
        {
            return new(this.AppName, this.Active)
            {
                AppTriggers = this.AppTriggers.Select(kvp => new KeyValuePair<int, TriggerModel>(kvp.Key, kvp.Value.Clone())).ToDictionary(),
                AppActions = this.AppActions.Select(kvp => new KeyValuePair<int, ActionModel>(kvp.Key, kvp.Value.Clone())).ToDictionary()
            };
        }
    }
}
