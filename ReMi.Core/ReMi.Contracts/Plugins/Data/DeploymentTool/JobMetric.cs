using System;
using System.Collections.Generic;

namespace ReMi.Contracts.Plugins.Data.DeploymentTool
{
    public class JobMetric
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? BuildNumber { get; set; }
        public int NumberOfTries { get; set; }
        public string Url { get; set; }
        public string JobId { get; set; }

        public IEnumerable<JobMetric> ChildMetrics { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, StartTime={1}, EndTime={2}, BuildNumber={3}, NumberOfTries={4}, Url={5}, ChildMetrics={6}, JobId={7}]",
                Name, StartTime, EndTime, BuildNumber, NumberOfTries, Url, ChildMetrics, JobId);
        }
    }
}
