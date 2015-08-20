using System;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.DeploymentTool;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.ReleaseExecution
{
    public class SignOffHandler : IHandleCommand<AddPeopleToSignOffReleaseCommand>, IHandleCommand<SignOffReleaseCommand>, IHandleCommand<RemoveSignOffCommand>
    {
        public Func<ISignOffGateway> SignOffGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void Handle(AddPeopleToSignOffReleaseCommand command)
        {
            if (command.IsBackground)
            {
                var releaseWindow = GetReleaseWindow(command.ReleaseWindowId);

                using (var gateway = AccountGatewayFactory())
                {
                    command.SignOffs =
                        gateway.GetProductOwners(releaseWindow.Products)
                            .Select(p => new SignOff { Signer = p, ExternalId = Guid.NewGuid() }).ToList();
                }
            }
            else
            {
                using (var gateway = AccountGatewayFactory())
                {
                    var accounts = command.SignOffs.Select(s => s.Signer).ToList();

                    var signerIds = command.SignOffs.Select(x => x.Signer.ExternalId).ToArray();
                    var existingAccounts = gateway.GetAccounts(signerIds).ToList();
                    if (existingAccounts.Any(x => x.IsBlocked))
                        throw new AccountIsBlockedException(existingAccounts.First(x => x.IsBlocked).ExternalId);

                    gateway.CreateNotExistingAccounts(accounts, "ProductOwner");
                }

                command.SignOffs.Where(s => String.IsNullOrEmpty(s.Signer.Role.Description)).ToList().ForEach(
                    signer =>
                    {
                        signer.Signer.Role.Description = "Product owner";
                        signer.Signer.Role.Name = "ProductOwner";
                    });
            }

            using (var gateway = SignOffGatewayFactory())
            {
                gateway.AddSigners(command.SignOffs, command.ReleaseWindowId);
            }

            EventPublisher.Publish(new ReleaseSignersAddedEvent
            {
                SignOffs = command.SignOffs,
                ReleaseWindowId = command.ReleaseWindowId
            });
        }

        public void Handle(SignOffReleaseCommand command)
        {
            var session = AccountsBusinessLogic.SignSession(command.UserName, command.Password);
            if (session == null)
                throw new FailedToAuthenticateException(command.UserName);

            Account signer;
            using (var accountGateway = AccountGatewayFactory())
            {
                signer = accountGateway.GetAccount(session.AccountId, true);
                if (signer.IsBlocked)
                    throw new AccountIsBlockedException(signer.ExternalId);
            }

            using (var gateway = SignOffGatewayFactory())
            {
                gateway.SignOff(session.AccountId, command.ReleaseWindowId, command.Comment);

                EventPublisher.Publish(new ReleaseSignedOffBySignerEvent
                {
                    ReleaseWindowGuid = command.ReleaseWindowId,
                    AccountId = session.AccountId,
                    Comment = command.Comment
                });

                var releaseSignedOff = gateway.CheckSigningOff(command.ReleaseWindowId);

                if (releaseSignedOff)
                {
                    NotifyAboutSignOffReleaseWindow(command.ReleaseWindowId, command.CommandContext);
                }
            }
        }

        public void Handle(RemoveSignOffCommand command)
        {
            using (var gateway = SignOffGatewayFactory())
            {
                gateway.RemoveSigner(command.SignOffId);
                EventPublisher.Publish(new RemoveSignOffEvent
                {
                    ReleaseWindowGuid = command.ReleaseWindowId,
                    SignOffId = command.SignOffId,
                    AccountId = command.AccountId
                });

                var releaseSignedOff = gateway.CheckSigningOff(command.ReleaseWindowId);
                if (releaseSignedOff)
                {
                    NotifyAboutSignOffReleaseWindow(command.ReleaseWindowId, command.CommandContext);
                }
            }
        }

        private void NotifyAboutSignOffReleaseWindow(Guid releaseWindowId, BaseContext context)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            if (!ReleaseWindowHelper.IsMaintenance(releaseWindow))
            {
                CommandDispatcher.Send(new PopulateDeploymentMeasurementsCommand
                {
                    CommandContext = context.CreateChild<CommandContext>(),
                    ReleaseWindowId = releaseWindow.ExternalId
                });
            }

            EventPublisher.Publish(new ReleaseWindowSignedOffEvent { ReleaseWindow = releaseWindow });

            EventPublisher.Publish(new ReleaseStatusChangedEvent
            {
                ReleaseWindowId = releaseWindow.ExternalId,
                ReleaseStatus = EnumDescriptionHelper.GetDescription(ReleaseStatus.SignedOff)
            });
        }

        private ReleaseWindow GetReleaseWindow(Guid windowId)
        {
            using (var windowGateway = ReleaseWindowGatewayFactory())
            {
                return windowGateway.GetByExternalId(windowId, true);
            }
        }
    }
}
