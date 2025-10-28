using System.Reflection;

namespace AppManager.Profile
{
    public class AppManagedModel : IAppManaged
    {
         public string AppName
        { get; set; }
        public bool Selected
        { get; set; }
        public bool IncludeSimilar
        { get; set; }
        public bool ForceExit
        { get; set; }
        public bool IncludeChildren
        { get; set; }

        // New action-related properties
        public string ExecutablePath { get; set; }
        public string LaunchArguments { get; set; }
        public string[] AvailableActions { get; set; } = { "Launch", "Close", "Restart", "Focus", "BringToFront", "Minimize" };
        public int ActionTimeoutMs { get; set; } = 5000;

        public AppManagedModel() 
        { }

        public static explicit operator AppManagedModel(AppManaged v)
        {
            AppManagedModel m = new AppManagedModel();

            foreach (PropertyInfo propertyInfo in typeof(IAppManaged).GetProperties())
            {
                m.GetType().GetProperty(propertyInfo.Name).SetValue(m, propertyInfo.GetValue(v));
            }

            return m;
        }

        public Actions.ActionParameters CreateActionParameters()
        {
            return new Actions.ActionParameters
            {
                ForceOperation = ForceExit,
                IncludeChildProcesses = IncludeChildren,
                IncludeSimilarNames = IncludeSimilar,
                TimeoutMs = ActionTimeoutMs,
                ExecutablePath = ExecutablePath,
                Arguments = LaunchArguments
            };
        }
    }
}
