using AppManager.Actions;
using AppManager.Triggers;
using System.Collections.Generic;
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

        public static explicit operator AppManagedModel(AppManaged v)
        {
            AppManagedModel m = new AppManagedModel();

            foreach (PropertyInfo propertyInfo in typeof(IAppManaged).GetProperties())
            {
                var targetProperty = m.GetType().GetProperty(propertyInfo.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(m, propertyInfo.GetValue(v));
                }
            }

            return m;
        }

    }
}
