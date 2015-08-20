using Autofac;
using ReMi.BusinessEntities.Exceptions;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;
using System.Linq;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetPluginsConfigurationHandler : IHandleQuery<GetPluginsConfigurationRequest, GetPluginsConfigurationResponse>
    {
        public Plugin.Common.PluginsConfiguration.IPluginConfiguration PluginConfiguration { get; set; }
        public IContainer Container { get; set; }

        public GetPluginsConfigurationResponse Handle(GetPluginsConfigurationRequest request)
        {
            var pluginInitializer = PluginConfiguration.GetPluginInitializer(request.PluginId);
            if (pluginInitializer == null)
                throw new PluginNotFoundException(request.PluginId);

            var globalConfiguration = Container.ResolveNamed<IPluginConfiguration>(request.PluginId.ToString().ToUpper());
            var packageConfiguration = Container.IsRegisteredWithName(request.PluginId.ToString().ToUpper(), typeof(IPluginPackageConfiguration))
                ? Container.ResolveNamed<IPluginPackageConfiguration>(request.PluginId.ToString().ToUpper())
                : null;
            var gc = globalConfiguration.GetPluginConfiguration();
            var gct = globalConfiguration.GetConfigurationTemplate();
            var pc = packageConfiguration == null ? null : packageConfiguration.GetPluginPackageConfiguration();
            var pct = packageConfiguration == null ? null : packageConfiguration.GetConfigurationTemplate();

            var plugin = new BusinessEntities.Plugins.Plugin
            {
                PluginId = pluginInitializer.Id,
                PluginKey = pluginInitializer.Key,
                PluginTypes = pluginInitializer.PluginType.ToFlagList(),
                GlobalConfiguration = gc,
                GlobalConfigurationTemplates = gct,
                PackageConfiguration = pc == null
                    ? null
                    : pc.GroupBy(c => c.PackageId)
                        .ToDictionary(c => c.Key, c => c.First()),
                PackageConfigurationTemplates = pct,
                IsGlobalConfigurationReadonly = pluginInitializer.IsGlobalConfigurationReadonly,
                IsPackageConfigurationReadonly = pluginInitializer.IsPackageConfigurationReadonly
            };
            var response = new GetPluginsConfigurationResponse
            {
                Plugin = plugin
            };

            return response;
        }
    }
}
