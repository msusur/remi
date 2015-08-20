using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.BusinessEntities.Plugins
{
    public class Plugin
    {
        public Guid PluginId { get; set; }
        public string PluginKey { get; set; }
        public IEnumerable<PluginType> PluginTypes { get; set; }

        public bool IsGlobalConfigurationReadonly { get; set; }
        public bool IsPackageConfigurationReadonly { get; set; }

        public IPluginConfigurationEntity GlobalConfiguration { get; set; }
        public IDictionary<Guid, IPluginPackageConfigurationEntity> PackageConfiguration { get; set; }

        public IEnumerable<PluginConfigurationTemplate> GlobalConfigurationTemplates { get; set; }
        public IEnumerable<PluginConfigurationTemplate> PackageConfigurationTemplates { get; set; }
    }
}
