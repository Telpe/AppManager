using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager
{
    internal interface IAppManaged
    {
        public string AppName { get; set; }
        public bool Selected { get; set; }
        public bool IncludeSimilar { get; set; }
        public bool ForceExit { get; set; }
        public bool IncludeChildren { get; set; }

    }
}
