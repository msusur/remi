using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.DeploymentTool;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution
{
    public interface IReleaseDeploymentMeasurementGateway : IDisposable
    {
        IEnumerable<JobMeasurement> GetDeploymentMeasurements(Guid releaseWindowId);
        void StoreDeploymentMeasurements(IEnumerable<JobMeasurement> measurement, Guid releaseWindowId, Guid accountId);
        void RemoveDeploymentMeasurements(Guid releaseWindowId);
    }
}
