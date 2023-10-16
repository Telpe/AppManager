using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager
{
    public class CloserOptions
    {
        public bool? ForceKill { get; set; } = false;
        public bool? IncludeChildren { get; set; } = true;
        public bool? IncludeTasksLikeGiven { get; set; } = false;
        public bool? ExitWhenDone { get; set; } = false;
    }
}
