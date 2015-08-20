using System;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;

namespace ReMi.CommandHandlers.Plugins
{
    public class AssignGlobalPluginCommandHandler : IHandleCommand<AssignGlobalPluginCommand>
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; } 

        public void Handle(AssignGlobalPluginCommand command)
        {
            using (var gateway = PluginGatewayFactory())
            {
                gateway.AssignGlobalPlugin(command.ConfigurationId, command.PluginId);
            }
        }
    }
}
