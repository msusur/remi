using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Update plugin global configuration entity", CommandGroup.Plugins)]
    public class UpdatePluginGlobalConfigurationEntityCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string PluginKey { get; set; }

        public string PropertyName { get; set; }

        public string JsonValue { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, PluginKey={1}, PropertyName={2}, JsonValue={3}]",
                CommandContext, PluginKey, PropertyName, JsonValue);
        }
    }
}
