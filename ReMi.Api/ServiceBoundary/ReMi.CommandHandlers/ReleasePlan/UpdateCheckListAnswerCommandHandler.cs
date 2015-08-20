using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateCheckListAnswerCommandHandler : IHandleCommand<UpdateCheckListAnswerCommand>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateCheckListAnswerCommand command)
        {
            command.CheckListItem.LastChangedBy = command.CommandContext.UserName;
            Guid windowGuid;
            using (var checkListGateway = CheckListGatewayFactory())
            {
                checkListGateway.UpdateAnswer(command.CheckListItem);
                windowGuid = checkListGateway.GetCheckListItem(command.CheckListItem.ExternalId).ReleaseWindowId;
            }
            
            EventPublisher.Publish(new CheckListAnswerUpdatedEvent
            {
                AnsweredBy = command.CheckListItem.LastChangedBy,
                Checked = command.CheckListItem.Checked,
                CheckListId = command.CheckListItem.ExternalId,
                ReleaseWindowGuid = windowGuid
            });
        }
    }
}
