using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Gerrit
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("Gerrit host", PluginConfigurationType.String)]
        public string Host { get; set; }
        [PluginConfiguration("Port", PluginConfigurationType.Int)]
        public int Port { get; set; }
        [PluginConfiguration("Gerrit User Name", PluginConfigurationType.String)]
        public string User { get; set; }
        [PluginConfiguration("Private Key", PluginConfigurationType.LongString)]
        public string PrivateKey { get; set; }

        public override string ToString()
        {
            return string.Format("[Host={0}, Port={1}, User={2}, PrivateKey={3}]",
                Host, Port, User, PrivateKey);
        }
    }
}
