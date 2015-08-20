using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using DataReleaseJob = ReMi.DataEntities.ReleasePlan.ReleaseJob;
using ReleaseJob = ReMi.BusinessEntities.DeploymentTool.ReleaseJob;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseJobGateway : BaseGateway, IReleaseJobGateway
    {
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<DataReleaseJob> ReleaseJobRepository { get; set; }
        public IRepository<ReleaseDeploymentMeasurement> ReleaseDeploymentMeasurementRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public ReleaseJob GetReleaseJob(Guid releaseWindowId, Guid jobId)
        {
            var releaseJob = ReleaseJobRepository
                .GetSatisfiedBy(o => o.ReleaseWindow.ExternalId == releaseWindowId && o.JobId == jobId);

            if (releaseJob == null)
                return null;

            return MappingEngine.Map<DataReleaseJob, ReleaseJob>(releaseJob);
        }

        public IEnumerable<ReleaseJob> GetReleaseJobs(Guid releaseWindowId, bool getLastBuildNumber = false)
        {
            var items = ReleaseJobRepository
                .GetAllSatisfiedBy(o => o.ReleaseWindow.ExternalId == releaseWindowId)
                .ToList();

            var result = MappingEngine.Map<IEnumerable<DataReleaseJob>, IEnumerable<ReleaseJob>>(items).ToArray();
            if (!getLastBuildNumber) return result;

            var jobNames = items.Select(x => x.Name);
            var lastBuildJobs = ReleaseDeploymentMeasurementRepository.Entities
                .Where(x => !x.ParentMeasurementId.HasValue && x.BuildNumber.HasValue)
                .Where(x => jobNames.Any(n => x.StepName == n))
                .ToArray()
                .GroupBy(x => x.StepName)
                .Select(x => new { Name = x.Key, Number = x.Max(t => t.BuildNumber)})
                .ToArray();

            result.Each(x =>
            {
                var build = lastBuildJobs.FirstOrDefault(b => b.Name == x.Name);
                if (build != null) x.LastBuildNumber = build.Number;
            });
            return result;
        }

        public void AddJobToRelease(ReleaseJob job, Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            var dbReleaseJob = ReleaseJobRepository.GetSatisfiedBy(
                    o => o.JobId == job.JobId && o.ReleaseWindowId == releaseWindow.ReleaseWindowId);
            if (dbReleaseJob != null)
                throw new ReleaseJobDuplicationException(dbReleaseJob.ReleaseJobId, releaseWindow.ExternalId);

            var newReleaseJob = MappingEngine.Map<ReleaseJob, DataReleaseJob>(job);
            newReleaseJob.ReleaseWindowId = releaseWindow.ReleaseWindowId;
            ReleaseJobRepository.Insert(newReleaseJob);
        }

        public void RemoveJobFromRelease(Guid jobId, Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            var dbReleaseJob = ReleaseJobRepository.GetSatisfiedBy(
                    o => o.JobId == jobId && o.ReleaseWindowId == releaseWindow.ReleaseWindowId);
            if (dbReleaseJob == null)
                throw new EntityNotFoundException(typeof(DataReleaseJob), string.Format("[JobId={0}, ReleaseWindowId={1}]", jobId, releaseWindowId));

            ReleaseJobRepository.Delete(dbReleaseJob);
        }

        public override void OnDisposing()
        {
            ReleaseJobRepository.Dispose();
            ReleaseWindowRepository.Dispose();

            base.OnDisposing();
        }
    }
}
