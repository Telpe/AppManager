using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.OsApi.Events
{
    public delegate void TypedEvent<TSender, TArgs>(TSender sender, TArgs args);
    
}
