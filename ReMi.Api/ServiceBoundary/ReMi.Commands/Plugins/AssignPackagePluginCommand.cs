using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Assign plugin to package", CommandGroup.Plugins)]
    public class AssignPackagePluginCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ConfigurationId { get; set; }
        public Guid? PluginId { get; set; }

        public override string ToString()
        {
            return string.Format("CommandContext={0}, ConfigurationId={1}, PluginId={2}",
                CommandContext, ConfigurationId, PluginId);
        }
    }
}
