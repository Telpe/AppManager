using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppManager
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
    }
}
