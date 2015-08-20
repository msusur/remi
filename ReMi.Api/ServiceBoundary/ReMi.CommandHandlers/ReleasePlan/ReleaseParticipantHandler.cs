using System;
using System.Linq;
using Common.Logging;
using ReMi.Commands.Auth;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class ReleaseParticipantHandler : IHandleCommand<AddReleaseParticipantCommand>, IHandleCommand<RemoveReleaseParticipantCommand>
    {
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        public void Handle(AddReleaseParticipantCommand command)
        {
            if (command.Participants == null || command.Participants.Count == 0)
            {
                _log.InfoFormat("There is no participants to add");
                return;
            }

            using (var gateway = AccountsGatewayFactory())
            {
                var participantIds = command.Participants.Select(x => x.Account.ExternalId).ToArray();
                var existingAccounts = gateway.GetAccounts(participantIds).ToList();
                if (existingAccounts.Any(x => x.IsBlocked))
                    throw new AccountIsBlockedException(existingAccounts.First(x => x.IsBlocked).ExternalId);

                gateway.CreateNotExistingReleaseParticipants(command.Participants);
            }

            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                gateway.AddReleaseParticipants(command.Participants);
            }

            CommandDispatcher.Send(new AssociateAccountsWithProductCommand
            {
                Accounts = command.Participants.Select(p => p.Account).ToList(),
                ReleaseWindowId = command.Participants[0].ReleaseWindowId
            });

            EventPublisher.Publish(new ReleaseParticipantsAddedEvent
            {
                Participants = command.Participants,
                ReleaseWindowId = command.Participants[0].ReleaseWindowId
            });
        }

        public void Handle(RemoveReleaseParticipantCommand command)
        {
            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                gateway.RemoveReleaseParticipant(command.Participant);
            }

            EventPublisher.Publish(new ReleaseParticipantRemovedEvent()
            {
                Participant = command.Participant,
                ReleaseWindowId = command.Participant.ReleaseWindowId
            });
        }
    }
}
