using System.Collections.Generic;

namespace ReMi.Contracts.Plugins.Data
{
    public class PluginConfigurationTemplate
    {
        public string Description { get; set; }
        public PluginConfigurationType Type { get; set; }
        public string PropertyName { get; set; }
        public IEnumerable<PluginConfigurationEnumTemplate> Enums { get; set; }
        public IEnumerable<PluginConfigurationSelectTemplate> Select { get; set; }
    }

    public class PluginConfigurationEnumTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PluginConfigurationSelectTemplate
    {
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
