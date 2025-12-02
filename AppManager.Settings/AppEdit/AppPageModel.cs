using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager.Settings.AppEdit
{
    internal class AppPageModel
    {
        public AppManagedModel[] BackupModels { get; set; } = Array.Empty<AppManagedModel>();
        public AppManagedModel CurrentModel { get; set; }
        public bool IsStored { get; set; } = true;
    }
}
