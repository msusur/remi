using Autofac;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.CommandHandlers.Plugins
{
    public class UpdatePluginGlobalConfigurationCommandHandler : IHandleCommand<UpdatePluginGlobalConfigurationCommand>
    {
        public IContainer Container { get; set; }

        public void Handle(UpdatePluginGlobalConfigurationCommand command)
        {
            var pluginConfiguration = Container.ResolveNamed<IPluginConfiguration>(command.PluginId.ToString().ToUpper());
            pluginConfiguration.SetPluginConfiguration(command.JsonValues);
        }
    }
}
