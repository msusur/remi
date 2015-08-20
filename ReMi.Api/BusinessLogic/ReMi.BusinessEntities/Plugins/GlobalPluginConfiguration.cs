using ReMi.Contracts.Plugins.Data;
using System;

namespace ReMi.BusinessEntities.Plugins
{
    public class GlobalPluginConfiguration
    {
        public Guid ExternalId { get; set; }
        public PluginType PluginType { get; set; }
        public Guid? PluginId { get; set; }

        public override string ToString()
        {
            return String.Format("[ExternalId={0}, PluginType={1}, PluginId={2}]",
                ExternalId, PluginType, PluginId);
        }
    }
}
