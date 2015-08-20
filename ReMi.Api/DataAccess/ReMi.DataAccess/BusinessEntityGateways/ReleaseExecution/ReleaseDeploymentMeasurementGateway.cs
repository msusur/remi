using AutoMapper;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.DeploymentTool;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution
{
    public class ReleaseDeploymentMeasurementGateway : BaseGateway, IReleaseDeploymentMeasurementGateway
    {
        public IRepository<ReleaseDeploymentMeasurement> ReleaseDeploymentMeasurementRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }

        public IMappingEngine MappingEngine { get; set; }

        public IEnumerable<JobMeasurement> GetDeploymentMeasurements(Guid releaseWindowId)
        {
            var measurements =
                ReleaseDeploymentMeasurementRepository.GetAllSatisfiedBy(o => o.ReleaseWindow.ExternalId == releaseWindowId).ToList();

            return ProcessMeasurements(measurements).ToList();
        }

        private IEnumerable<JobMeasurement> ProcessMeasurements(IEnumerable<ReleaseDeploymentMeasurement> measurements, int? parentId = null)
        {
            var jobMeasurements = measurements as IList<ReleaseDeploymentMeasurement> ?? measurements.ToList();
            var levelItems = jobMeasurements.Where(o => o.ParentMeasurementId == parentId).ToList();

            foreach (var levelItem in levelItems)
            {
                var step = MappingEngine.Map<ReleaseDeploymentMeasurement, JobMeasurement>(levelItem);
                
                step.ChildSteps = ProcessMeasurements(jobMeasurements, levelItem.ReleaseDeploymentMeasurementId).ToList();

                yield return step;
            }
        }

        public void StoreDeploymentMeasurements(IEnumerable<JobMeasurement> measurements, Guid releaseWindowId, Guid accountId)
        {
            if (measurements == null)
                throw new ArgumentNullException("measurements");

            var jobMeasurements = measurements.ToList();
            if (!jobMeasurements.Any())
                return;

            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var account = AccountRepository.GetSatisfiedBy(o => o.ExternalId == accountId);
            if (account == null)
                throw new AccountNotFoundException(accountId);

            var existingMeasurements = ReleaseDeploymentMeasurementRepository.GetAllSatisfiedBy(o => o.ReleaseWindowId == releaseWindow.ReleaseWindowId);
            if (existingMeasurements.Any())
                throw new DeploymentJobMeasurementAlreadyExists(releaseWindow.ExternalId);

            InsertMeasurements(jobMeasurements, releaseWindow.ReleaseWindowId, account.AccountId);
        }

        public void RemoveDeploymentMeasurements(Guid releaseWindowId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);
            var toRemove = ReleaseDeploymentMeasurementRepository.Entities
                .Where(x => x.ReleaseWindowId == releaseWindow.ReleaseWindowId).ToArray();
            ReleaseDeploymentMeasurementRepository.Delete(toRemove);
        }

        private ReleaseWindow GetReleaseWindow(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            return releaseWindow;
        }

        private void InsertMeasurements(IEnumerable<JobMeasurement> measurements, int releaseWindowId, int accountId, int parentId = 0)
        {
            if (measurements == null)
                return;

            var stepMeasurements = measurements.ToList();
            foreach (var stepMeasurement in stepMeasurements)
            {
                if(stepMeasurement == null)
                    continue;

                var dbRecord = new ReleaseDeploymentMeasurement
                {
                    ParentMeasurementId = parentId > 0 ? parentId : (int?)null,

                    StepName = stepMeasurement.StepName,
                    StepId = stepMeasurement.StepId,
                    Locator = stepMeasurement.Locator,
                    StartTime = stepMeasurement.StartTime.ToMssqlRangeDateTime(),
                    FinishTime = stepMeasurement.FinishTime.ToMssqlRangeDateTime(),
                    ReleaseWindowId = releaseWindowId,
                    BuildNumber = stepMeasurement.BuildNumber,
                    NumberOfTries = stepMeasurement.NumberOfTries,
                    CreatedOn = SystemTime.Now,
                    CreatedByAccountId = accountId,
                };

                ReleaseDeploymentMeasurementRepository.Insert(dbRecord);

                InsertMeasurements(stepMeasurement.ChildSteps, releaseWindowId, accountId, dbRecord.ReleaseDeploymentMeasurementId);
            }
        }

        public override void OnDisposing()
        {
            ReleaseDeploymentMeasurementRepository.Dispose();
            AccountRepository.Dispose();
            ReleaseWindowRepository.Dispose();

            MappingEngine.Dispose();

            base.OnDisposing();
        }
    }
}
