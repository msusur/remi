using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateCheckListCommentCommandHandler : IHandleCommand<UpdateCheckListCommentCommand>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateCheckListCommentCommand command)
        {
            command.CheckListItem.LastChangedBy = command.CommandContext.UserName;
            Guid windowGuid;
            using (var checkListGateway = CheckListGatewayFactory())
            {
                checkListGateway.UpdateComment(command.CheckListItem);
                windowGuid = checkListGateway.GetCheckListItem(command.CheckListItem.ExternalId).ReleaseWindowId;
            }

            EventPublisher.Publish(new CheckListCommentUpdatedEvent
            {
                AnsweredBy = command.CheckListItem.LastChangedBy,
                Comment = command.CheckListItem.Comment,
                CheckListId = command.CheckListItem.ExternalId,
                ReleaseWindowGuid = windowGuid
            });
        }
    }
}
