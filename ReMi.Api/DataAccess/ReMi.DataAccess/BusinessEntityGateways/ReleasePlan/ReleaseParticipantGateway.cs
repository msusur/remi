using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseParticipantGateway : BaseGateway, IReleaseParticipantGateway
    {
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<ReleaseParticipant> ReleaseParticipantRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public override void OnDisposing()
        {
            AccountRepository.Dispose();
            ReleaseWindowRepository.Dispose();
            ReleaseParticipantRepository.Dispose();

            base.OnDisposing();
        }

        public void AddReleaseParticipants(List<BusinessEntities.ReleasePlan.ReleaseParticipant> releaseParticipants, Guid authorId)
        {
            var releaseWindowGuid = releaseParticipants[0].ReleaseWindowId;

            var releaseWindow =
                ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowGuid);

            if (releaseWindow == null)
            {
                Log.ErrorFormat(
                    "ReleaseParticipantGateway: ReleaseWindow with ExternalId={0} was not found in accounts repository",
                    releaseParticipants[0].ReleaseWindowId);
                throw new ReleaseWindowNotFoundException(releaseWindowGuid);
            }

            var releaseWindowId = releaseWindow.ReleaseWindowId;

            var existingParticipants =
                ReleaseParticipantRepository.GetAllSatisfiedBy(rp => rp.ReleaseWindowId == releaseWindowId).ToList();

            foreach (var releaseParticipant in releaseParticipants)
            {
                var accountId =
                    AccountRepository.GetSatisfiedBy(acc => acc.Email == releaseParticipant.Account.Email).AccountId;
                if (!existingParticipants.Any(p => p.AccountId == accountId && p.ReleaseWindowId == releaseWindowId))
                {
                    var releaseParticipantEntity = new ReleaseParticipant
                    {
                        AccountId = accountId,
                        ReleaseWindowId = releaseWindowId,
                        ExternalId = releaseParticipant.ReleaseParticipantId == Guid.Empty ? Guid.NewGuid() : releaseParticipant.ReleaseParticipantId,
                        ApprovedOn =
                            releaseParticipant.Account.ExternalId == authorId
                                ? SystemTime.Now
                                : (DateTime?) null
                    };

                    ReleaseParticipantRepository.Insert(releaseParticipantEntity);
                }
            }
        }

        public void AddReleaseParticipants(List<BusinessEntities.ReleasePlan.ReleaseParticipant> releaseParticipants)
        {
            AddReleaseParticipants(releaseParticipants, Guid.NewGuid());
        }

        public void RemoveReleaseParticipant(BusinessEntities.ReleasePlan.ReleaseParticipant releaseParticipant)
        {
            var account =
                AccountRepository.GetSatisfiedBy(
                    x =>
                        x.Email == releaseParticipant.Account.Email);

            if (account == null)
            {
                Log.ErrorFormat("ReleaseParticipantGateway: Account={0} was not found in accounts repository",
                    releaseParticipant.Account);
            }

            var accountId = account.AccountId;

            var releaseWindow =
                ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseParticipant.ReleaseWindowId);

            if (releaseWindow == null)
            {
                Log.ErrorFormat(
                    "ReleaseParticipantGateway: ReleaseWindow with ExternalId={0} was not found in accounts repository",
                    releaseParticipant.ReleaseWindowId);
            }

            var releaseWindowId = releaseWindow.ReleaseWindowId;

            var participant =
                ReleaseParticipantRepository.GetSatisfiedBy(
                    x => x.AccountId == accountId && x.ReleaseWindowId == releaseWindowId);

            if (participant != null)
            {
                ReleaseParticipantRepository.Delete(participant);
            }
            else
            {
                Log.ErrorFormat("ReleaseParticipantGateway: Cannot find release participant={0} in repository",
                    releaseParticipant);
            }
        }

        public IEnumerable<BusinessEntities.ReleasePlan.ReleaseParticipant> GetReleaseParticipants(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(window => window.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                Log.ErrorFormat("Releease window with id={0} was not found in repository", releaseWindowId);

            var releaseParticipants =
                ReleaseParticipantRepository.GetAllSatisfiedBy(
                    rp => rp.ReleaseWindowId == releaseWindow.ReleaseWindowId).ToList();

            if (!releaseParticipants.Any())
            {
                Log.InfoFormat("There are no participants for release={0}", releaseWindowId);
                return new List<BusinessEntities.ReleasePlan.ReleaseParticipant>();
            }

            var accountIdList = releaseParticipants.Select(account => account.AccountId).ToList();

            var accounts =
                AccountRepository.GetAllSatisfiedBy(account => accountIdList.Any(acc => acc == account.AccountId))
                    .ToList();

            var releaseParticipantsList = releaseParticipants.Join(accounts, rp => rp.AccountId, a => a.AccountId,
                (rp, a) =>
                    new BusinessEntities.ReleasePlan.ReleaseParticipant
                    {
                        ReleaseWindowId = releaseWindowId,
                        ReleaseParticipantId = rp.ExternalId,
                        Account = Mapper.Map<Account, BusinessEntities.Auth.Account>(a),
                        IsParticipationConfirmed = rp.ApprovedOn != null
                    }).ToList();

            return releaseParticipantsList;
        }

        public List<BusinessEntities.Auth.Account> GetReleaseMembers(Guid releaseWindowId)
        {
            var window = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            var members = window.ReleaseParticipants.Select(x => x.Account).ToList();
            members.AddRange(
                window.SignOffs.Where(x => members.All(m => m.Email != x.Account.Email)).Select(s => s.Account));
            members.AddRange(
                window.ReleaseApprovers.Where(x => members.All(m => m.Email != x.Account.Email)).Select(s => s.Account));

            return Mapper.Map<List<Account>, List<BusinessEntities.Auth.Account>>(members);

        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow GetReleaseWindow(Guid releaseParticipantId)
        {
            return
                Mapper.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                    ReleaseParticipantRepository.GetSatisfiedBy(x => x.ExternalId == releaseParticipantId).ReleaseWindow);
        }

        public void ApproveReleaseParticipation(Guid releaseParticipantId)
        {
            var releaseParticipant =
                ReleaseParticipantRepository.GetSatisfiedBy(rp => rp.ExternalId == releaseParticipantId);

            if (releaseParticipant.ApprovedOn == null)
            {
                releaseParticipant.ApprovedOn = SystemTime.Now;
                ReleaseParticipantRepository.Update(releaseParticipant);
            }
        }

        public void ClearParticipationApprovements(Guid releaseWindowId, Guid authorId)
        {
            var participants =
                ReleaseParticipantRepository.GetAllSatisfiedBy(rp => rp.ReleaseWindow.ExternalId == releaseWindowId);

            foreach (var releaseParticipant in participants)
            {
                if (releaseParticipant.Account.ExternalId == authorId)
                {
                    releaseParticipant.ApprovedOn = SystemTime.Now;
                }
                else
                {
                    releaseParticipant.ApprovedOn = null;
                }

                ReleaseParticipantRepository.Update(releaseParticipant);
            }
        }
    }
}
