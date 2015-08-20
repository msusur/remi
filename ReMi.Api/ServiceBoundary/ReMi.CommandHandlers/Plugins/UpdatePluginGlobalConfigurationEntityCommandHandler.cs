using System;
using Autofac;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;

namespace ReMi.CommandHandlers.Plugins
{
    public class UpdatePluginGlobalConfigurationEntityCommandHandler : IHandleCommand<UpdatePluginGlobalConfigurationEntityCommand>
    {
        public IContainer Container { get; set; }
        public Func<IPluginGateway> PluginGatewayFactory { get; set; } 

        public void Handle(UpdatePluginGlobalConfigurationEntityCommand command)
        {
            Guid pluginId;
            using (var gateway = PluginGatewayFactory())
            {
                pluginId = gateway.GetPlugin(command.PluginKey).PluginId;
            }
            var pluginConfiguration = Container.ResolveNamed<IPluginConfiguration>(pluginId.ToString().ToUpper());
            pluginConfiguration.SetPluginConfiguration(command.PropertyName, command.JsonValue);
        }
    }
}
