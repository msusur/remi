using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Jira
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("Jira API URL", PluginConfigurationType.String)]
        public string JiraUrl { get; set; }
        [PluginConfiguration("Jira Browse URL", PluginConfigurationType.String)]
        public string JiraBrowseUrl { get; set; }
        [PluginConfiguration("Max issue count", PluginConfigurationType.Int)]
        public int JiraIssuesMaxCount { get; set; }
        [PluginConfiguration("Jira User Name", PluginConfigurationType.String)]
        public string JiraUser { get; set; }
        [PluginConfiguration("Jira Password", PluginConfigurationType.Password)]
        public string JiraPassword { get; set; }
    }
}
