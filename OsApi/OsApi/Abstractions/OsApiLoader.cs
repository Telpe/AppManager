using AppManager.OsApi.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AppManager.OsApi.Abstractions
{
    public static class OsApiLoader
    {
        private static readonly string OsApiDirValue = Path.GetDirectoryName(typeof(OsApiLoader).Assembly.Location) ?? String.Empty;

        public static IOsApi Load()
        {
            string os = DetectOsFamily(); // Windows7, Windows10, Windows11, Linux, osv.

            string? assemblyPath = FindBestMatchingAssembly(os);

            if (assemblyPath == null)
                throw new PlatformNotSupportedException($"Ingen OS-API fundet til {os}");

            var asm = Assembly.LoadFrom(assemblyPath);
            var type = asm.GetTypes().First(t => typeof(IOsApi).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            return (IOsApi)Activator.CreateInstance(type)!;
        }

        private static string? FindBestMatchingAssembly(string os)
        {
            return os switch
            {
                "Windows11" => Path.Combine(OsApiDirValue, "AppManager.OsApi.Windows11.dll"),
                _ => null
            };
        }

        private static string DetectOsFamily()
        {
            return "Windows11";
        }
    }
}
