using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.BusinessEntities.Plugins
{
    public class PluginView2
    {
        public Guid PluginId { get; set; }
        public string PluginKey { get; set; }
        public IEnumerable<PluginType> PluginTypes { get; set; }

        public bool IsGlobalConfigurationReadonly { get; set; }
        public bool IsPackageConfigurationReadonly { get; set; }
    }
}
