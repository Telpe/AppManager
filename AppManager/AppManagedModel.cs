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
        protected string _AppName = "";
        protected bool _IsSelected = false;
        protected bool _IsSimilarIncluded = false;
        protected bool _IsExitForced = false;
        protected bool _IsChildrenIncluded = false;

        public string AppName
        {
            get { return _AppName; }
            set { _AppName = value; }
        }
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; }
        }
        public bool IsSimilarIncluded
        {
            get { return _IsSimilarIncluded; }
            set { _IsSimilarIncluded = value; }
        }
        public bool IsExitForced
        {
            get { return _IsExitForced; }
            set { _IsExitForced = value; }
        }
        public bool IsChildrenIncluded
        {
            get { return _IsChildrenIncluded; }
            set { _IsChildrenIncluded = value; }
        }

        public AppManagedModel() { }

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
