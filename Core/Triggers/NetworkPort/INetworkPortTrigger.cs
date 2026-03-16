using System;
using System.Collections.Generic;

namespace AppManager.Core.Triggers.NetworkPort
{
    public interface INetworkPortTrigger
    {
        int? Port { get; set; }
        string? IPAddress { get; set; }
        int? TimeoutMs { get; set; }
        Dictionary<string, object>? CustomProperties { get; set; }
    }
}