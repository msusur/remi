using ReMi.BusinessEntities.Plugins;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Queries.Plugins
{
    public class GetPackagePluginConfigurationResponse
    {
        public IDictionary<Guid, IDictionary<PluginType, PackagePluginConfiguration>> PackagePluginConfiguration { get; set; }
        public IEnumerable<BusinessEntities.Plugins.Plugin> PackagePlugins { get; set; }

        public override string ToString()
        {
            return String.Format("[PackagePluginConfiguration={0}, PackagePlugins={1}]",
                PackagePluginConfiguration.FormatElements(), PackagePlugins.FormatElements());
        }
    }
}
