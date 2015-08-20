using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Plugins
{
    [Command("Crates missing plugin package configuration", CommandGroup.Plugins, IsBackground = true)]
    public class AddPackagePluginConfigurationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid PackageId { get; set; }

        public override string ToString()
        {
            return String.Format("[PackageId={0}, CommandContext={1}]", PackageId, CommandContext);
        }
    }
}
