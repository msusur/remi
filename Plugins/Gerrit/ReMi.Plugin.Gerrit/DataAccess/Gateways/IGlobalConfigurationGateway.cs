using System;

namespace ReMi.Plugin.Gerrit.DataAccess.Gateways
{
    public interface IGlobalConfigurationGateway : IDisposable
    {
        PluginConfigurationEntity GetGlobalConfiguration();

        void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration);
    }
}
