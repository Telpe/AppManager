using System;
using System.ComponentModel;
using System.Reflection;

namespace AppManager.Config.Utilities
{
    public static class DescriptionHelper
    {
        // Læser fra Klasse, Interface, Property eller Method
        public static string GetDescription(MemberInfo member)
        {
            var attribute = member.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? $"{member.Name} (Ingen beskrivelse)";
        }

        // Extension method til Enum-værdier (f.eks. Status.Active.ToDescription())
        public static string ToDescription(this Enum enumValue)
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString());

            if (memInfo.Length > 0)
            {
                var attribute = memInfo[0].GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }

            return enumValue.ToString();
        }
    }
}



