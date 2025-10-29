using AppManager.Actions;
using AppManager.Triggers;
using System.Collections.Generic;

namespace AppManager.Profile
{
    public interface IAppManaged
    {
        public string AppName { get; set; }
        public bool Active { get; set; }

        public Dictionary<int, TriggerModel> AppTriggers { get; set; }
        public Dictionary<int, ActionModel> AppActions { get; set; }
    }
}
