using System;

namespace ReMi.Plugin.Jenkins.DataAccess.Gateways
{
    public interface IGlobalConfigurationGateway : IDisposable
    {
        PluginConfigurationEntity GetGlobalConfiguration();

        void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration);
    }
}
