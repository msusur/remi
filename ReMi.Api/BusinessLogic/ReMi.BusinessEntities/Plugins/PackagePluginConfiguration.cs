using ReMi.Contracts.Plugins.Data;
using System;

namespace ReMi.BusinessEntities.Plugins
{
    public class PackagePluginConfiguration
    {
        public Guid ExternalId { get; set; }
        public PluginType PluginType { get; set; }
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public string BusinessUnit { get; set; }
        public Guid? PluginId { get; set; }

        public override string ToString()
        {
            return String.Format("[ExternalId={0}, PluginType={1}, PackageId={2}, PackageName={3}, PluginId={4}, PluginId={5}]",
                ExternalId, PluginType, PackageId, PackageName, BusinessUnit, PluginId);
        }
    }
}
