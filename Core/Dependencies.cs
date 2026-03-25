using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AppManager.Core
{
    public static class Dependencies
    {
        public static bool Initialized { get; } = true;

        static Dependencies()
        {
            AssemblyLoadContext.Default.Resolving += ResolveOsApi;
        }

        private static Assembly ResolveOsApi(AssemblyLoadContext ctx, AssemblyName name)
        {
            var baseDir = Path.Combine(AppContext.BaseDirectory, "OsApi");
            var path = Path.Combine(baseDir, name.Name + ".dll");

            return File.Exists(path)
                ? ctx.LoadFromAssemblyPath(path)
                : throw new FileNotFoundException($"Assembly '{name.Name}' not found in '{baseDir}'.");
        }
    }
}
