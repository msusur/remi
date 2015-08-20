using System;
using ReMi.Common.Utils;
using System.Linq;

namespace ReMi.Plugin.Common.PluginsConfiguration
{
    public static class PropertyUpdater
    {
        public static void UpdatePropertyValue<T>(this T source, string propertyName, object value, ISerialization serialization = null)
        {
            var propertyInfo = typeof (T).GetProperties()
                .FirstOrDefault(x => x.Name == propertyName);
            if (propertyInfo == null)
                return;

            var deserializedValue = Deserialize(value, propertyInfo.PropertyType, serialization);

            propertyInfo.SetValue(source, deserializedValue);
        }

        private static object Deserialize(object value, Type type, ISerialization serialization)
        {
            if (type.IsEnum)
                return Enum.Parse(type, value.ToString());
            return serialization == null
                ? value
                : serialization.FromJson(value.ToString(), type);
        }
    }
}
