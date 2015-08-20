using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using System;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;

namespace ReMi.CommandHandlers.Plugins
{
    public class AddPackagePluginConfigurationCommandHandler : IHandleCommand<AddPackagePluginConfigurationCommand>
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; } 

        public void Handle(AddPackagePluginConfigurationCommand command)
        {
            using (var gateway = PluginGatewayFactory())
            {
                gateway.AddPluginPackageConfiguration(command.PackageId);
            }
        }
    }
}
