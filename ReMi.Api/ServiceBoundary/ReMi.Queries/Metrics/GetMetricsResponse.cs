using ReMi.BusinessEntities.Metrics;
using System.Collections.Generic;

namespace ReMi.Queries.Metrics
{
    public class GetMetricsResponse
    {
        public IEnumerable<Metric> Metrics { get; set; }
        public bool AutomaticDeployTime { get; set; }

        public override string ToString()
        {
            return string.Format("[Metrics={0}, AutomaticDeployTime={1}]",
                Metrics, AutomaticDeployTime);
        }
    }
}
