using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CreateCheckListCommandHandler : IHandleCommand<CreateCheckListCommand>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }

        public void Handle(CreateCheckListCommand command)
        {
            using (var checkListGateway = CheckListGatewayFactory())
            {
                checkListGateway.Create(command.ReleaseWindowId);
            }
        }
    }
}
