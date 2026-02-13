using System.Linq;

namespace AppManager.Core.Models
{
    public class AppManagedModel
    {
        public string AppName { get; set; }
        public bool? InActive { get; set; }

        public ActionModel[]? Actions { get; set; }
        public ActionModel? Click { get; set; }
        public ActionModel? DoubleClick { get; set; }

        public AppManagedModel(string appName)
        {
            AppName = appName;
        }

        public AppManagedModel Clone()
        {
            return new(this.AppName)
            {
                InActive = this.InActive,
                Actions = this.Actions?.Select(a => a.Clone()).ToArray(),
                Click = this.Click?.Clone(),
                DoubleClick = this.DoubleClick?.Clone()
            };
        }

    }
}
