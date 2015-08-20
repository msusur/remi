using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.DeploymentTool;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public class ReleaseWindowStateUpdater : IReleaseWindowStateUpdater
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }
        public Func<ISignOffGateway> SignOffGatewayFactory { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void CloseRelease(Guid releaseWindowId, string releaseNotes, IEnumerable<Account> recipients, Guid userId)
        {
            Close(releaseWindowId, releaseNotes, recipients, userId);
        }

        private void Close(Guid releaseWindowId, string releaseNotes, IEnumerable<Account> recipients, Guid userId)
        {
            if (!recipients.IsNullOrEmpty())
            {
                using (var gateway = AccountsGatewayFactory())
                {
                    foreach (var addressee in recipients)
                    {
                        gateway.CreateNotExistingAccount(addressee);
                    }
                }
            }

            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = gateway.GetByExternalId(releaseWindowId, true);

                var isReleaseMaintenance = ReleaseWindowHelper.IsMaintenance(releaseWindow);

                if (isReleaseMaintenance || releaseWindow.SignedOff.HasValue)
                {
                    gateway.CloseRelease(releaseNotes, releaseWindowId);
                }
                else
                {
                    using (var signOffGateway = SignOffGatewayFactory())
                    {
                        var signOffs = signOffGateway.GetSignOffs(releaseWindowId);
                        var allowToClose = CheckIfAllowedToClose(userId, signOffs, releaseWindow);
                        if (allowToClose)
                        {
                            if (signOffs != null)
                            {
                                foreach (var signOff in signOffs.Where(x => !x.SignedOff))
                                {
                                    signOffGateway.SignOff(signOff.Signer.ExternalId, releaseWindow.ExternalId,
                                        "Automated sign off during release closing");
                                }
                                signOffGateway.CheckSigningOff(releaseWindow.ExternalId);
                            }

                            gateway.CloseRelease(releaseNotes, releaseWindowId);

                            CommandDispatcher.Send(new PopulateDeploymentMeasurementsCommand
                            {
                                ReleaseWindowId = releaseWindow.ExternalId
                            });
                        }
                        else
                            throw new ReleaseNotSignedOffException(releaseWindow);
                    }
                }
            }

            EventPublisher.Publish(new ReleaseWindowClosedEvent
            {
                Recipients = recipients,
                ReleaseWindowId = releaseWindowId,
                IsFailed = false
            });

            EventPublisher.Publish(new ReleaseStatusChangedEvent
            {
                ReleaseWindowId = releaseWindowId,
                ReleaseStatus = EnumDescriptionHelper.GetDescription(ReleaseStatus.Closed)
            });
        }

        private bool CheckIfAllowedToClose(Guid userId, IEnumerable<SignOff> signOffs, ReleaseWindow releaseWindow)
        {
            return BusinessRuleEngine.Execute<bool>(userId,
                BusinessRuleConstants.Release.AllowCloseAfterSignOffRule.ExternalId,
                new Dictionary<string, object>
                {
                    { BusinessRuleConstants.Release.AllowCloseAfterSignOffRule.Parameters.ElementAt(0).Name, signOffs },
                    { BusinessRuleConstants.Release.AllowCloseAfterSignOffRule.Parameters.ElementAt(1).Name, releaseWindow }
                });
        }
    }
}
