using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers.Button
{
    public interface IButtonTrigger
    {
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}