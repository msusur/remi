using System;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;

namespace ReMi.CommandHandlers.Plugins
{
    public class AssignPackagePluginCommandHandler : IHandleCommand<AssignPackagePluginCommand>
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; }

        public void Handle(AssignPackagePluginCommand command)
        {
            using (var gateway = PluginGatewayFactory())
            {
                gateway.AssignPackagePlugin(command.ConfigurationId, command.PluginId);
            }
        }
    }
}
