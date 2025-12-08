using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers
{
    public interface IButtonTrigger
    {
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}