using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class ReAssignReleaseChangesToReleaseCommandHandler : IHandleCommand<ReAssignReleaseChangesToReleaseCommand>
    {
        public Func<ISourceControlChangeGateway> SourceControlChangeGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(ReAssignReleaseChangesToReleaseCommand command)
        {
            using (var gateway = SourceControlChangeGatewayFactory())
            {
                gateway.RemoveChangesFromRelease(command.ReleaseWindowId);
            }

            CommandDispatcher.Send(new StoreSourceControlChangesCommand
            {
                CommandContext = command.CommandContext.CreateChild(),
                ReleaseWindowId = command.ReleaseWindowId
            });
        }
    }
}
