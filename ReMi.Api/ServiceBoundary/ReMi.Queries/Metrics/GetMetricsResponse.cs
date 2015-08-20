using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Metrics;

namespace ReMi.Queries.Metrics
{
    public class GetMetricsResponse
    {
        public List<Metric> Metrics { get; set; }

        public override string ToString()
        {
            return String.Format("[Metrics={0}]", Metrics);
        }
    }
}
