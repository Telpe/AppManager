using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppManager.Utils
{
    public class VersionJsonConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string versionString = reader.GetString();
            
            if (string.IsNullOrEmpty(versionString))
            {
                return new Version { Exspansion = 0, Patch = 0, Hotfix = 0, Work = 0 };
            }

            try
            {
                return Version.Parse(versionString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing version '{versionString}': {ex.Message}");
                return new Version { Exspansion = 0, Patch = 0, Hotfix = 0, Work = 0 };
            }
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}