using System;
using System.Collections.Generic;
using ReMi.Contracts.Enums;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Jira
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("JQL Filter", PluginConfigurationType.NameValueCollection)]
        public IEnumerable<NameValuePair> JqlFilter { get; set; }
        [PluginConfiguration("Defect JQL Filter", PluginConfigurationType.NameValueCollection)]
        public IEnumerable<NameValuePair> DefectFilter { get; set; }
        [PluginConfiguration("Update Ticke Mode", PluginConfigurationType.Enum)]
        public UpdateTicketMode UpdateTicketMode { get; set; }
        [PluginConfiguration("Ticket Label", PluginConfigurationType.String)]
        public string Label { get; set; }
    }

    public enum UpdateTicketMode
    {
        None,
        [EnumDescription("Add Label")]
        AddLabel
    }
}
