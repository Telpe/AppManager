using AppManager.OsApi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppManager.OsApi
{
    public static class OSAPI
    {
        private static IOsApi CurrentValue = OsApiLoader.Load();
        
        public static IOsApi Current { get => CurrentValue; }
        
        public static void SetCurrentOsApi(IOsApi osApi)
        {
            ArgumentNullException.ThrowIfNull(osApi);
            CurrentValue = osApi;
        }
    }
}
