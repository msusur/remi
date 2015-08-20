using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.DeploymentTool;

namespace ReMi.Queries.ReleaseExecution
{
    public class GetDeploymentMeasurementsResponse
    {
        public IEnumerable<JobMeasurement> Measurements { get; set; }

        public override string ToString()
        {
            return String.Format("[Measurements={0}]", Measurements);
        }
    }
}
