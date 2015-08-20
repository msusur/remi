using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class RemoveCheckListQuestionCommandHandler : IHandleCommand<RemoveCheckListQuestionCommand>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(RemoveCheckListQuestionCommand command)
        {
            Guid releaseWindowId;
            using (var gateway = CheckListGatewayFactory())
            {
                releaseWindowId = gateway.GetCheckListItem(command.CheckListQuestionId).ReleaseWindowId;
                if (command.ForWholeProduct)
                {
                    gateway.RemoveCheckListQuestionForPackage(command.CheckListQuestionId);
                }
                else
                {
                    gateway.RemoveCheckListQuestion(command.CheckListQuestionId);
                }
            }

            EventPublisher.Publish(new CheckListQuestionRemovedEvent
            {
                CheckListId = command.CheckListQuestionId,
                ReleaseWindowGuid = releaseWindowId
            });
        }
    }
}
