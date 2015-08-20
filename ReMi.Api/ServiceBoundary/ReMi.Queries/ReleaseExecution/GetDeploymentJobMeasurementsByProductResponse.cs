using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Metrics;

namespace ReMi.Queries.ReleaseExecution
{
    public class GetDeploymentJobMeasurementsByProductResponse
    {
        public IEnumerable<Measurement> Measurements { get; set; }

        public override string ToString()
        {
            return String.Format("[Measurements={0}]", Measurements);
        }
    }
}
