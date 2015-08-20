using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using System;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class ClearApproversSignaturesCommandHandler : IHandleCommand<ClearApproversSignaturesCommand>
    {
        public Func<IReleaseApproverGateway> ReleaseApproverGatewayFactory { get; set; }

        public void Handle(ClearApproversSignaturesCommand command)
        {
            using (var gateway = ReleaseApproverGatewayFactory())
            {
                gateway.ClearApproverSignatures(command.ReleaseWindowId);
            }
        }
    }
}
