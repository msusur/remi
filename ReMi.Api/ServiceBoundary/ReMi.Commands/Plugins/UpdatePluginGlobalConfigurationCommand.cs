using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Update plugin global configuration", CommandGroup.Plugins)]
    public class UpdatePluginGlobalConfigurationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid PluginId { get; set; }

        public string JsonValues { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, PluginId={1}, JsonValues={2}]",
                CommandContext, PluginId, JsonValues);
        }
    }
}
