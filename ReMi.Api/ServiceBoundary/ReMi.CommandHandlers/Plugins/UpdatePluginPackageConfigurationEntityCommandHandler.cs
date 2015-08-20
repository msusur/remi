using Autofac;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using System;

namespace ReMi.CommandHandlers.Plugins
{
    public class UpdatePluginPackageConfigurationEntityCommandHandler : IHandleCommand<UpdatePluginPackageConfigurationEntityCommand>
    {
        public IContainer Container { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public Func<IPluginGateway> PluginGatewayFactory { get; set; } 

        public void Handle(UpdatePluginPackageConfigurationEntityCommand command)
        {
            Guid packageId, pluginId;
            using (var gateway = PackageGatewayFactory())
            {
                packageId = gateway.GetProduct(command.PackageName).ExternalId;
            }
            using (var gateway = PluginGatewayFactory())
            {
                pluginId = gateway.GetPlugin(command.PluginKey).PluginId;
            }
            var pluginConfiguration = Container.ResolveNamed<IPluginPackageConfiguration>(pluginId.ToString().ToUpper());
            pluginConfiguration.SetPluginPackageConfigurationEntity(packageId, command.PropertyName, command.JsonValue);
        }
    }
}
