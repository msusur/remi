using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Common.PluginsConfiguration
{
    public static class PluginConfigurationTemplateBuilder
    {
        public static IEnumerable<PluginConfigurationTemplate> Build<T>(Func<IEnumerable<PluginConfigurationSelectTemplate>> getSelectValues = null)
            where T : class, IPluginConfigurationEntity
        {
            var attributeType = typeof(PluginConfigurationAttribute);
            return typeof(T).GetProperties()
                .Where(x => !x.GetCustomAttributes(attributeType, false).IsNullOrEmpty())
                .Select(x =>
                {
                    var attr = (PluginConfigurationAttribute)x.GetCustomAttributes(attributeType, false).First();
                    return new PluginConfigurationTemplate
                    {
                        Description = string.IsNullOrEmpty(attr.Description) ? x.Name : attr.Description,
                        PropertyName = x.Name,
                        Type = attr.Type,
                        Enums = attr.Type == PluginConfigurationType.Enum && x.PropertyType.IsEnum ? BuildEnum(x.PropertyType) : null,
                        Select = attr.Type == PluginConfigurationType.Select && getSelectValues != null ? getSelectValues() : null
                    };
                });
        }

        private static IEnumerable<PluginConfigurationEnumTemplate> BuildEnum(Type enumType)
        {
            return Enum.GetNames(enumType)
                .Select(x => new PluginConfigurationEnumTemplate
                {
                    Name = x,
                    Description = EnumDescriptionHelper.GetDescription(enumType, x)
                });
        }
    }
}
