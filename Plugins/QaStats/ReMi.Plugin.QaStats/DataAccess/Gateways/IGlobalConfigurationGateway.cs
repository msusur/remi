using System;

namespace ReMi.Plugin.QaStats.DataAccess.Gateways
{
    public interface IGlobalConfigurationGateway : IDisposable
    {
        PluginConfigurationEntity GetGlobalConfiguration();

        void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration);
    }
}
