using ReMi.Commands.Plugins;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Events.Packages;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.EventHandlers.Plugins
{
    public class AddPluginPackageConfigurationAfterAddingPackageHandler : IHandleEvent<NewPackageAddedEvent>
    {
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(NewPackageAddedEvent evnt)
        {
            if (evnt.Package != null)
                CommandDispatcher.Send(new AddPackagePluginConfigurationCommand
                {
                    CommandContext = evnt.Context.CreateChild<CommandContext>(),
                    PackageId = evnt.Package.ExternalId
                });
        }
    }
}
