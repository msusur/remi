using System;
using System.Collections.Generic;

namespace ReMi.Plugin.Jira.DataAccess.Gateways
{
    public interface IGlobalConfigurationGateway : IDisposable
    {
        PluginConfigurationEntity GetGlobalConfiguration();

        void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration);
    }
}
