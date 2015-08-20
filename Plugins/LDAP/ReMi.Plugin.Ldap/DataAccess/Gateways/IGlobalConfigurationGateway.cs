using System;

namespace ReMi.Plugin.Ldap.DataAccess.Gateways
{
    public interface IGlobalConfigurationGateway : IDisposable
    {
        PluginConfigurationEntity GetGlobalConfiguration();

        void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration);
    }
}
