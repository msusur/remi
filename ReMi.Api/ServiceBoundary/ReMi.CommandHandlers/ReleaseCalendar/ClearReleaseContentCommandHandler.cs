using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class ClearReleaseContentCommandHandler : IHandleCommand<ClearReleaseContentCommand>
    {
        public Func<IReleaseContentGateway> ReleaseContentGatewayFactory { get; set; }

        public void Handle(ClearReleaseContentCommand command)
        {
            using (var gateway = ReleaseContentGatewayFactory())
            {
                gateway.RemoveTicketsFromRelease(command.ReleaseWindowId);
            }
        }
    }
}
