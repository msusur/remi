using System;
using System.Reflection;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Plugins;
using ReMi.Plugin.Common.PluginsConfiguration;
using System.Collections.Generic;
using System.Linq;
using PluginConfiguration = ReMi.Plugin.Common.PluginsConfiguration.PluginConfiguration;

namespace ReMi.DataAccess.Helpers
{
    public static class PluginCollector
    {
        private static readonly IPluginConfiguration PluginConfiguration;

        static PluginCollector()
        {
            PluginConfiguration = new PluginConfiguration(GetAssemblies("remi.plugin", "remi.plugin.common"));
        }

        public static IEnumerable<DataEntities.Plugins.Plugin> Collect()
        {
            var pluginInitializers = PluginConfiguration.PluginInitializers;
            return pluginInitializers
                .Select(x => new DataEntities.Plugins.Plugin
                {
                    ExternalId = x.Id,
                    Key = x.Key,
                    PluginType = x.PluginType
                });
        }

        public static IEnumerable<PluginType> CollectGlobalPluginTypes()
        {
            var pluginTypes = EnumDescriptionHelper.GetEnumDescriptions<PluginType, PluginTypeDescription>();
            return pluginTypes
                .Where(x => x.IsGlobal.HasValue && x.IsGlobal.Value)
                .Select(x => (PluginType)x.Id);
        }

        public static IEnumerable<PluginType> CollectPackagePluginTypes()
        {
            var pluginTypes = EnumDescriptionHelper.GetEnumDescriptions<PluginType, PluginTypeDescription>();
            return pluginTypes
                .Where(x => !x.IsGlobal.HasValue || !x.IsGlobal.Value)
                .Select(x => (PluginType)x.Id);
        }

        private static IEnumerable<Assembly> GetAssemblies(string filterIn, string filterOut)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(o =>
                {
                    var name = o.GetName().Name;
                    return (string.IsNullOrEmpty(filterIn) || name.StartsWith(filterIn, StringComparison.InvariantCultureIgnoreCase))
                        && (string.IsNullOrEmpty(filterOut) || !name.StartsWith(filterOut, StringComparison.InvariantCultureIgnoreCase));
                })
                .ToArray();


            return assemblies;
        }
    }
}

