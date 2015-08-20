using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Gerrit.Service.Model;

namespace ReMi.Plugin.Gerrit
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("Repositories", PluginConfigurationType.Json)]
        public IEnumerable<Repository> Repositories { get; set; }

        public override string ToString()
        {
            return string.Format("[PackageId={0}, Repositories={1}]",
                PackageId, Repositories.FormatElements());
        }
    }
}
