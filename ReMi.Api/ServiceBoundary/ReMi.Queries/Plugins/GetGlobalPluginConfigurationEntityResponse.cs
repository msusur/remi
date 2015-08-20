using ReMi.Contracts.Plugins.Data;
using System;

namespace ReMi.Queries.Plugins
{
    public class GetGlobalPluginConfigurationEntityResponse
    {
        public IPluginConfigurationEntity GlobalConfiguration { get; set; }

        public override string ToString()
        {
            return String.Format("[GlobalConfiguration={0}]",
                GlobalConfiguration);
        }
    }
}
