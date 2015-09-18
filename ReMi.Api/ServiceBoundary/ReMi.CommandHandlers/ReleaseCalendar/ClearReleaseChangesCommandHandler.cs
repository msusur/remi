using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using System;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class ClearReleaseChangesCommandHandler : IHandleCommand<ClearReleaseChangesCommand>
    {
        public Func<ISourceControlChangeGateway> SourceControlChangesGatewayFactory { get; set; }

        public void Handle(ClearReleaseChangesCommand command)
        {
            using (var gateway = SourceControlChangesGatewayFactory())
            {
                gateway.RemoveChangesFromRelease(command.ReleaseWindowId);
            }
        }
    }
}
