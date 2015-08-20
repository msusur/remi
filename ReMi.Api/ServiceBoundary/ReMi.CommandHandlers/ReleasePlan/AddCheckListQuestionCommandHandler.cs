using System;
using System.Linq;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class AddCheckListQuestionCommandHandler : IHandleCommand<AddCheckListQuestionsCommand>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(AddCheckListQuestionsCommand command)
        {
            using (var gateway = CheckListGatewayFactory())
            {

                gateway.AddCheckListQuestions(command.QuestionsToAdd, command.ReleaseWindowId);
                gateway.AssociateCheckListQuestionWithPackage(command.QuestionsToAssign, command.ReleaseWindowId);
            }

            EventPublisher.Publish(new CheckListQuestionsAddedEvent
            {
                Questions = command.QuestionsToAdd.Union(command.QuestionsToAssign),
                ReleaseWindowGuid = command.ReleaseWindowId
            });
        }
    }
}
