using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class ConfirmReleaseTaskReceiptHandler : 
        IHandleCommand<ConfirmReleaseTaskReceiptCommand>,
        IHandleCommand<CleanReleaseTaskReceiptCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }

        public void Handle(ConfirmReleaseTaskReceiptCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.ConfirmReleaseTaskReceipt(command.ReleaseTaskId);
            }
        }

        public void Handle(CleanReleaseTaskReceiptCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.ClearReleaseTaskReceipt(command.ReleaseTaskId);
            }
        }
    }
}
