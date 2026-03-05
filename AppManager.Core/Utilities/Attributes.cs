using System;

namespace AppManager.Core.Utilities
{
    // Simple attribute
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ActionCategoryAttribute : Attribute
    {
        public string Category { get; }

        public ActionCategoryAttribute(string category)
        {
            Category = category;
        }
    }

    // More complex attribute with multiple properties
    public class ActionParameterAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidationPattern { get; set; }

        public ActionParameterAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    // Attribute with usage restrictions
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ParameterOrderAttribute : Attribute
    {
        public int Order { get; }

        public ParameterOrderAttribute(int order)
        {
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DescriptionKeyAttribute : Attribute
    {
        public int TextKey { get; }

        public DescriptionKeyAttribute(int textKey)
        {
            TextKey = textKey;
        }
    }
}
