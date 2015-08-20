using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReleaseApproverDTO = ReMi.DataEntities.ReleasePlan.ReleaseApprover;
using ReleaseApprover = ReMi.BusinessEntities.ReleasePlan.ReleaseApprover;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseApproverGateway : BaseGateway, IReleaseApproverGateway
    {
        public IRepository<ReleaseApproverDTO> ReleaseApproverRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public void AddApprover(BusinessEntities.ReleasePlan.ReleaseApprover approver)
        {
            var accountExisting = AccountRepository.GetSatisfiedBy(o => o.ExternalId == approver.Account.ExternalId);
            if (accountExisting == null)
            {
                throw new AccountNotFoundException(approver.Account.ExternalId);
            }

            var approval = ReleaseApproverRepository.GetSatisfiedBy(
                o => o.Account.AccountId == accountExisting.AccountId && o.ReleaseWindow.ExternalId == approver.ReleaseWindowId);

            if (approval != null)
                throw new ReleaseApproverDuplicationException(approval.ExternalId);


            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == approver.ReleaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(approver.ReleaseWindowId);

            ReleaseApproverRepository.Insert(new ReleaseApproverDTO
            {
                AccountId = accountExisting.AccountId,
                ReleaseWindowId = releaseWindow.ReleaseWindowId,
                CreatedOn = SystemTime.Now,
                ExternalId = approver.ExternalId
            });
        }

        public void ApproveRelease(Guid accountId, Guid releaseWindowId, String description)
        {
            var approval = ReleaseApproverRepository.GetSatisfiedBy(
                o => o.Account.ExternalId == accountId && o.ReleaseWindow.ExternalId == releaseWindowId);

            if (approval == null)
                throw new ReleaseApproverNotFoundException(accountId);

            if (approval.ApprovedOn.HasValue)
            {
                Logger.WarnFormat("Approver already approved the release. AcccountId={0}, ReleaseWindowId={1}",
                    accountId, releaseWindowId);

                throw new Exception("Approver already approved the release");
            }

            approval.ApprovedOn = SystemTime.Now;

            ReleaseApproverRepository.Update(
                record => record.ReleaseApproverId == approval.ReleaseApproverId,
                record =>
                {
                    record.ApprovedOn = approval.ApprovedOn;
                    record.Comment = description;
                });
        }

        public void RemoveApprover(Guid approverId)
        {
            var approval = ReleaseApproverRepository.GetSatisfiedBy(
                o => o.ExternalId == approverId);

            if (approval == null)
                throw new ReleaseApproverNotFoundException(approverId);

            ReleaseApproverRepository.Delete(approval);
        }

        public void ClearApproverSignatures(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new EntityNotFoundException(typeof(ReleaseWindow), releaseWindowId);

            releaseWindow.ReleaseApprovers.Each(x => x.ApprovedOn = null);

            ReleaseWindowRepository.Update(releaseWindow);
        }

        public DateTime? WhenApproved(Guid accountId, Guid releaseWindowId)
        {
            var approval = ReleaseApproverRepository.GetSatisfiedBy(
                o => o.Account.ExternalId == accountId && o.ReleaseWindow.ExternalId == releaseWindowId);

            if (approval == null)
                throw new ReleaseApproverNotFoundException(accountId);

            return approval.ApprovedOn;
        }

        public IEnumerable<ReleaseApprover> GetApprovers(Guid releaseWindowId)
        {
            var approvers = ReleaseApproverRepository.GetAllSatisfiedBy(o => o.ReleaseWindow.ExternalId == releaseWindowId)
                    .Select(x => MappingEngine.Map<ReleaseApproverDTO, ReleaseApprover>(x))
                    .ToList();

            return approvers;
        }

        public override void OnDisposing()
        {
            ReleaseApproverRepository.Dispose();
            AccountRepository.Dispose();
            ReleaseWindowRepository.Dispose();
            MappingEngine.Dispose();

            base.OnDisposing();
        }
    }
}
