using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Ldap
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("User Name", PluginConfigurationType.String)]
        public string UserName { get; set; }
        [PluginConfiguration("Password", PluginConfigurationType.Password)]
        public string Password { get; set; }
        [PluginConfiguration("Ldap Path", PluginConfigurationType.String)]
        public string LdapPath { get; set; }
        [PluginConfiguration("Search Criteria", PluginConfigurationType.LongString)]
        public string SearchCriteria { get; set; }
    }
}
