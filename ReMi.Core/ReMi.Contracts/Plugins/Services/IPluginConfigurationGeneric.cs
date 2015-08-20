using ReMi.Contracts.Plugins.Data;

namespace ReMi.Contracts.Plugins.Services
{
    public interface IPluginConfiguration<out T> : IPluginConfiguration where T : IPluginConfigurationEntity, new()
    {
        new T GetPluginConfiguration();
    }
}
