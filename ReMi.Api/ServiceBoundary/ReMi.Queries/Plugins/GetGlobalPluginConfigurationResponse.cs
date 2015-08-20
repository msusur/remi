using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;

namespace ReMi.Queries.Plugins
{
    public class GetGlobalPluginConfigurationResponse
    {
        public IEnumerable<GlobalPluginConfiguration> GlobalPluginConfiguration { get; set; }
        public IEnumerable<BusinessEntities.Plugins.Plugin> GlobalPlugins { get; set; }

        public override string ToString()
        {
            return String.Format("[GlobalPluginConfiguration={0}, GlobalPlugins={1}]",
                GlobalPluginConfiguration.FormatElements(), GlobalPlugins.FormatElements());
        }
    }
}
