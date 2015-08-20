using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Update plugin package configuration entity", CommandGroup.Plugins)]
    public class UpdatePluginPackageConfigurationEntityCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string PluginKey { get; set; }

        public string PackageName { get; set; }

        public string PropertyName { get; set; }

        public string JsonValue { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, PluginKey={1}, PluginKey={2}, PropertyName={3}, JsonValue={4}]",
                CommandContext, PluginKey, PackageName, PropertyName, JsonValue);
        }
    }
}
