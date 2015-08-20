using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class DeleteReleaseTaskAttachmentCommandHandler : IHandleCommand<DeleteReleaseTaskAttachmentCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }

        public void Handle(DeleteReleaseTaskAttachmentCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.DeleteReleaseTaskAttachment(command.ReleaseTaskAttachmentId);
            }
        }
    }
}
