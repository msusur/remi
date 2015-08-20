using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Go
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("GO User Name", PluginConfigurationType.String)]
        public string GoUser { get; set; }
        [PluginConfiguration("GO Password", PluginConfigurationType.Password)]
        public string GoPassword { get; set; }
        [PluginConfiguration("GO Servers", PluginConfigurationType.NameValueCollection)]
        public IEnumerable<NameValuePair> GoServers { get; set; }

        public override string ToString()
        {
            return string.Format("[GoUser={0}, GoServers={1}]",
                GoUser, GoServers.FormatElements());
        }
    }
}
