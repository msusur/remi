using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Update plugin package configuration", CommandGroup.Plugins)]
    public class UpdatePluginPackageConfigurationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid PluginId { get; set; }

        public Guid PackageId { get; set; }

        public string JsonValues { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, PluginId={1}, PackageId={2}, JsonValues={3}]",
                CommandContext, PluginId, PackageId, JsonValues);
        }
    }
}
