using Autofac;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.CommandHandlers.Plugins
{
    public class UpdatePluginPackageConfigurationCommandHandler : IHandleCommand<UpdatePluginPackageConfigurationCommand>
    {
        public IContainer Container { get; set; }

        public void Handle(UpdatePluginPackageConfigurationCommand command)
        {
            var pluginConfiguration = Container.ResolveNamed<IPluginPackageConfiguration>(command.PluginId.ToString().ToUpper());
            pluginConfiguration.SetPluginPackageConfigurationEntity(command.PackageId, command.JsonValues);
        }
    }
}
