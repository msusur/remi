using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.Common.Constants;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;
using ReMi.Events.ReleasePlan;
using ReMi.Queries.ContinuousDelivery;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class ApproveReleaseHandler : IHandleCommand<ApproveReleaseCommand>, IHandleCommand<AddReleaseApproversCommand>, IHandleCommand<RemoveReleaseApproverCommand>
    {
        public Func<IReleaseApproverGateway> ReleaseApproverGateway { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGateway { get; set; }
        public Func<IAccountsGateway> AccountsGateway { get; set; }
        public Func<IProductGateway> ProductGateway { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }
        public IPublishEvent PublishEvent { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse> GetQaStatus { get; set; }
        public IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse> GetReleaseContentInformationQuery { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }

        public void Handle(ApproveReleaseCommand command)
        {
            var session = AccountsBusinessLogic.SignSession(command.UserName, command.Password);
            if (session == null)
                throw new FailedToAuthenticateException(command.UserName);
            using (var gateway = AccountsGateway())
            {
                var approver = gateway.GetAccount(session.AccountId, true);
                if (approver.IsBlocked)
                    throw new AccountIsBlockedException(session.AccountId);
            }

            bool isFullyApproved;
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGateway())
            {
                releaseWindow = gateway.GetByExternalId(command.ReleaseWindowId, true);
            }

            using (var gateway = ReleaseApproverGateway())
            {
                gateway.ApproveRelease(session.AccountId, command.ReleaseWindowId, command.Comment);

                var approvers = gateway.GetApprovers(command.ReleaseWindowId).ToList();

                PublishEvent.Publish(new ApprovementEvent
                {
                    ReleaseWindowId = command.ReleaseWindowId,
                    ApproverId = session.AccountId,
                    Comment = command.Comment
                });

                isFullyApproved = IsFullyApproved(approvers, releaseWindow, command.CommandContext.UserId);
            }

            if (!isFullyApproved) return;

            using (var gateway = ReleaseWindowGateway())
            {
                ApproveRelease(releaseWindow, gateway);
            }
        }

        public void Handle(AddReleaseApproversCommand command)
        {
            foreach (var account in command.Approvers)
            {
                AddApprover(account);
            }
        }

        public void Handle(RemoveReleaseApproverCommand command)
        {
            bool isFullyApproved;
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGateway())
            {
                releaseWindow = gateway.GetByExternalId(command.ReleaseWindowId);
            }
            using (var gateway = ReleaseApproverGateway())
            {
                gateway.RemoveApprover(command.ApproverId);

                var approvers = gateway.GetApprovers(command.ReleaseWindowId);
                isFullyApproved = IsFullyApproved(approvers, releaseWindow, command.CommandContext.UserId);
            }

            using (var gateway = ReleaseWindowGateway())
            {
                PublishEvent.Publish(new ApproverRemovedFromReleaseEvent
                {
                    ReleaseWindowId = releaseWindow.ExternalId,
                    ApproverId = command.ApproverId
                });

                if (isFullyApproved)
                {
                    ApproveRelease(releaseWindow, gateway);
                }
            }
        }

        private bool IsFullyApproved(IEnumerable<ReleaseApprover> approvers, ReleaseWindow releaseWindow, Guid userId)
        {
            return BusinessRuleEngine.Execute<bool>(userId, BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId,
                new Dictionary<string, object>
                {
                    { BusinessRuleConstants.Release.ReleaseApprovalRule.Parameters[0].Name, approvers },
                    { BusinessRuleConstants.Release.ReleaseApprovalRule.Parameters[1].Name, releaseWindow }
                });
        }

        private void AddApprover(ReleaseApprover approver)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGateway())
            {
                releaseWindow = gateway.GetByExternalId(approver.ReleaseWindowId);
            }

            using (var gateway = AccountsGateway())
            {
                var existingAccount = gateway.GetAccount(approver.Account.ExternalId);
                if (existingAccount == null)
                {
                    IEnumerable<Product> products;
                    using (var gatewayProd = ProductGateway())
                    {
                        products = gatewayProd.GetProducts(approver.ReleaseWindowId).ToList();
                    }

                    approver.Account.Products = products.Select(x => new ProductView { IsDefault = true, Name = x.Description, ExternalId = x.ExternalId }).ToList();
                    approver.Account = gateway.CreateAccount(approver.Account);
                }
                else if (existingAccount.IsBlocked)
                    throw new AccountIsBlockedException(existingAccount.ExternalId);
            }

            using (var gateway = ReleaseApproverGateway())
            {
                gateway.AddApprover(approver);
            }

            PublishEvent.Publish(new ApproverAddedToReleaseWindowEvent
            {
                ReleaseWindowId = releaseWindow.ExternalId,
                Approver = approver
            });
        }

        private void ApproveRelease(ReleaseWindow releaseWindow, IReleaseWindowGateway gateway)
        {
            gateway.ApproveRelease(releaseWindow.ExternalId);

            using (var gatewayProd = ProductGateway())
            {
                var products = gatewayProd.GetProducts(releaseWindow.ExternalId).Where(o => o.ReleaseTrack == ReleaseTrack.Manual).ToList();
                if (products.Any())
                {
                    var status = ReleaseDecision.Go;

                    if (products.Any(product =>
                        GetQaStatus.Handle(new GetContinuousDeliveryStatusRequest { Products = new[] { product.Description } })
                            .StatusCheck.Any(x => x.Status != StatusType.Green)))
                    {
                        status = ReleaseDecision.NoGo;
                    }

                    CommandDispatcher.Send(new UpdateReleaseDecisionCommand
                    {
                        ReleaseDecision = status,
                        ReleaseWindowId = releaseWindow.ExternalId
                    });
                }
            }


            PublishEvent.Publish(new ReleaseWindowApprovedEvent { ReleaseWindow = releaseWindow });
            PublishEvent.Publish(new ReleaseStatusChangedEvent
            {
                ReleaseWindowId = releaseWindow.ExternalId,
                ReleaseStatus = EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved)
            });

            if (releaseWindow.ReleaseType == ReleaseType.Scheduled || releaseWindow.ReleaseType == ReleaseType.Automated)
            {
                var content = GetReleaseContentInformationQuery.Handle(new GetReleaseContentInformationRequest { ReleaseWindowId = releaseWindow.ExternalId });

                var includedContent = content.Content.Where(c => c.IncludeToReleaseNotes).ToList();
                CommandDispatcher.Send(new PersistTicketsCommand
                {
                    Tickets = includedContent,
                    ReleaseWindowId = releaseWindow.ExternalId
                });

                CommandDispatcher.Send(new StoreSourceControlChangesCommand
                {
                    ReleaseWindowId = releaseWindow.ExternalId
                });
            }
        }
    }
}
