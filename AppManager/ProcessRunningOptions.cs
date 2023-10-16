using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager
{
    public class ProcessRunningOptions
    {
        public bool? ForceKill { get; set; } = false;
        public bool? IncludeChildren { get; set; } = true;
        public bool? IncludeTasksLikeGiven { get; set; } = false;
        public bool? ExitWhenDone { get; set; } = false;
    }
}
