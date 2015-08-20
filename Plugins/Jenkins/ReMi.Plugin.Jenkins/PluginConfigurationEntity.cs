using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Jenkins
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("Jenkins User Name", PluginConfigurationType.String)]
        public string JenkinsUser { get; set; }
        [PluginConfiguration("Jenkins Password", PluginConfigurationType.Password)]
        public string JenkinsPassword { get; set; }
        [PluginConfiguration("Jenkins Servers", PluginConfigurationType.NameValueCollection)]
        public IEnumerable<NameValuePair> JenkinsServers { get; set; }

        public override string ToString()
        {
            return string.Format("[JenkinsUser={0}, JenkinsServers={1}]",
                JenkinsUser, JenkinsServers.FormatElements());
        }
    }
}
