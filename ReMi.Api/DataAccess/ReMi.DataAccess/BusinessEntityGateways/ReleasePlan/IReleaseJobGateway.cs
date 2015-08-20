using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.DeploymentTool;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseJobGateway : IDisposable
    {
        ReleaseJob GetReleaseJob(Guid releaseWindowId, Guid jobId);
        IEnumerable<ReleaseJob> GetReleaseJobs(Guid releaseWindowId, bool getLastBuildNumber = false);
        void AddJobToRelease(ReleaseJob job, Guid releaseWindowId);
        void RemoveJobFromRelease(Guid jobId, Guid releaseWindowId);
    }
}
