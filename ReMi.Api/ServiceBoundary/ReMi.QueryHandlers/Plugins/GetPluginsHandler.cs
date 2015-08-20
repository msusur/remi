using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Plugins;
using System.Linq;
using ReMi.Common.Utils.Enums;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetPluginsHandler : IHandleQuery<GetPluginsRequest, GetPluginsResponse>
    {
        public Plugin.Common.PluginsConfiguration.IPluginConfiguration PluginConfiguration { get; set; }

        public GetPluginsResponse Handle(GetPluginsRequest request)
        {
            var pluginInitializers = PluginConfiguration.PluginInitializers.ToList();
            var plugins = pluginInitializers
                .Select(x => new BusinessEntities.Plugins.PluginView2
                {
                    PluginId = x.Id,
                    PluginKey = x.Key,
                    PluginTypes = x.PluginType.ToFlagList(),
                    IsGlobalConfigurationReadonly = x.IsGlobalConfigurationReadonly,
                    IsPackageConfigurationReadonly = x.IsPackageConfigurationReadonly
                })
                .OrderBy(x => x.PluginKey)
                .ToList();

            var response = new GetPluginsResponse
            {
                Plugins = plugins
            };

            return response;
        }
    }
}
