using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppManager.Core.Utilities
{
    public static class AttributeHelper
    {
        public static T? GetCustomAttribute<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        public static T? GetCustomAttribute<T>(PropertyInfo property) where T : Attribute
        {
            return property.GetCustomAttribute<T>();
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(Type type) where T : Attribute
        {
            return type.GetProperties()
                .Where(p => p.GetCustomAttribute<T>() != null)
                .OrderBy(p => p.GetCustomAttribute<ParameterOrderAttribute>()?.Order ?? int.MaxValue);
        }
    }
}
