using Autofac;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetPackagePluginConfigurationEntityHandler : IHandleQuery<GetPackagePluginConfigurationEntityRequest, GetPackagePluginConfigurationEntityResponse>
    {
        public IContainer Container { get; set; }

        public GetPackagePluginConfigurationEntityResponse Handle(GetPackagePluginConfigurationEntityRequest request)
        {
            var pluginConfiguration = Container.ResolveNamed<IPluginPackageConfiguration>(request.PluginId.ToString().ToUpper());
            return new GetPackagePluginConfigurationEntityResponse
            {
                PackageConfiguration = pluginConfiguration.GetPluginPackageConfigurationEntity(request.PackageId)
            };
        }
    }
}
