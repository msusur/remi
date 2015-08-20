using ReMi.Contracts.Plugins.Data;
using System;

namespace ReMi.Queries.Plugins
{
    public class GetPackagePluginConfigurationEntityResponse
    {
        public IPluginPackageConfigurationEntity PackageConfiguration { get; set; }

        public override string ToString()
        {
            return String.Format("[PackageConfiguration={0}]",
                PackageConfiguration);
        }
    }
}
