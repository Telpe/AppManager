using System.Reflection;

namespace AppManager.OsApi.Abstractions
{
    public static class OsApiLoader
    {
        public static IOsApi Load()
        {
            var os = DetectOsFamily(); // Windows7, Windows10, Windows11, Linux, osv.

            var assemblyPath = FindBestMatchingAssembly(os);

            if (assemblyPath == null)
                throw new PlatformNotSupportedException($"Ingen OS-API fundet til {os}");

            var asm = Assembly.LoadFrom(assemblyPath);
            var type = asm.GetTypes().First(t => typeof(IOsApi).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            return (IOsApi)Activator.CreateInstance(type)!;
        }
    }
}
