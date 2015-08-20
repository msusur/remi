using System;

namespace ReMi.Contracts.Plugins.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PluginConfigurationAttribute : Attribute
    {
        private readonly string _description;
        private readonly PluginConfigurationType _type;

        public PluginConfigurationAttribute(string description, PluginConfigurationType type)
        {
            _description = description;
            _type = type;
        }

        public string Description
        {
            get { return _description; }
        }

        public PluginConfigurationType Type
        {
            get { return _type; }
        }
    }
}
