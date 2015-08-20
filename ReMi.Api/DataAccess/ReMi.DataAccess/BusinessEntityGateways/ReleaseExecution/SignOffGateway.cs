using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.ReleaseExecution;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ReleaseCalendar;
using DataSignOff = ReMi.DataEntities.ReleaseExecution.SignOff;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution
{
    public class SignOffGateway : BaseGateway, ISignOffGateway
    {
        public IRepository<DataSignOff> SignOffRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Metric> MetricRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public void SignOff(Guid accountId, Guid releaseWindowId, String description)
        {
            var signOff = SignOffRepository.GetSatisfiedBy(s => 
                s.Account.ExternalId == accountId
                && s.ReleaseWindow.ExternalId == releaseWindowId);

            if (signOff == null)
            {
                throw new SignOffNotFoundException(accountId);
            }

            signOff.SignedOff = SystemTime.Now;
            signOff.Comment = description;

            SignOffRepository.Update(signOff);
        }

        public void RemoveSigner(Guid signOffId)
        {
            var signOff = SignOffRepository.GetSatisfiedBy(s => s.ExternalId == signOffId);

            if (signOff == null)
            {
                throw new SignOffNotFoundException(signOffId);
            }
            
            SignOffRepository.Delete(signOff);
        }

        public void AddSigners(List<SignOff> signOffs, Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(r => r.ExternalId == releaseWindowId);

            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            var accountIds = signOffs.Select(s => s.Signer.ExternalId).ToList();
            var signOffsToAdd =
                AccountRepository.GetAllSatisfiedBy(a => accountIds.Contains(a.ExternalId))
                    .Join(signOffs, a => a.ExternalId, s => s.Signer.ExternalId,
                        (a, s) =>
                            new DataSignOff
                            {
                                AccountId = a.AccountId,
                                ReleaseWindowId = releaseWindow.ReleaseWindowId,
                                ExternalId = s.ExternalId
                            })
                    .ToList();

            SignOffRepository.Insert(signOffsToAdd);
        }

        public List<SignOff> GetSignOffs(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(r => r.ExternalId == releaseWindowId);

            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            var signOffs =
                SignOffRepository.GetAllSatisfiedBy(s => s.ReleaseWindow.ExternalId == releaseWindowId).ToList();

            return Mapper.Map<List<DataSignOff>, List<SignOff>>(signOffs);
        }

        public bool CheckSigningOff(Guid releaseWindowId)
        {
            var signOffs =
                SignOffRepository.GetAllSatisfiedBy(s => s.ReleaseWindow.ExternalId == releaseWindowId);

            var checkForSignOff = signOffs.All(s => s.SignedOff.HasValue);
            if (checkForSignOff)
            {
                var metric = MetricRepository.GetSatisfiedBy(x => x.ReleaseWindow.ExternalId == releaseWindowId && x.MetricType == MetricType.SignOff);
                if (metric != null)
                {
                    metric.ExecutedOn = SystemTime.Now;

                    MetricRepository.Update(metric);
                }
                else
                {
                    var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(r => r.ExternalId == releaseWindowId);
                    if (releaseWindow == null)
                    {
                        throw new ReleaseWindowNotFoundException(releaseWindowId);
                    }

                    MetricRepository.Insert(new Metric
                    {
                        ExecutedOn = SystemTime.Now,
                        ExternalId = Guid.NewGuid(),
                        MetricType = MetricType.SignOff,
                        ReleaseWindowId = releaseWindow.ReleaseWindowId
                    });
                }
            }

            return checkForSignOff;
        }
    }
}
