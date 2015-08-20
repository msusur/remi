using Autofac;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetGlobalPluginConfigurationEntityHandler : IHandleQuery<GetGlobalPluginConfigurationEntityRequest, GetGlobalPluginConfigurationEntityResponse>
    {
        public IContainer Container { get; set; }

        public GetGlobalPluginConfigurationEntityResponse Handle(GetGlobalPluginConfigurationEntityRequest request)
        {
            var pluginConfiguration = Container.ResolveNamed<IPluginConfiguration>(request.PluginId.ToString().ToUpper());
            return new GetGlobalPluginConfigurationEntityResponse
            {
                GlobalConfiguration = pluginConfiguration.GetPluginConfiguration()
            };
        }
    }
}
