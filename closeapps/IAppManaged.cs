using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace closeapps
{
    internal interface IAppManaged
    {
        public string AppName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSimilarIncluded { get; set; }
        public bool IsExitForced { get; set; }
        public bool IsChildrenIncluded { get; set; }

    }
}
